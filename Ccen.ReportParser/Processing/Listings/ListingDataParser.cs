using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ListingDataParser : BaseParser
    {
        private bool _isProcessInactive;
        private bool _canCreateStyleInfo;

        public ListingDataParser(bool isProcessInactive, bool canCreateStyleInfo)
            : base()
        {
            _isProcessInactive = isProcessInactive;
            _canCreateStyleInfo = canCreateStyleInfo;
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            return new ListingLineParser(log);
        }


        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            var listingProcessor = new ListingLineProcessing(Context, time, _canCreateStyleInfo);

            using (var uow = new UnitOfWork(Log))
            {
                uow.DisableValidation();

                var items = reportItems.Cast<ItemDTO>().ToList();

                if (!_isProcessInactive)
                    items = items.Where(i => i.PublishedStatus != (int) PublishedStatuses.PublishedInactive).ToList();

                foreach (var item in items)
                {
                    item.MarketplaceId = api.MarketplaceId;
                    item.Market = (int) api.Market;
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
                ProcessItems(uow, api, time, listingProcessor, items, reportInfo.WasModified);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db,
            IMarketApi api,
            ITime time,
            ListingLineProcessing listingProcessor,
            IList<ItemDTO> reportListings,
            bool itemsWasModified)
        {
            Log.Debug("ProcessItems begin");
            var syncInfo = Context.SyncInformer;

            //Exclude standalone listings
            Log.Debug("Before exclude standalone listings: " + reportListings.Count());
            var stanaloneSKUs = reportListings.Where(l => !String.IsNullOrEmpty(l.ASIN)
                    && StringHelper.ContainsNoCase(l.SKU, "-" + l.ASIN))
                .Select(l => l.SKU)
                .ToList();
            Log.Debug("Standalone SKUs: " + stanaloneSKUs.Count());
            Log.Debug("SKUs: " + String.Join(", ", stanaloneSKUs));
            reportListings = reportListings.Where(l => !stanaloneSKUs.Contains(l.SKU)).ToList();

            var publishingInProgressListings = (from i in db.Items.GetAll()
                                                join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                                where l.Market == (int)api.Market
                                                    && (l.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                                                    && (i.ItemPublishedStatus == (int)PublishedStatuses.New
                                                     || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                                     || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                                     || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors)
                                                    && !l.IsRemoved
                                                select new
                                                {
                                                    SKU = l.SKU,
                                                    ListingId = l.ListingId
                                                }).ToList();
            var publishingInProgressListingIds = publishingInProgressListings.Select(l => l.ListingId).ToList();
            var publishingInProgressSKUs = publishingInProgressListings.Select(l => l.SKU).ToList();

            #region Step 1. Mark all except parsing as removed

            if (!itemsWasModified)
            {
                //TODO: Temp removed new listings
                //var excludeFromRemoveListingIds = reportListings.Select(r => r.ListingId).ToList();
                //excludeFromRemoveListingIds.AddRange(publishingInProgressListingIds);
                //var removedList = db.Listings.MarkNotExistingAsRemoved(excludeFromRemoveListingIds, api.Market, api.MarketplaceId);
                //foreach (var removedItem in removedList)
                //    Log.Debug("Mark as removed, listingId=" + removedItem);
            }

            #endregion

            #region Process new report listings
            //Get all not exist in DB
            //var newReportListings = db.Listings.CheckForExistence(reportListings, api.Market, api.MarketplaceId);
            //var existListingId = db.Listings.GetFiltered(l => !l.IsRemoved
            //    && l.Market == (int)api.Market
            //    && l.MarketplaceId == api.MarketplaceId)
            //    .Select(l => l.ListingId)
            //    .ToList();
            var existListingSKUs = db.Listings.GetFiltered(l => !l.IsRemoved
                && l.Market == (int)api.Market
                && l.MarketplaceId == api.MarketplaceId)
                .Select(l => l.SKU)
                .ToList();
            var newReportListings = reportListings.Where(r => !existListingSKUs.Contains(r.SKU)).ToList();
            newReportListings = newReportListings.Where(l => !publishingInProgressSKUs.Contains(l.SKU)).ToList();

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
                }
                catch (Exception ex) //Can continue if only part of records was filled
                {
                    syncInfo.AddError("", "Can't fill new listing items with additional info", ex);
                    Log.Error("Can't fill new listing items with additional info", ex);
                }
                Log.Debug("Error when GetItems for new listings, asins: " + String.Join(", ", newListingsWithError.Select(i => i.ASIN).ToList()));

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
            #endregion

            #region Step 3. Process existing listings
            var existingListings = reportListings.Where(r => !newReportListings.Select(i => i.SKU).Contains(r.SKU)).ToList();
            existingListings = existingListings.Where(l => !publishingInProgressSKUs.Contains(l.SKU)).ToList();

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
                }
                catch (Exception ex)
                {
                    syncInfo.AddError("", "Can't fill exist items with additional info", ex);
                    Log.Error("Can't fill exist items with additional info", ex);
                }

                Log.Debug("Error when GetItems for existing listings, asins: " + String.Join(", ", existingListingsWithError.Select(i => i.ASIN).ToList()));

                //1. Process Items (with remap ParentASIN)
                listingProcessor.ProcessExistingItems(db, api, existingListings);

                //2. Process ParentItems, and creating styles based on there names
                //Need in some rare cases when items have empty Parent Item, ex.: parent item asin was changed but not for all product items
                var parentsASINs = existingListings.Where(l => !String.IsNullOrEmpty(l.ParentASIN)).ToList();
                var newParents = db.ParentItems.CheckForExistence(parentsASINs, api.Market, api.MarketplaceId);
                if (newParents.Any())
                {
                    listingsWithNewParents.AddRange(newParents);
                    listingProcessor.ProcessNewParents(db, api, newParents, existingListings);
                }
            }

            var processedParentASINs = listingsWithNewParents.Select(l => l.ParentASIN).ToList();
            var existParents = reportListings.Where(r => !processedParentASINs.Contains(r.ParentASIN)
                && !String.IsNullOrEmpty(r.ParentASIN)).ToList();
            if (existParents.Any())
            {
                listingProcessor.ProcessExistingParents(db,
                    api,
                    syncInfo,
                    existParents.Select(p => p.ParentASIN).Distinct().ToList(),
                    reportListings);
            }
            #endregion

            Log.Debug("ProcessItems end");
        }
    }
}
