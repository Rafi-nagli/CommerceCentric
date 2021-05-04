using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.DTO;
using Amazon.Model.Implementation.Sync;
using eBay.Api;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using DropShipper.Api;

namespace Amazon.Model.Implementation.Markets.eBay
{
    public class DSItemsSync : IItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private DropShipperApi _api;
        private IDbFactory _dbFactory;

        public DSItemsSync(ILogService log,
            ITime time,
            DropShipperApi api,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _api = api;
            _dbFactory = dbFactory;
        }

        public void SendInventoryUpdates()
        {
            SendInventoryUpdates(null);
        }

        public void SendInventoryUpdates(IList<string> skuList)
        {
            _log.Info("Begin SendInventoryUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                IList<ItemDTO> items = null;
                IList<Listing> listings = null;

                if (skuList == null)
                {
                    var beforeDate = _time.GetAppNowTime().AddMinutes(-120); //NOTE: update not often, every 2 hours
                    listings = db.Listings.GetQuantityUpdateRequiredList(_api.Market, _api.MarketplaceId);
                    var itemIdList = listings
                        //.Where(l => !l.LastQuantityUpdatedOnMarket.HasValue || l.LastQuantityUpdatedOnMarket < beforeDate)
                        .Select(l => l.ItemId)
                        .ToList();

                    items = db.Items.GetAllViewAsDto()
                        .Where(i => itemIdList.Contains(i.Id))
                        .ToList();
                }
                else
                {
                    items = db.Items.GetAllViewAsDto()
                        .Where(i => skuList.Contains(i.SKU))
                        .ToList();
                    listings = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)).ToList();
                }

                foreach (var listing in listings)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null)
                    {
                        _log.Info("Send qty updates, sourceMarketId=" + item.SourceMarketId + ", qty=" + listing.RealQuantity);
                        var result = _api.UpdateQuantity(listing.SKU,
                            listing.RealQuantity);
                        if (result.IsSuccess)
                        {
                            listing.QuantityUpdateRequested = false;
                            //NOTE: overwrite quantities, prevent to check after read qties
                            //listing.AmazonRealQuantity = listing.RealQuantity;
                            db.Commit();
                            _log.Info("Qty updated, sourceMarketId=" + item.SourceMarketId + ", SKU=" + listing.SKU + ", sendQty=" + listing.RealQuantity);
                        }
                        else
                        {
                            _log.Info("Can't update qty, result=" + result.ToString());
                        }
                        listing.LastQuantityUpdatedOnMarket = _time.GetAppNowTime();
                        db.Commit();
                    }
                    else
                    {
                        //Nothing, generally these listings aren't published
                    }
                }
            }
            _log.Info("End SendInventoryUpdates");
        }

        public void SendPriceUpdates()
        {
            SendPriceUpdates(null);
        }

        public void SendPriceUpdates(IList<string> skuList)
        {
            _log.Info("Begin SendPriceUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                IList<ItemDTO> items = null;
                IList<Listing> listings = null;

                if (skuList == null)
                {
                    var beforeDate = _time.GetAppNowTime().AddMinutes(-120); //NOTE: update not often, every 2 hours
                    listings = db.Listings.GetPriceUpdateRequiredList(_api.Market, _api.MarketplaceId);
                    var itemIdList = listings.Where(l => !l.LastQuantityUpdatedOnMarket.HasValue || l.LastQuantityUpdatedOnMarket < beforeDate).Select(l => l.ItemId).ToList();

                    items = db.Items.GetAllViewAsDto()
                        .Where(i => itemIdList.Contains(i.Id))
                        .ToList();
                }
                else
                {
                    items = db.Items.GetAllViewAsDto()
                        .Where(i => skuList.Contains(i.SKU))
                        .ToList();
                    listings = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)).ToList();
                }

                var listingWithErrorList = new List<Listing>();
                var today = _time.GetAppNowTime();
                var market = _api.Market;

                foreach (var listing in listings)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null)
                    {
                        var currentPrice = listing.CurrentPrice;
                        if (item.SalePrice.HasValue && item.SaleStartDate <= today)
                            currentPrice = item.SalePrice.Value;

                        _log.Info("Send price updates, sourceMarketId=" + item.SourceMarketId
                                    + ", size=" + item.StyleSize
                                    + ", price=" + listing.CurrentPrice
                                    + ", includeSale=" + currentPrice);

                        var result = _api.UpdatePrice(item.SKU,
                            currentPrice);

                        if (result.IsSuccess)
                        { 
                            listing.PriceUpdateRequested = false;
                            listing.LastPriceUpdatedOnMarket = _time.GetAppNowTime();

                            _log.Info("Price updated, listingId=" + listing.ListingId + ", sendPrice=" +
                                        currentPrice);
                        }
                        else
                        {
                            listing.LastPriceUpdatedOnMarket = _time.GetAppNowTime();
                            _log.Info("Can't update prices, result=" + result.ToString());
                        }
                        db.Commit();
                    }
                    else
                    {
                        //Nothing, generally these listings aren't published
                    }
                }
            }
            _log.Info("End SendPriceUpdates");
        }

        public void SendItemUpdates()
        {
            throw new NotImplementedException();
        }

        public void ReadItems(DateTime? lastSync)
        {
            throw new NotImplementedException();
        }
    }
}
