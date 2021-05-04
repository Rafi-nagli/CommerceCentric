using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ListingDataForReadSyncParser : BaseParser
    {
        public ListingDataForReadSyncParser() : base()
        {
        }

        public override ILineParser GetLineParser(ILogService log, 
            MarketType market, 
            string marketplaceId)
        {            
            return new ListingLineParser(log);
        }


        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            using (var uow = new UnitOfWork(Log))
            {
                uow.DisableValidation();

                var items = reportItems.Cast<ItemDTO>().ToList();

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
                ProcessItems(uow, api, time, items);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db,
            IMarketApi api,
            ITime time,
            IList<ItemDTO> listings)
        {
            var syncInfo = Context.SyncInformer;

            Log.Debug("ProcessItems begin");

            var allDbListings = db.Listings.GetAll().Where(l => l.Market == (int)api.Market
                                                                && (l.MarketplaceId == api.MarketplaceId ||
                                                                 String.IsNullOrEmpty(api.MarketplaceId)))
                                                                 .ToList();

            var allDbItems = db.Items.GetAll().Where(l => l.Market == (int)api.Market
                                                                && (l.MarketplaceId == api.MarketplaceId ||
                                                                 String.IsNullOrEmpty(api.MarketplaceId)))
                                                                 .ToList();


            var updateItemIds = new List<long>();
            var existsListingSKUs = new List<string>();
            //STEP 1. Update listings ASINs
            Log.Debug("Update ASINs begin");
            foreach (var listing in listings)
            {
                var dbListing = allDbListings.FirstOrDefault(l => l.SKU == listing.SKU);
                if (dbListing != null)
                {
                    existsListingSKUs.Add(listing.SKU);

                    if (dbListing.ListingId != listing.ListingId)
                    {
                        dbListing.ListingId = listing.ListingId;
                    }
                    if (dbListing.ASIN != listing.ASIN)
                    {
                        dbListing.ASIN = listing.ASIN;
                    }

                    if (dbListing.AmazonRealQuantity != listing.RealQuantity)
                    {
                        Log.Debug("Price changed: " + dbListing.SKU + ": " + dbListing.AmazonRealQuantity + "=>" + listing.RealQuantity);
                        dbListing.AmazonRealQuantity = listing.RealQuantity;
                        dbListing.AmazonRealQuantityUpdateDate = time.GetAppNowTime();
                    }
                    if (dbListing.AmazonCurrentPrice != listing.CurrentPrice)
                    {
                        Log.Debug("Price changed: " + dbListing.SKU + ": " + dbListing.AmazonCurrentPrice + "=>" + listing.CurrentPrice);
                        dbListing.AmazonCurrentPrice = listing.CurrentPrice;
                        dbListing.AmazonCurrentPriceUpdateDate = time.GetAppNowTime();
                    }

                    var dbItem = allDbItems.FirstOrDefault(i => i.Id == dbListing.ItemId);
                    if (dbItem != null)
                    {
                        if (dbItem.ASIN != listing.ASIN)
                        {
                            Log.Debug("Item ASIN changed: " + dbItem.Id + ": " + dbItem.ASIN + " => " + listing.ASIN);
                            dbItem.ASIN = listing.ASIN;
                        }
                        if (dbItem.SourceMarketId != listing.ASIN)
                        {
                            dbItem.SourceMarketId = listing.ASIN;
                        }
                        if (dbItem.ItemPublishedStatusFromMarket != listing.PublishedStatus)
                        {
                            dbItem.ItemPublishedStatusFromMarket = listing.PublishedStatus;
                            dbItem.ItemPublishedStatusFromMarketDate = time.GetAppNowTime();
                        }
                        if (listing.PublishedStatus == (int)PublishedStatuses.Published)
                        {
                            dbItem.ItemPublishedStatusBeforeRepublishing = dbItem.ItemPublishedStatus;
                            dbItem.ItemPublishedStatus = listing.PublishedStatus;
                            dbItem.ItemPublishedStatusReason = "Listings report";
                            dbItem.ItemPublishedStatusDate = time.GetAppNowTime();
                        }

                        updateItemIds.Add(dbItem.Id);
                    }
                }
            }

            db.Commit();
            Log.Debug("Update ASINs end");

            //STEP 1.2. Market all not exists as unpublished
            Log.Debug("Update items Unpublished begin");
            var notUpdatedItems = allDbItems.Where(i => !updateItemIds.Contains(i.Id)).ToList();
            Log.Debug("Not updated items: " + notUpdatedItems.Count());
            foreach (var dbItem in notUpdatedItems)
            {
                if (dbItem.ItemPublishedStatus == (int)PublishedStatuses.Published)
                {
                    Log.Debug("Status changes for: " + dbItem.ASIN + " - " + dbItem.ItemPublishedStatus + "=>" + PublishedStatuses.New);
                    dbItem.ItemPublishedStatusBeforeRepublishing = dbItem.ItemPublishedStatus;
                    dbItem.ItemPublishedStatusReason = "System Warning: the listing has the Published status, but it does not appear in the listing report.";
                    dbItem.ItemPublishedStatusDate = time.GetAppNowTime();
                    dbItem.ItemPublishedStatus = (int)PublishedStatuses.New;
                    dbItem.IsAmazonParentASIN = false;
                    dbItem.IsExistOnAmazon = false;
                }
            }
            db.Commit();

            Log.Debug("Update items Unpublished end");

            
            //STEP 2. Update ParentASINs
            Log.Debug("Update ParentASINs begin");
            var newListingsWithError = new List<ItemDTO>();
            try
            {
                api.FillWithAdditionalInfo(Log,
                    time,
                    listings,
                    IdType.SKU,
                    ItemFillMode.Defualt,
                    out newListingsWithError);
            }
            catch (Exception ex) //Can continue if only part of records was filled
            {
                syncInfo.AddError("", "Can't fill new listing items with additional info", ex);
                Log.Error("Can't fill new listing items with additional info", ex);
            }

            var allDbParents = db.ParentItems.GetAll().Where(pi => pi.Market == (int)api.Market
                                                                   && (pi.MarketplaceId == api.MarketplaceId ||
                                                                    String.IsNullOrEmpty(api.MarketplaceId))).ToList();

            var updatedParentIds = new List<long>();
            foreach (var listing in listings)
            {
                var dbListing = allDbListings.FirstOrDefault(l => l.SKU == listing.SKU);
                var dbItem = dbListing == null ? null : allDbItems.FirstOrDefault(i => i.Id == dbListing.ItemId);
                var dbParentItem = dbItem == null ? null : allDbParents.FirstOrDefault(pi => pi.ASIN == dbItem.ParentASIN);

                if (dbItem != null
                    && dbItem.IsExistOnAmazon != listing.IsExistOnAmazon)
                {
                    Log.Debug("IsExistOnAmazon: " + listing.ASIN + ": " + dbItem.IsExistOnAmazon + "=>" + listing.IsExistOnAmazon);
                    dbItem.IsExistOnAmazon = listing.IsExistOnAmazon;
                }

                var parentASIN = listing.ParentASIN;
                if (String.IsNullOrEmpty(parentASIN))
                    parentASIN = listing.ASIN;

                if (dbItem != null
                    && dbItem.IsExistOnAmazon == true)
                {
                    dbItem.IsAmazonParentASIN = !String.IsNullOrEmpty(listing.ParentASIN);
                    dbItem.LastUpdateFromAmazon = time.GetUtcTime();
                }

                if (dbParentItem != null
                    && listing.IsExistOnAmazon == true)
                {
                    var allItemsForParentItem = allDbItems.Where(i => i.ParentASIN == dbParentItem.ASIN).ToList();

                    if (dbParentItem.ASIN != parentASIN)
                    {
                        Log.Debug("ParentItem ASIN: " + dbParentItem.ASIN + "=>" + parentASIN);
                        dbParentItem.ASIN = parentASIN;

                        allItemsForParentItem.ForEach(i => i.ParentASIN = parentASIN); //UPDATE for all, in case it may have different Parents on Amazon (us last one for all)
                    }

                    
                    if (listing.ParentASIN != dbParentItem.ASIN)
                        dbParentItem.IsAmazonUpdated = false;
                    else
                        dbParentItem.IsAmazonUpdated = listing.IsExistOnAmazon;
                }
            }
            db.Commit();
            Log.Debug("Update ParentASINs end");

            //2.1 Update not exist parent asins
            var notUpdatedParents = allDbParents.Where(pi => !updatedParentIds.Contains(pi.Id)).ToList();
            Log.Debug("Not exist parent items: " + notUpdatedParents.Count());
            foreach (var dbParentItem in notUpdatedParents)
            {
                Log.Debug("Mark as not processed: " + dbParentItem.ASIN);
                dbParentItem.IsAmazonUpdated = false;
            }
            db.Commit();

            //3.0 Set to inactive not exist listings
            var notExistListings = listings.Where(l => !existsListingSKUs.Contains(l.SKU)).ToList();
            foreach (var notExistListing in notExistListings)
            {
                Log.Debug("Request qty=0 for SKU=" + notExistListing.SKU);
                if (Context.ActionService != null)
                    Context.ActionService.AddAction(db,
                        SystemActionType.UpdateOnMarketProductQuantity,
                        notExistListing.SKU,
                        new UpdateQtyInput()
                        {
                            Market = api.Market,
                            MarketplaceId = api.MarketplaceId,

                            ListingId = null,
                            SKU = notExistListing.SKU,
                            SourceMarketId = notExistListing.SourceMarketId,

                            NewQty = 0,
                        },
                        null,
                        null);
            }

            Log.Debug("ProcessItems end");
        }
    }
}
