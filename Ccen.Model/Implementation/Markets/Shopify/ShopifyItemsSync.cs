using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Model.General;
using Shopify.Api;

namespace Amazon.Model.Implementation.Markets.Shopify
{
    public class ShopifyItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public ShopifyItemsSync(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void ReadItemsInfo(ShopifyApi api)
        {
            _log.Info("Begin ReadItemsInfo");
            var itemsWithError = new List<string>();
            var items = api.GetItems(_log,
                _time,
                null,
                ItemFillMode.Defualt,
                out itemsWithError);

            _log.Info("Getted items: " + items.Count());

            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();

                var allParentItems = db.ParentItems.GetAll()
                        .Where(m => m.Market == (int)api.Market
                                 && (m.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId)))
                        .Select(pi => new
                        {
                            SourceMarketId = pi.SourceMarketId,
                            ASIN = pi.ASIN
                        })
                        .ToList();

                var allDbItems = db.Items.GetAll()
                        .Where(m => m.Market == (int)api.Market
                                 && (m.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId)))
                        .ToList();

                var allDbListings = db.Listings.GetAll()
                            .Where(m => m.Market == (int)api.Market
                                 && (m.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId)))
                            .ToList();

                _log.Info("Exist items=" + allDbItems.Count());

                var recordsWithChanges = new List<long>();
                foreach (var item in items)
                {
                    var dbParentItem = allParentItems.FirstOrDefault(pi => pi.SourceMarketId == item.SourceMarketId);

                    if (dbParentItem == null)
                    {
                        _log.Info("Unable to find parent item for, marketItemId=" + item.SourceMarketId + ", ASIN=" + item.ASIN);
                        continue;
                    }
                    foreach (var variation in item.Variations)
                    {
                        _log.Info("Read info for SKU=" + variation.SKU + ", qty=" + variation.AmazonRealQuantity + ", price=" + variation.AmazonCurrentPrice);
                        var dbItem = allDbItems.FirstOrDefault(m => m.ParentASIN == dbParentItem.ASIN);
                        if (dbItem == null)
                        {
                            _log.Info("Unable to find item for, marketItemId=" + variation.SourceMarketId + ", SKU=" + variation.SKU);
                            continue;
                        }

                        var dbListing = allDbListings.FirstOrDefault(l => l.ItemId == dbItem.Id);
                        if (dbListing == null)
                        {
                            _log.Info("Unable to find listing for itemId=" + dbItem.Id);
                            continue;
                        }

                        if (dbItem.SourceMarketId != variation.SourceMarketId)
                        {
                            _log.Info("Item SourceMarketId changed: "
                                + dbItem.SourceMarketId + " -> " + variation.SourceMarketId);
                            dbItem.SourceMarketId = variation.SourceMarketId;
                            dbItem.ASIN = variation.SourceMarketId;
                        }
                        if (dbItem.ItemPublishedStatus != variation.PublishedStatus
                            && (variation.PublishedStatus == (int)PublishedStatuses.Unpublished
                                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.Published
                                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.Unpublished)
                            && dbItem.ItemPublishedStatus != (int)PublishedStatuses.New)
                        {
                            dbItem.ItemPublishedStatus = variation.PublishedStatus;
                            dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                            dbItem.ItemPublishedStatusReason = "Derived from Shopify";
                        }

                        if (dbListing.AmazonCurrentPrice != variation.AmazonCurrentPrice)
                        {
                            _log.Info("Changes: price: " + dbListing.AmazonCurrentPrice + "=>" + variation.AmazonCurrentPrice);

                            if (recordsWithChanges.All(r => r != dbListing.Id))
                                recordsWithChanges.Add(dbListing.Id);

                            dbListing.AmazonCurrentPrice = variation.AmazonCurrentPrice;
                            dbListing.AmazonCurrentPriceUpdateDate = _time.GetAppNowTime();
                        }

                        if (dbListing.AmazonRealQuantity != variation.AmazonRealQuantity)
                        {
                            _log.Info("Changes: qty: " + dbListing.AmazonRealQuantity + "=>" + variation.AmazonRealQuantity);

                            if (recordsWithChanges.All(r => r != dbListing.Id))
                                recordsWithChanges.Add(dbListing.Id);

                            dbListing.AmazonRealQuantity = variation.AmazonRealQuantity;
                            dbListing.AmazonRealQuantityUpdateDate = _time.GetAppNowTime();
                        }
                    }
                }
                _log.Info("Before commit");
                db.Commit();
                _log.Info("After commit");

                //Unpublish not exists items
                var existSourceMarketIds = items.SelectMany(i => i.Variations.Select(v => v.SourceMarketId).ToList()).ToList();
                var notExistDbItems = allDbItems.Where(i => !existSourceMarketIds.Contains(i.SourceMarketId)).ToList();
                foreach (var notExistItem in notExistDbItems)
                {
                    notExistItem.ItemPublishedStatus = (int)PublishedStatuses.Unpublished;
                    notExistItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                    notExistItem.ItemPublishedStatusReason = "Derived from Shopify";
                }
                db.Commit();
            }
            _log.Info("End ReadItemsInfo");
        }


        public void ProcessUnpublishedRequests(ShopifyApi api)
        {
            _log.Info("Begin ProcessUnpublishedRequests");
            var sleeper = new StepSleeper(TimeSpan.FromSeconds(2), 1);
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)api.Market
                    && (i.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                    && i.PublishedStatus == (int)PublishedStatuses.HasUnpublishRequest)
                    .ToList();

                var parentItemAsins = items.Select(i => i.ParentASIN).Distinct().ToList();
                var parentItemsToUnpublish = db.ParentItems.GetAllAsDto().Where(pi => pi.Market == (int)api.Market
                            && (pi.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                            && parentItemAsins.Contains(pi.ASIN))
                    .ToList();

                foreach (var parentItem in parentItemsToUnpublish)
                {
                    var itemId = StringHelper.TryGetLong(parentItem.SourceMarketId);
                    if (!itemId.HasValue)
                    {
                        _log.Error("Item has empty SourceMarketId");
                        continue;
                    }
                    sleeper.NextStep();
                    var result = api.UnpublishItem(itemId.Value);
                    if (result.IsSuccess)
                    {
                        _log.Info("ASIN was unpublished=" + parentItem.ASIN + ", sourceMarketId=" + parentItem.SourceMarketId);
                        var dbItemList = db.Items.GetAll().Where(i => i.ParentASIN == parentItem.ASIN
                            && i.Market == parentItem.Market
                            && (i.MarketplaceId == parentItem.MarketplaceId
                                || String.IsNullOrEmpty(parentItem.MarketplaceId)))
                             .ToList();
                        foreach (var dbItem in dbItemList)
                        {
                            _log.Info("Unpublished variation, id=" + dbItem.SourceMarketId);
                            dbItem.ItemPublishedStatus = (int)PublishedStatuses.Unpublished;
                            db.Commit();
                        }
                    }
                }
            }
            _log.Info("End ProcessUnpublishedRequests");
        }

        public void ProcessPublishedRequests(ShopifyApi api)
        {
            var sleeper = new StepSleeper(TimeSpan.FromSeconds(2), 1);

            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)api.Market
                    && (i.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                    && i.PublishedStatus == (int)PublishedStatuses.HasPublishRequest)
                    .ToList();

                var parentItemAsins = items.Select(i => i.ParentASIN).Distinct().ToList();
                var parentItemsToUnpublish = db.ParentItems.GetAllAsDto().Where(pi => pi.Market == (int)api.Market
                            && (pi.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                            && parentItemAsins.Contains(pi.ASIN))
                    .ToList();

                foreach (var parentItem in parentItemsToUnpublish)
                {
                    var itemId = StringHelper.TryGetLong(parentItem.SourceMarketId);
                    if (!itemId.HasValue)
                    {
                        _log.Error("Item has empty SourceMarketId");
                        continue;
                    }
                    sleeper.NextStep();
                    var result = api.PublishItem(itemId.Value);
                    if (result.IsSuccess)
                    {
                        _log.Info("ASIN was published=" + parentItem.ASIN + ", sourceMarketId=" + parentItem.SourceMarketId);
                        var dbItemList = db.Items.GetAll().Where(i => i.ParentASIN == parentItem.ASIN
                            && i.Market == parentItem.Market
                            && (i.MarketplaceId == parentItem.MarketplaceId
                                || String.IsNullOrEmpty(parentItem.MarketplaceId)))
                             .ToList();
                        foreach (var dbItem in dbItemList)
                        {
                            _log.Info("Published variation, id=" + dbItem.SourceMarketId);
                            dbItem.ItemPublishedStatus = (int)PublishedStatuses.Published;
                            db.Commit();
                        }
                    }
                }
            }
        }


        public void SendPriceUpdates(ShopifyApi api)
        {
            _log.Info("Begin SendPriceUpdates");
            var today = _time.GetAppNowTime().Date;
            var sleeper = new StepSleeper(TimeSpan.FromSeconds(2), 1);

            using (var db = _dbFactory.GetRWDb())
            {
                var itemQuery = from l in db.Listings.GetAll()
                                join i in db.Items.GetAll() on l.ItemId equals i.Id
                                join s in db.Styles.GetAll() on i.StyleId equals s.Id
                                join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId into withSale
                                from sale in withSale.DefaultIfEmpty()
                                where l.PriceUpdateRequested
                                    && (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                        || i.ItemPublishedStatus == (int)PublishedStatuses.Unpublished)
                                    && i.Market == (int)api.Market
                                    && (i.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                                    && !l.IsRemoved
                                select new ItemDTO()
                                {
                                    ListingEntityId = l.Id,
                                    SourceMarketId = i.SourceMarketId,
                                    SKU = l.SKU,
                                    StyleId = i.StyleId,
                                    CurrentPrice = l.CurrentPrice,
                                    ListPrice = s.MSRP,
                                    SalePrice = sale != null ? sale.SalePrice : null,
                                    SaleStartDate = sale != null ? sale.SaleStartDate : null,
                                    SaleEndDate = sale != null ? sale.SaleEndDate : null,
                                };

                var listingWithErrorList = new List<ItemDTO>();

                var items = itemQuery.ToList();

                foreach (var item in items)
                {
                    //var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (!String.IsNullOrEmpty(item.SourceMarketId))
                    {
                        sleeper.NextStep();

                        var result = api.UpdatePrice(StringHelper.TryGetLong(item.SourceMarketId).Value,
                            item.CurrentPrice,
                            item.ListPrice
                            //item.SalePrice,
                            //item.SaleStartDate,
                            //item.SaleEndDate
                            );

                        if (result.Status == CallStatus.Success)
                        {
                            var dbListing = db.Listings.GetAll().FirstOrDefault(i => i.Id == item.ListingEntityId);
                            dbListing.PriceUpdateRequested = false;
                            dbListing.AmazonCurrentPrice = item.CurrentPrice;
                            db.Commit();
                            _log.Info("Price updated, listingId=" + dbListing.ListingId + ", newPrice=" +
                                        dbListing.CurrentPrice);
                        }
                        else
                        {
                            listingWithErrorList.Add(item);
                            _log.Info("Can't update qty, result=" + result.ToString());
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(item.SourceMarketId))
                            _log.Warn("Item hasn't sourceMarketId, itemId=" + item.Id);
                    }
                }
            }
            _log.Info("End SendPriceUpdates");
        }

        public void SendInventoryUpdates(ShopifyApi api)
        {
            _log.Info("Begin SendInventoryUpdates");
            var sleeper = new StepSleeper(TimeSpan.FromSeconds(2), 1);

            using (var db = _dbFactory.GetRWDb())
            {
                var listings = db.Listings.GetQuantityUpdateRequiredList(api.Market, api.MarketplaceId);
                var itemIdList = listings.Select(l => l.ItemId).ToList();
                var items = db.Items.GetAllViewAsDto().Where(i => itemIdList.Contains(i.Id)).ToList();

                var listingWithErrorList = new List<Listing>();

                foreach (var listing in listings)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null
                        && !String.IsNullOrEmpty(item.SourceMarketId))
                    {
                        sleeper.NextStep();
                        var result = api.UpdateQuantityByLocation(long.Parse(item.SourceMarketId),
                            listing.RealQuantity,
                            true);

                        if (result.Status == CallStatus.Success)
                        {
                            listing.QuantityUpdateRequested = false;
                            listing.AmazonRealQuantity = listing.RealQuantity;
                            db.Commit();
                            _log.Info("Qty updated, listingId=" + listing.ListingId + ", sendQty=" +
                                        listing.RealQuantity);
                        }
                        else
                        {
                            listingWithErrorList.Add(listing);
                            _log.Info("Can't update qty, result=" + result.ToString());
                        }
                    }
                    else
                    {
                        if (item == null)
                        {
                            _log.Warn("Can't find item, for listing=" + listing.ListingId);
                        }
                        else
                        {
                            if (!item.DropShipperId.HasValue)
                                _log.Warn("Item hasn't dropShipperId, itemId=" + item.Id);
                            if (String.IsNullOrEmpty(item?.SourceMarketId))
                                _log.Warn("Item hasn't sourceMarketId, itemId=" + item.Id);
                        }
                    }
                }
            }
            _log.Info("End SendInventoryUpdates");
        }

        public void SendItemUpdates(ShopifyApi api)
        {
            _log.Info("Begin SendItemUpdates");
            var runner = new PublicationRunner(_log, _time, _dbFactory, PublicationRunner.PublishingMode.PerItem);
            var sleeper = new StepSleeper(TimeSpan.FromSeconds(2), 1);
            runner.SendItemUpdates((img) => { },
                (parentItem,
                    enableColorVariation,
                    variations,
                    styles,
                    styleImages,
                    styleFeatures,
                    dropShippers) =>
                {
                    var description = BuildDescription(styles, styleFeatures);
                    sleeper.NextStep();

                    return api.AddOrUpdateProduct(parentItem,
                        variations,
                        styles,
                        styleImages,
                        styleFeatures,
                        description);
                },
                api.Market,
                api.MarketplaceId,
                null);
            _log.Info("End SendItemUpdates");
        }

        private string BuildDescription(IList<StyleEntireDto> styles, IList<StyleFeatureValueDTO> features)
        {
            if (!styles.Any())
                return null;

            var mainStyle = styles.FirstOrDefault();
            var styleFeatures = features.Where(f => f.StyleId == mainStyle.Id
                && f.FeatureId != StyleFeatureHelper.PRODUCT_TYPE).ToList();
            var description = mainStyle.Description;
            var featureLines = styleFeatures
                .Where(f => !String.IsNullOrEmpty(f.Value))
                .Select(f => String.Format("<li><strong>{0}: </strong>{1}</li>", f.FeatureName, f.Value)).ToList();
            if (String.IsNullOrEmpty(description))
                description = "<ul>" + String.Join(Environment.NewLine, featureLines) + "</ul>" + "<div>" + description + "</div><br/>";

            return description;
        }

    }
}
