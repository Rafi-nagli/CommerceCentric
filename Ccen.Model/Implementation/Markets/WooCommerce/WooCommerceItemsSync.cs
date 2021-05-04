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
using WooCommerce.Api;

namespace Amazon.Model.Implementation.Markets.Shopify
{
    public class WooCommerceItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public WooCommerceItemsSync(ILogService log,
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

        public void SendPriceUpdates(WooCommerceApi api)
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

        public void SendInventoryUpdates(WooCommerceApi api)
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
    }
}
