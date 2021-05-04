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
    //public class ListingUpdateParser : BaseParser
    //{
    //    public ListingUpdateParser()
    //        : base()
    //    {
    //    }

    //    public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
    //    {
    //        return new ListingLineParser(log);
    //    }


    //    public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
    //    {
    //        using (var uow = new UnitOfWork())
    //        {
    //            uow.DisableValidation();

    //            var items = reportItems.Cast<ItemDTO>().ToList();

    //            foreach (var item in items)
    //            {
    //                item.MarketplaceId = api.MarketplaceId;
    //                item.Market = (int) api.Market;
    //                item.CurrentPriceCurrency = "USD";
    //                if (api.MarketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
    //                    item.CurrentPriceCurrency = "CAD";
    //                if (api.MarketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
    //                    item.CurrentPriceCurrency = "GBP";
    //                if (api.MarketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
    //                    item.CurrentPriceCurrency = "MXN";
    //                item.CurrentPriceInUSD = PriceHelper.RougeConvertToUSD(item.CurrentPriceCurrency, item.CurrentPrice);
    //            }

    //            Log.Debug("Begin process items");
    //            ProcessItems(uow, api, time, items);
    //            Log.Debug("End process items");
    //        }
    //    }

    //    private void ProcessItems(IUnitOfWork db, 
    //        IMarketApi api, 
    //        ITime time, 
    //        IList<ItemDTO> listings)
    //    {
    //        var syncInfo = Context.SyncInformer;

    //        Log.Debug("ProcessItems begin");

    //        var allDbItems = db.Items.GetAll().Where(l => l.Market == (int)api.Market
    //                                                            && (l.MarketplaceId == api.MarketplaceId ||
    //                                                                String.IsNullOrEmpty(api.MarketplaceId))
    //                                                            //TODO: NOTE why those restriction here?
    //                                                            //&& (l.ItemPublishedStatus == (int)PublishedStatuses.New
    //                                                            //    || l.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors)
    //                                                                )
    //                                                           .ToList();

    //        var dbItemIds = allDbItems.Select(i => i.Id).ToList();
    //        var allDbListings = db.Listings.GetAll().Where(l => dbItemIds.Contains(l.ItemId)).ToList();

    //        Log.Debug("Items to update: " + allDbItems.Count());

    //        //STEP 1. Update listings ASINs
    //        Log.Debug("Update ASINs begin");
    //        foreach (var listing in listings)
    //        {
    //            var dbListing = allDbListings.FirstOrDefault(l => l.SKU == listing.SKU);
    //            if (dbListing != null)
    //            {
    //                if (dbListing.ListingId != listing.ListingId)
    //                {
    //                    dbListing.ListingId = listing.ListingId;
    //                }
    //                if (dbListing.ASIN != listing.ASIN)
    //                {
    //                    dbListing.ASIN = listing.ASIN;
    //                }

    //                if (dbListing.AmazonRealQuantity != listing.RealQuantity)
    //                {
    //                    Log.Debug("Price changed: " + dbListing.SKU + ": " + dbListing.AmazonRealQuantity + "=>" + listing.RealQuantity);
    //                    dbListing.AmazonRealQuantity = listing.RealQuantity;
    //                    dbListing.AmazonRealQuantityUpdateDate = time.GetAppNowTime();
    //                }
    //                if (dbListing.AmazonCurrentPrice != listing.CurrentPrice)
    //                {
    //                    Log.Debug("Price changed: " + dbListing.SKU + ": " + dbListing.AmazonCurrentPrice + "=>" + listing.CurrentPrice);
    //                    dbListing.AmazonCurrentPrice = listing.CurrentPrice;
    //                    dbListing.AmazonCurrentPriceUpdateDate = time.GetAppNowTime();
    //                }

    //                var dbItem = allDbItems.FirstOrDefault(i => i.Id == dbListing.ItemId);
    //                if (dbItem != null)
    //                {
    //                    if (dbItem.ASIN != listing.ASIN)
    //                    {
    //                        Log.Debug("Item ASIN changed: " + dbItem.Id + ": " + dbItem.ASIN + " => " + listing.ASIN);
    //                        dbItem.ASIN = listing.ASIN;
    //                    }
    //                    if (dbItem.SourceMarketId != listing.ASIN)
    //                    {
    //                        dbItem.SourceMarketId = listing.ASIN;
    //                    }
    //                }
    //            }
    //        }

    //        db.Commit();
    //        Log.Debug("Update ASINs end");


    //        //STEP 2. Update ParentASINs
    //        Log.Debug("Update ParentASINs begin");
    //        var newListingsWithError = new List<ItemDTO>();
    //        try
    //        {
    //            api.FillWithAdditionalInfo(Log,
    //                time,
    //                listings,
    //                IdType.SKU,
    //                ItemFillMode.Defualt,
    //                out newListingsWithError);
    //        }
    //        catch (Exception ex) //Can continue if only part of records was filled
    //        {
    //            syncInfo.AddError("", "Can't fill new listing items with additional info", ex);
    //            Log.Error("Can't fill new listing items with additional info", ex);
    //        }
            
    //        var allDbParents = db.ParentItems.GetAll().Where(pi => pi.Market == (int) api.Market
    //                                                               && (pi.MarketplaceId == api.MarketplaceId ||
    //                                                                String.IsNullOrEmpty(api.MarketplaceId))).ToList();

    //        foreach (var listing in listings)
    //        {
    //            var dbListing = allDbListings.FirstOrDefault(l => l.SKU == listing.SKU);
    //            var dbItem = dbListing == null ? null : allDbItems.FirstOrDefault(i => i.Id == dbListing.ItemId);
    //            var dbParentItem = dbItem == null ? null : allDbParents.FirstOrDefault(pi => pi.ASIN == dbItem.ParentASIN);

    //            //NOTE: for case when first child changed parent ASIN value of ParentItem
    //            if (dbParentItem == null && !String.IsNullOrEmpty(listing.ParentASIN))
    //                dbParentItem = allDbParents.FirstOrDefault(pi => pi.ASIN == listing.ParentASIN);

    //            if (dbItem != null
    //                && dbItem.OnMarketTemplateName != listing.OnMarketTemplateName)
    //            {
    //                Log.Debug("Template changed: " + dbItem.OnMarketTemplateName + "=>" + listing.OnMarketTemplateName);
    //                dbItem.OnMarketTemplateName = listing.OnMarketTemplateName;
    //            }

    //            if (dbItem != null
    //                && dbItem.IsExistOnAmazon != listing.IsExistOnAmazon)
    //            {
    //                Log.Debug("IsExistOnAmazon: " + listing.ASIN + ": " + dbItem.IsExistOnAmazon + "=>" + listing.IsExistOnAmazon);
    //                dbItem.IsExistOnAmazon = listing.IsExistOnAmazon;                    
    //            }

    //            if (dbItem != null
    //                && dbItem.IsExistOnAmazon == true)
    //            {
    //                dbItem.IsAmazonParentASIN = !String.IsNullOrEmpty(listing.ParentASIN);
    //                dbItem.LastUpdateFromAmazon = time.GetUtcTime();
    //            }

    //            if (dbParentItem != null
    //                && !String.IsNullOrEmpty(listing.ParentASIN))
    //            {
    //                var parentASIN = listing.ParentASIN;
    //                if (String.IsNullOrEmpty(parentASIN))
    //                    parentASIN = listing.ASIN;

    //                if (listing.ParentASIN != dbParentItem.ASIN)
    //                {
    //                    Log.Debug("ParentItem ASIN: " + dbParentItem.ASIN + "=>" + listing.ParentASIN);
    //                    dbParentItem.ASIN = listing.ParentASIN;
    //                    dbParentItem.SourceMarketId = listing.ParentASIN;
    //                }

    //                if (dbItem != null && dbItem.ParentASIN != listing.ParentASIN)
    //                {
    //                    Log.Debug("Item ParentASIN: " + dbItem.ParentASIN + "=>" + listing.ParentASIN);
    //                    if (dbItem.IsExistOnAmazon != true) //NOTE: When update from SKU to ASIN
    //                    {
    //                        var notUpdatedItems = db.Items.GetAll().Where(i => i.ParentASIN == dbItem.ParentASIN
    //                            && i.Market == (int)dbItem.Market
    //                                && (i.MarketplaceId == dbItem.MarketplaceId ||
    //                                String.IsNullOrEmpty(dbItem.MarketplaceId))
    //                                && i.IsExistOnAmazon != true).ToList();
    //                        foreach (var item in notUpdatedItems)
    //                        {
    //                            Log.Debug("Change Related Item ParentASIN: " + dbItem.ParentASIN + "=>" + listing.ParentASIN);
    //                            item.ParentASIN = listing.ParentASIN;
    //                        }
    //                    }
    //                    dbItem.ParentASIN = listing.ParentASIN;
    //                }

    //                if (dbParentItem.IsAmazonUpdated != listing.IsExistOnAmazon)
    //                {
    //                    dbParentItem.IsAmazonUpdated = listing.IsExistOnAmazon;
    //                }                    

    //                var duplicateParentItems = db.ParentItems.GetAll().Where(i => i.ASIN == parentASIN
    //                    && i.Id != dbParentItem.Id
    //                    && i.Market == (int)api.Market
    //                    && i.MarketplaceId == api.MarketplaceId);
    //                foreach (var parentItem in duplicateParentItems)
    //                {
    //                    Log.Debug("Duplicate parentItem was removed: " + parentItem.Id + ", ASIN=" + parentItem.ASIN);
    //                    db.ParentItems.Remove(parentItem);
    //                }
    //                db.Commit();
    //            }
    //        }
    //        db.Commit();
    //        Log.Debug("Update ParentASINs end");

    //        Log.Debug("ProcessItems end");
    //    }
    //}
}
