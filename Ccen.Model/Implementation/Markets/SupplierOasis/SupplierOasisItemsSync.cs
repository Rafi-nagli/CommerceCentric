using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Supplieroasis.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.SupplierOasis
{
    public class SupplierOasisItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public SupplierOasisItemsSync(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void ReadItemsInfo(SupplieroasisApi api)
        {
            _log.Info("Begin ReadItemsInfo");
            var itemsWithError = new List<string>();
            var items = api.GetItems(_log,
                _time,
                null,
                ItemFillMode.Defualt,
                out itemsWithError);

            using (var db = _dbFactory.GetRWDb())
            {
                _log.Info("Total items=" + items.Count());
                //Create
                var allItems = db.Items.GetAllViewAsDto().Where(m => m.Market == (int)api.Market
                                                                    && (m.MarketplaceId == api.MarketplaceId ||
                                                                    String.IsNullOrEmpty(api.MarketplaceId))).ToList();
                foreach (var item in items)
                {
                    foreach (var variation in item.Variations)
                    {
                        var existItem = allItems.FirstOrDefault(i => StringHelper.IsEqualNoCase(i.SKU, variation.SKU));

                        if (existItem == null)
                        {
                            _log.Info("Add new item, MarketItemId=" + variation.SourceMarketId);

                            var exampleItem = db.Items.GetAllViewAsDto().FirstOrDefault(i => i.Market == (int)MarketType.Amazon
                                                                                && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                                                                && i.SKU == variation.SKU);
                            if (exampleItem == null)
                            {
                                exampleItem = db.Items.GetAllViewAsDto().OrderByDescending(i => i.CreateDate).FirstOrDefault(i => i.SKU == variation.SKU);
                            }

                            var styleString = SkuHelper.RetrieveStyleIdFromSKU(db, variation.SKU, null);
                            if (String.IsNullOrEmpty(styleString))
                                styleString = variation.SKU;

                            if (String.IsNullOrEmpty(styleString))
                            {
                                _log.Info("StyleString is empty for: " + variation.SKU + ", itemSKU=" + item.SKU);
                                continue;
                            }

                            var parentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.ASIN == styleString
                                && pi.Market == (int)MarketType.OverStock);
                            if (parentItem == null)
                            {
                                parentItem = new ParentItem()
                                {
                                    ASIN = styleString,
                                    SourceMarketId = styleString,
                                    Market = item.Market,
                                    MarketplaceId = item.MarketplaceId,

                                    CreateDate = _time.GetAmazonNowTime()
                                };

                                db.ParentItems.Add(parentItem);
                                db.Commit();
                            }
                            
                            if (exampleItem == null)
                            {
                                exampleItem = new DTO.ItemDTO();
                                var style = db.Styles.GetAll().FirstOrDefault(st => st.StyleID == styleString);
                                if (style != null)
                                    exampleItem.StyleId = style.Id;
                                var size = SkuHelper.RetrieveSizeFromSKU(variation.SKU);

                                if (style != null)
                                {
                                    var styleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.StyleId == style.Id
                                        && si.Size == size);
                                    if (styleItem != null)
                                        exampleItem.StyleItemId = styleItem.Id;
                                }
                            }

                            var newItem = new Item()
                            {
                                ASIN = item.ASIN,
                                ParentASIN = parentItem.ASIN,
                                Market = item.Market,
                                MarketplaceId = item.MarketplaceId,

                                Barcode = variation.Barcode,

                                SourceMarketId = item.SourceMarketId,
                                ItemPublishedStatus = variation.PublishedStatus,

                                StyleId = exampleItem?.StyleId,
                                StyleItemId = exampleItem?.StyleItemId,
                                                                
                                CreateDate = _time.GetAmazonNowTime()
                            };
                            db.Items.Add(newItem);
                            db.Commit();

                            var newListing = new Listing()
                            {
                                ItemId = newItem.Id,
                                SKU = variation.SKU,
                                ListingId = variation.SKU,
                                Market = variation.Market,
                                MarketplaceId = variation.MarketplaceId,

                                CreateDate = _time.GetAmazonNowTime(),
                                AmazonRealQuantity = variation.AmazonRealQuantity,
                            };
                            db.Listings.Add(newListing);
                            db.Commit();
                        }
                    }
                }


                //Update
                var updatedItemIds = new List<long>();
                foreach (var item in items)
                {
                    foreach (var variation in item.Variations)
                    {
                        _log.Info("Read info for SKU=" + variation.SKU);
                        var dbListing = db.Listings.GetAll().FirstOrDefault(m => m.Market == (int)api.Market
                                                                    && (m.MarketplaceId == api.MarketplaceId ||
                                                                    String.IsNullOrEmpty(api.MarketplaceId))
                                                                    && m.SKU == variation.SKU);
                        if (dbListing == null)
                        {
                            _log.Info("Unable to find item for, SKU=" + variation.SKU);
                            continue;
                        }

                        var dbItem = db.Items.GetAll().FirstOrDefault(i => i.Id == dbListing.ItemId);
                        if (dbItem == null)
                        {
                            _log.Info("Unable to find item for itemId=" + dbListing.ItemId);
                            continue;
                        }

                        updatedItemIds.Add(dbItem.Id);

                        if (dbItem.ItemPublishedStatus != variation.PublishedStatus)
                        {
                            dbItem.ItemPublishedStatus = variation.PublishedStatus;
                            dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                            dbItem.ItemPublishedStatusReason = "Derived from market";
                        }

                        dbItem.Barcode = variation.Barcode;

                        //dbListing.AmazonCurrentPrice = variation.AmazonCurrentPrice;
                        //dbListing.AmazonCurrentPriceUpdateDate = _time.GetAppNowTime();

                        dbListing.AmazonRealQuantity = variation.AmazonRealQuantity;
                        dbListing.AmazonRealQuantityUpdateDate = _time.GetAppNowTime();
                    }
                }
                db.Commit();

                //Remove not exists
                var toRemoveItems = db.Items.GetAll().Where(i => i.Market == (int)api.Market
                                                                    && (i.MarketplaceId == api.MarketplaceId ||
                                                                    String.IsNullOrEmpty(api.MarketplaceId))
                                                                    && !updatedItemIds.Contains(i.Id)
                                                                    && i.ItemPublishedStatus == (int)PublishedStatuses.Published);
                _log.Info("Items to unpublish, count=" + toRemoveItems.Count());
                foreach (var toRemoveItem in toRemoveItems)
                {
                    toRemoveItem.ItemPublishedStatusBeforeRepublishing = toRemoveItem.ItemPublishedStatus;
                    toRemoveItem.ItemPublishedStatus = (int)PublishedStatuses.Unpublished;
                    toRemoveItem.ItemPublishedStatusDate = _time.GetUtcTime();
                }
                db.Commit();
            }
            _log.Info("End ReadItemsInfo");
        }

        public void SendInventoryUpdates(SupplieroasisApi api)
        {
            SendInventoryUpdates(api, null);
        }


        public void SendInventoryUpdates(SupplieroasisApi api, IList<string> skuList)
        {
            _log.Info("Begin SendInventoryUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                var listings = db.Listings.GetQuantityUpdateRequiredList(api.Market, api.MarketplaceId);

                if (skuList != null)
                    listings = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)
                                && l.Market == (int)api.Market
                                && (l.MarketplaceId == api.MarketplaceId
                            || String.IsNullOrEmpty(api.MarketplaceId)))
                            .ToList();

                var itemIdList = listings.Select(l => l.ItemId).ToList();
                var items = db.Items.GetAllViewAsDto().Where(i => itemIdList.Contains(i.Id)).ToList();

                var listingWithErrorList = new List<Listing>();
                                
                foreach (var listing in listings)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null)
                    {
                        var result = api.UpdateQuantity(item.SKU,
                            StringHelper.GetFirstNotEmpty(item.Barcode, item.SKU),
                            listing.RealQuantity);

                        if (result.Status == CallStatus.Success)
                        {
                            listing.QuantityUpdateRequested = false;
                            listing.AmazonRealQuantity = listing.RealQuantity;
                            db.Commit();
                            _log.Info("Qty updated, sku=" + listing.SKU + ", sendQty=" +
                                        listing.RealQuantity);
                        }
                        else
                        {
                            listingWithErrorList.Add(listing);
                            _log.Info("Can't update qty, sku=" + listing.SKU + ", result=" + result.Message);
                        }
                    }
                    else
                    {
                        if (item == null)
                            _log.Warn("Can't find item, for listing=" + listing.SKU + ", id=" + listing.Id);
                    }
                }
            }
            _log.Info("End SendInventoryUpdates");
        }
    }
}
