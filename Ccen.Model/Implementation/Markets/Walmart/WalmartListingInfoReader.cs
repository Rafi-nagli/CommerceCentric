using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Models;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class WalmartListingInfoReader
    {
        private IWalmartApi _api;
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ISystemActionService _actionService;
        private IItemHistoryService _itemHistoryService;
        private string _reportBaseDirectory;
        private string _feedBaseDirectory;

        public WalmartListingInfoReader(ILogService log, 
            ITime time,
            IWalmartApi api, 
            IDbFactory dbFactory,
            ISystemActionService actionService,
            IItemHistoryService itemHistoryService,
            string reportBaseDirectory,
            string feedBaseDirectory)
        {
            _api = api;
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _actionService = actionService;
            _itemHistoryService = itemHistoryService;
            _reportBaseDirectory = reportBaseDirectory;
            _feedBaseDirectory = feedBaseDirectory;
        }

        public void ReadListingInventory()
        {
            if (_api.Market == MarketType.Walmart)
            {
                var markAllWFSListingInfos = ((WalmartApi)_api).GetWFSInventory(null);
                var markAllWFSSKUs = markAllWFSListingInfos.Where(i => i.RealQuantity > 0 || i.AmazonRealQuantity > 0).Select(i => i.SKU).ToList();

                using (var db = _dbFactory.GetRWDb())
                {
                    var dbWFSListings = db.Listings.GetAll().Where(l => l.IsFBA
                        && l.Market == (int)MarketType.Walmart
                        && !l.IsRemoved)
                        .ToList();
                    var dbExistWFSSKUs = dbWFSListings.Select(l => l.SKU).ToList();

                    var newWFSListingSKUs = markAllWFSSKUs.Where(i => !dbExistWFSSKUs.Contains(i)).ToList();
                    var dbNewWFSListings = db.Listings.GetAll().Where(l => newWFSListingSKUs.Contains(l.SKU)
                        && l.Market == (int)MarketType.Walmart
                        && !l.IsRemoved).ToList();
                    foreach (var dbNewWFSListing in dbNewWFSListings)
                    {
                        _log.Info("Set IsFBA=true, for SKU=" + dbNewWFSListing.SKU);
                        _itemHistoryService.AddRecord(dbNewWFSListing.ItemId, ItemHistoryHelper.IsFBAKey, dbNewWFSListing.IsFBA, true, null);
                        dbNewWFSListing.IsFBA = true;                        
                        SystemActionHelper.RequestPriceRecalculation(db, _actionService, dbNewWFSListing.Id, null);
                    }

                    var dbRemoveWFSStatus = dbWFSListings.Where(i => !markAllWFSSKUs.Contains(i.SKU)).ToList();
                    foreach (var dbRemoveWFSListing in dbRemoveWFSStatus)
                    {
                        _log.Info("Rest IsFBA=false, for SKU=" + dbRemoveWFSListing.SKU);
                        _itemHistoryService.AddRecord(dbRemoveWFSListing.ItemId, ItemHistoryHelper.IsFBAKey, dbRemoveWFSListing.IsFBA, false, null);
                        dbRemoveWFSListing.IsFBA = false;
                        SystemActionHelper.RequestPriceRecalculation(db, _actionService, dbRemoveWFSListing.Id, null);
                    }
                }
            }
        }

        public void UpdateListingInfo()
        {
            UpdateListingInfo(null);
        }

        public void UpdateListingInfo(string overrideReportPath)
        {
            IList<ItemDTO> items = new List<ItemDTO>();
            if (_api.Market == MarketType.Walmart)
            {
                var reportFilename = _api.GetItemsReport(_reportBaseDirectory);
                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
            }
            if (_api.Market == MarketType.WalmartCA)
            {
                //https://seller.walmart.ca/resource/item/download?type=item
                var reportFilename = "";
                if (!String.IsNullOrEmpty(overrideReportPath))
                    reportFilename = overrideReportPath;
                else
                    reportFilename = Path.Combine(_reportBaseDirectory, "item\\item_ca_report (10).zip");

                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
                foreach (var item in items)
                {
                    var itemId = item.SourceMarketId;
                    item.SourceMarketId = item.ListingId;
                    item.ListingId = itemId;
                }
                //items = _api.GetAllItems();
            }


            _log.Info("Received, item count=" + items.Count);

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var item in items)
                {
                    var dbListing = db.Listings.GetAll()
                        .OrderBy(l => l.IsRemoved)
                        .FirstOrDefault(i => i.SKU == item.SKU
                        && i.Market == (int)_api.Market);
                    if (dbListing != null)
                    {
                        //_log.Info("Item market, sku=" + item.SKU + ", qty=" + item.AmazonRealQuantity + ", price=" + item.AmazonRealQuantity + ", status=" + item.PublishedStatus);
                        if (dbListing.AmazonCurrentPrice != item.AmazonCurrentPrice)
                            _log.Info("SKU=" + item.SKU + ", price: " + dbListing.AmazonCurrentPrice + "=>" + item.AmazonCurrentPrice);
                        dbListing.AmazonCurrentPrice = item.AmazonCurrentPrice;
                        dbListing.AmazonCurrentPriceUpdateDate = _time.GetAppNowTime();
                        dbListing.ListingId = StringHelper.GetFirstNotEmpty(item.ListingId, item.SKU);

                        if (dbListing.AmazonRealQuantity != item.AmazonRealQuantity)
                            _log.Info("SKU=" + item.SKU + ", qty: " + dbListing.AmazonRealQuantity + "=>" + item.AmazonRealQuantity);
                        dbListing.AmazonRealQuantity = item.AmazonRealQuantity;
                        dbListing.AmazonRealQuantityUpdateDate = _time.GetAppNowTime();

                        //NOTE 25/11/2020: We do it in ReadInventoryInfo by separate API, this information is not valid
                        //if (dbListing.IsFBA != item.IsFBA)
                        //{
                        //    SystemActionHelper.RequestPriceRecalculation(db, _actionService, dbListing.Id, null);
                        //    _log.Info("SKU=" + item.SKU + ", isFBA: " + dbListing.IsFBA + "=>" + item.IsFBA);
                        //}
                        //dbListing.IsFBA = item.IsFBA;

                        var dbItem = db.Items.GetAll().FirstOrDefault(i => i.Id == dbListing.ItemId);
                        if (dbItem != null)
                        {
                            if (dbItem.Barcode != item.Barcode
                                && !String.IsNullOrEmpty(item.Barcode))
                            {
                                _log.Info("Item Barcode has been updated, from=" + dbItem.Barcode + ", to=" + item.Barcode);
                                dbItem.Barcode = item.Barcode;
                            }

                            if (dbItem.SourceMarketId != item.SourceMarketId)
                            {
                                _log.Info("Item SourceMarketId has been updated, from=" + dbItem.SourceMarketId +
                                          ", to=" + item.SourceMarketId);
                                dbItem.SourceMarketId = item.SourceMarketId;
                            }

                            dbItem.Rank = item.Rank;

                            dbItem.ItemPublishedStatusReason = StringHelper.Substring(item.PublishedStatusReason, 255);

                            if (dbItem.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited
                                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.Published
                                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                //|| dbItem.ItemPublishedStatus == (int)PublishedStatuses.Unpublished //NOTE: always keep UNPUBLISHED status
                                )
                            {
                                if (dbItem.ItemPublishedStatus != item.PublishedStatus)
                                {
                                    _log.Info("Item update published status, from=" + dbItem.ItemPublishedStatus +
                                              ", to=" + item.PublishedStatus);
                                    dbItem.ItemPublishedStatus = item.PublishedStatus;
                                    dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                                }
                            }
                        }
                    }
                }
                db.Commit();
            }

        }

        public void FindSecondDayFlagDisparity(string overrideFeedpath)
        {
            IList<ItemDTO> items = new List<ItemDTO>();
            if (_api.Market == MarketType.Walmart)
            {
                string reportFilename = null;
                if (!String.IsNullOrEmpty(overrideFeedpath))
                    reportFilename = overrideFeedpath;
                else
                    reportFilename = _api.GetItemsReport(_reportBaseDirectory);
                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
            }
            if (_api.Market == MarketType.WalmartCA)
            {
                string reportFilename = overrideFeedpath;

                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
                foreach (var item in items)
                {
                    var itemId = item.SourceMarketId;
                    item.SourceMarketId = item.ListingId;
                    item.ListingId = itemId;
                }
            }

            _log.Info("Received, item count=" + items.Count);

            var itemsIdToUpdate = new List<long>();
            using (var db = _dbFactory.GetRWDb())
            {
                var allExistSKUs = db.Listings.GetAll()
                    .Where(i => !i.IsRemoved
                        && i.Market == (int)_api.Market
                        && (i.MarketplaceId == _api.MarketplaceId
                            || String.IsNullOrEmpty(_api.MarketplaceId)))
                    .Select(i => new ItemDTO()
                    {
                        SKU = i.SKU,
                        IsPrime = i.IsPrime,
                        Id = i.ItemId
                    })
                    .ToList();
                allExistSKUs.ForEach(i => i.SKU = (i.SKU ?? "").ToUpper());

                foreach (var item in items)
                {
                    var upperSKU = (item.SKU ?? "").ToUpper();
                    var existSKU = allExistSKUs.FirstOrDefault(i => i.SKU == upperSKU);

                    if (existSKU != null && existSKU.IsPrime != item.IsPrime)
                    {
                        itemsIdToUpdate.Add(existSKU.Id);
                    }
                }

                _log.Info("Items count with disparity: " + itemsIdToUpdate.Count);
                _log.Info("SKUs: " + String.Join(",", itemsIdToUpdate));                
            }
        }

        public void ResetQtyForNotExistListings()
        {
            ResetQtyForNotExistListings(null);
        }

        public void ResetQtyForNotExistListings(string overrideReportPath)
        {
            IList<ItemDTO> items = new List<ItemDTO>();
            if (_api.Market == MarketType.Walmart)
            {
                var reportFilename = _api.GetItemsReport(_reportBaseDirectory);
                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
            }
            if (_api.Market == MarketType.WalmartCA)
            {
                var reportFilename = Path.Combine(_reportBaseDirectory, "item\\item_ca_report (8).zip");
                if (!String.IsNullOrEmpty(overrideReportPath))
                    reportFilename = overrideReportPath;

                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
                foreach (var item in items)
                {
                    var itemId = item.SourceMarketId;
                    item.SourceMarketId = item.ListingId;
                    item.ListingId = itemId;
                }
            }

            _log.Info("Received, item count=" + items.Count);

            var notExistItems = new List<ItemDTO>();
            using (var db = _dbFactory.GetRWDb())
            {
                var allExistSKUs = db.Listings.GetAll()
                    .Where(i => !i.IsRemoved
                        && i.Market == (int)_api.Market
                        && (i.MarketplaceId == _api.MarketplaceId
                            || String.IsNullOrEmpty(_api.MarketplaceId)))
                    .Select(i => new ItemDTO()
                    {
                        SKU = i.SKU,
                    })
                    .ToList();
                allExistSKUs.ForEach(i => i.SKU = (i.SKU ?? "").ToUpper());

                foreach (var item in items)
                {
                    var upperSKU = (item.SKU ?? "").ToUpper();
                    var existSKU = allExistSKUs.FirstOrDefault(i => i.SKU == upperSKU);

                    if (existSKU == null)
                    {
                        _log.Info("Item market, sku=" + item.SKU + ", qty=" + item.AmazonRealQuantity + ", price=" + item.AmazonCurrentPrice + ", status=" + item.PublishedStatus);
                        notExistItems.Add(item);
                    }
                }

                var itemsToUpdate = notExistItems.Where(i => i.AmazonRealQuantity > 0).ToList();
                itemsToUpdate.ForEach(i => i.RealQuantity = 0);
                _api.SubmitInventoryFeed("not_exist_reset_qty_" + DateTime.Now.Ticks.ToString(),
                    itemsToUpdate,
                    _feedBaseDirectory);
            }
        }


        public void RetireNotExistListings()
        {
            RetireNotExistListings(null);
        }

        public void RetireNotExistListings(string overrideReportPath)
        {
            IList<ItemDTO> items = new List<ItemDTO>();
            if (_api.Market == MarketType.Walmart)
            {
                var reportFilename = _api.GetItemsReport(_reportBaseDirectory);
                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
            }
            if (_api.Market == MarketType.WalmartCA)
            {
                var reportFilename = Path.Combine(_reportBaseDirectory, "item\\item_ca_report (8).zip");
                if (!String.IsNullOrEmpty(overrideReportPath))
                    reportFilename = overrideReportPath;

                var report = new WalmartReport(reportFilename);
                items = report.GetItems();
                foreach (var item in items)
                {
                    var itemId = item.SourceMarketId;
                    item.SourceMarketId = item.ListingId;
                    item.ListingId = itemId;
                }
            }

            _log.Info("Received, item count=" + items.Count);

            var notExistItems = new List<ItemDTO>();
            using (var db = _dbFactory.GetRWDb())
            {
                var allExistSKUs = db.Listings.GetAll()
                    .Where(i => !i.IsRemoved
                        && i.Market == (int)_api.Market
                        && (i.MarketplaceId == _api.MarketplaceId
                            || String.IsNullOrEmpty(_api.MarketplaceId)))
                    .Select(i => new ItemDTO()
                    {
                        SKU = i.SKU,
                    })
                    .ToList();
                allExistSKUs.ForEach(i => i.SKU = (i.SKU ?? "").ToUpper());

                foreach (var item in items)
                {
                    var upperSKU = (item.SKU ?? "").ToUpper();
                    var existSKU = allExistSKUs.FirstOrDefault(i => i.SKU == upperSKU);

                    if (existSKU == null)
                    {
                        _log.Info("Item market, sku=" + item.SKU + ", qty=" + item.AmazonRealQuantity + ", price=" + item.AmazonCurrentPrice + ", status=" + item.PublishedStatus);
                        notExistItems.Add(item);
                    }
                }

                var itemsToUpdate = notExistItems.ToList();
                itemsToUpdate.ForEach(i => i.RealQuantity = 0);
                
                _api.SubmitInventoryFeed("not_exist_reset_qty_" + DateTime.Now.Ticks.ToString(),
                    itemsToUpdate,
                    _feedBaseDirectory);

                foreach (var item in notExistItems)
                {
                    try
                    {
                        var result = _api.RetireItem(item.SKU);
                        if (result.IsFail)
                        {
                            _log.Info("Unable to retire SKU=" + item.SKU);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                }
            }
        }
    }
}
