using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ListingDataParserV2 : BaseParser
    {
        private bool _isProcessInactive;
        private bool _canCreateStyleInfo;
        private bool _enableFakeParentASIN;
        private DateTime? _removePeriodForPublishingInProgress;

        public ListingDataParserV2(bool isProcessInactive, 
            bool canCreateStyleInfo,
            bool enableFakeParentASIN,
            DateTime? removePeriodForPublishingInProgress)
            : base()
        {
            _isProcessInactive = isProcessInactive;
            _canCreateStyleInfo = canCreateStyleInfo;
            _enableFakeParentASIN = enableFakeParentASIN;
            _removePeriodForPublishingInProgress = removePeriodForPublishingInProgress;
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            return new ListingLineParser(log);
        }


        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            var listingProcessor = new ListingLineProcessing(Context, time, _canCreateStyleInfo);

            var items = reportItems.Cast<ItemDTO>().ToList();

            if (!_isProcessInactive)
                items = items.Where(i => i.PublishedStatus != (int)PublishedStatuses.PublishedInactive).ToList();

            //NOTE: Exclude all Rusty
            items = items.Where(i => !StringHelper.ContainsNoCase(i.Name, "Rusty")).ToList();

            foreach (var item in items)
            {
                item.MarketplaceId = api.MarketplaceId;
                item.Market = (int)api.Market;
                item.CurrentPriceCurrency = "USD";
                if (api.MarketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    item.CurrentPriceCurrency = "CAD";
                if (api.MarketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                    item.CurrentPriceCurrency = "GBP";
                if (api.MarketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    item.CurrentPriceCurrency = "MXN";
                if (api.MarketplaceId == MarketplaceKeeper.AmazonAuMarketplaceId)
                    item.CurrentPriceCurrency = "AUD";
                item.CurrentPriceInUSD = PriceHelper.RougeConvertToUSD(item.CurrentPriceCurrency, item.CurrentPrice);
            }

            Log.Debug("Begin process items");
            ProcessItems(api, time, listingProcessor, items, reportInfo.WasModified);
            Log.Debug("End process items");
        }

        private void ProcessItems(IMarketApi api,
            ITime time,
            ListingLineProcessing listingProcessor,
            IList<ItemDTO> reportListings,
            bool itemsWasModified)
        {
            Log.Debug("ProcessItems begin");
            var syncInfo = Context.SyncInformer;
            var dbFactory = Context.DbFactory;

            //Exclude standalone listings
            Log.Debug("Before exclude standalone listings: " + reportListings.Count());
            var stanaloneSKUs = reportListings.Where(l => !String.IsNullOrEmpty(l.ASIN)
                    && StringHelper.ContainsNoCase(l.SKU, "-" + l.ASIN))
                .Select(l => l.SKU)
                .ToList();
            Log.Debug("Standalone SKUs: " + stanaloneSKUs.Count());
            Log.Debug("SKUs: " + String.Join(", ", stanaloneSKUs));
            reportListings = reportListings.Where(l => !stanaloneSKUs.Contains(l.SKU)).ToList();

            IList<string> publishingInProgressListingIds = new List<string>();
            using (var db = dbFactory.GetRWDb())
            {
                var publishingPeriod = _removePeriodForPublishingInProgress;

                var publishingInProgressListings = (from i in db.Items.GetAll()
                                                    join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                                    where l.Market == (int)api.Market
                                                        && !l.IsRemoved
                                                        && (l.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                                                        && (i.ItemPublishedStatus == (int)PublishedStatuses.New
                                                         || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                                         || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges                                                             
                                                         || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress                                                         
                                                         || (i.ItemPublishedStatus == (int)PublishedStatuses.Published && !i.IsExistOnAmazon.HasValue))
                                                    select new
                                                    {
                                                        SKU = l.SKU,
                                                        ListingId = l.ListingId,
                                                        CreateDate = l.CreateDate
                                                    });

                if (publishingPeriod.HasValue)
                    publishingInProgressListings = publishingInProgressListings.Where(l => l.CreateDate > publishingPeriod);

                publishingInProgressListingIds = publishingInProgressListings.Select(l => l.ListingId).ToList();
            }

            #region Step 1. Mark all except parsing as removed

            if (!itemsWasModified)
            {                
                var excludeMarketListingIds = reportListings.Select(r => r.ListingId).ToList();
                excludeMarketListingIds.AddRange(publishingInProgressListingIds);

                using (var db = dbFactory.GetRWDb())
                {
                    var toUnbulishListingIds = (from l in db.Listings.GetAll()
                                               join i in db.Items.GetAll() on l.ItemId equals i.Id
                                               where !String.IsNullOrEmpty(l.ListingId)
                                                     && l.Market == (int)api.Market
                                                     && (l.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                                                     && !l.IsRemoved
                                                     && (i.ItemPublishedStatus == (int)PublishedStatuses.HasUnpublishRequest
                                                        || i.ItemPublishedStatus == (int)PublishedStatuses.Unpublished)
                                               select l.Id)
                                              .ToList();

                    var allMarketListings = (from l in db.Listings.GetAll()
                                               join i in db.Items.GetAll() on l.ItemId equals i.Id
                                               where !String.IsNullOrEmpty(l.ListingId)
                                                     && l.Market == (int)api.Market
                                                     && (l.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                                                     && !l.IsRemoved
                                                     //&& i.ItemPublishedStatus != (int)PublishedStatuses.HasChanges //NOTE: already going to republish
                                               select new
                                               {
                                                   l.Id,
                                                   l.ListingId,
                                                   ItemId = i.Id,
                                                   l.CreateDate
                                               })
                                              .ToList();

                    var missingOnMarketListingInfoList = allMarketListings
                        .Where(l => !excludeMarketListingIds.Contains(l.ListingId))
                        .Distinct()
                        .ToList();

                    foreach (var listingInfo in missingOnMarketListingInfoList)
                    {
                        if (toUnbulishListingIds.Contains(listingInfo.Id)
                            || (_removePeriodForPublishingInProgress.HasValue && listingInfo.CreateDate < _removePeriodForPublishingInProgress))
                        {
                            //db.Items.SetItemPublishingStatus(itemId, (int)PublishedStatuses.Unpublished, "Requested by user via UnpublishRequest", time.GetAmazonNowTime());
                            var dbListings = db.Listings.GetAll().Where(l => l.Id == listingInfo.Id).ToList();
                            foreach (var dbListing in dbListings)
                            {
                                dbListing.IsRemoved = true;
                                Log.Info("Listing, SKU: " + dbListing.SKU + " set as removed");
                            }                            
                        }
                        else
                        {
                            try
                            {
                                db.Items.SetItemPublishingStatus(listingInfo.ItemId, (int)PublishedStatuses.HasChanges, "System Warning: the listing has the Published status, but it does not appear in the listing report.", time.GetAppNowTime());
                            }
                            catch (Exception ex) //NOTE: usually update twice one Item
                            {
                                Log.Error("SetItemPublishingStatus error, itemId=" + listingInfo.ItemId, ex);
                            }
                            Log.Info("Item, id: " + listingInfo.ItemId + " set to republish");
                        }
                    }
                    db.Commit();

                    //db.Listings.MarkAsRemoved(toRemoveDbListingIds);
                    //TODO: change status to HasChanges to restore items
                    Log.Debug("Missing on marketplace items, count=" + missingOnMarketListingInfoList.Count());
                    Log.Debug("Mark to republish, ItemIds=" + String.Join(",", missingOnMarketListingInfoList.Select(i => i.ItemId)));
                }
            }

            #endregion

            #region Process new report listings
            IList<ItemDTO> newReportListings = new List<ItemDTO>();
            using (var db = dbFactory.GetRWDb())
            {
                var existListingSKUs = db.Listings.GetFiltered(l => !l.IsRemoved
                    && l.Market == (int)api.Market
                    && l.MarketplaceId == api.MarketplaceId)
                    .Select(l => l.SKU)
                    .ToList();
                newReportListings = reportListings.Where(r => !existListingSKUs.Contains(r.SKU)).ToList();

                foreach (var newListingItem in newReportListings)
                    Log.Debug("New listing, listingId=" + newListingItem.ListingId);

                var listingsWithNewParents = new List<ItemDTO>();
                if (newReportListings.Any())
                {
                    var newListingsWithError = new List<ItemDTO>();
                    //0. Get ParentASIN for listings (and other infos)
                    try
                    {
                        api.FillWithAdditionalInfo(Log,
                            time,
                            newReportListings,
                            IdType.SKU,
                            ItemFillMode.NoAdv, //NOTE: TODO: Request barcodes by separate service
                            out newListingsWithError);

                        if (_enableFakeParentASIN)
                        {
                            foreach (var l in newReportListings)
                            {
                                if (l.IsExistOnAmazon == true
                                    && String.IsNullOrEmpty(l.ParentASIN))
                                    l.ParentASIN = l.ASIN;
                            }
                        }
                    }
                    catch (Exception ex) //Can continue if only part of records was filled
                    {
                        syncInfo.AddError("", "Can't fill new listing items with additional info", ex);
                        Log.Error("Can't fill new listing items with additional info", ex);
                    }
                    Log.Debug("Listings with errors when GetItems for new listings, asins: " + String.Join(", ", newListingsWithError.Select(i => i.ASIN).ToList()));

                    //1. Process Items
                    listingProcessor.ProcessNewListingsWithItems(db, api, time, newReportListings);

                    //2. Process ParentItems, and creating styles based on there names
                    var listingsWithParentASIN = newReportListings.Where(l => !String.IsNullOrEmpty(l.ParentASIN)).ToList();
                    listingsWithNewParents = db.ParentItems.CheckForExistence(listingsWithParentASIN, api.Market, api.MarketplaceId);
                    if (listingsWithNewParents.Any())
                    {
                        Log.Debug("Process new parents");
                        listingProcessor.ProcessNewParents(db, api, listingsWithNewParents, newReportListings);
                    }
                }
            }
            #endregion

            #region Step 3. Process existing listings
            var existingListings = reportListings.Where(r => !newReportListings.Select(i => i.SKU).Contains(r.SKU)).ToList();

            if (existingListings.Any())
            {
                var existingListingsWithError = new List<ItemDTO>();
                //0. Get ParentASIN for listings (and other infos)

                //NOTE: Get base info. Should go after "get barcodes"
                try
                {
                    api.FillWithAdditionalInfo(Log,
                        time,
                        existingListings,
                        IdType.SKU,
                        ItemFillMode.NoAdv,
                        out existingListingsWithError);

                    if (_enableFakeParentASIN)
                    {
                        foreach (var l in existingListings)
                        {
                            if (l.IsExistOnAmazon == true
                                && String.IsNullOrEmpty(l.ParentASIN))
                                l.ParentASIN = l.ASIN;
                        }
                    }
                }
                catch (Exception ex)
                {
                    syncInfo.AddError("", "Can't fill exist items with additional info", ex);
                    Log.Error("Can't fill exist items with additional info", ex);
                }

                Log.Debug("Error when GetItems for existing listings, asins: " + String.Join(", ", existingListingsWithError.Select(i => i.ASIN).ToList()));

                //1. Process Items (keep Parent ASIN to keep source relationships)
                using (var db = dbFactory.GetRWDb())
                {
                    listingProcessor.ProcessExistingItems(db, api, existingListings);
                }
                
                //DISABLED: we don't creating new parents for existing listings, always keep ParentASINs (on CCEN we have required relationships, real ParentASIN may be others)

                //2. Process ParentItems, and creating styles based on there names
                //Need in some rare cases when items have empty Parent Item, ex.: parent item asin was changed but not for all product items
                var parentsASINs = existingListings.Where(l => !String.IsNullOrEmpty(l.ParentASIN)).ToList();

                using (var db = dbFactory.GetRWDb())
                {
                    var newParents = db.ParentItems.CheckForExistence(parentsASINs, api.Market, api.MarketplaceId);
                    if (newParents.Any())
                    {
                        listingProcessor.ProcessNewParents(db, api, newParents, existingListings);
                    }
                }
            }

            using (var db = dbFactory.GetRWDb())
            {
                listingProcessor.UpdateParentsForExistantItems(db,
                    api,
                    syncInfo);
            }

            #endregion

            Log.Debug("ProcessItems end");
        }
    }
}
