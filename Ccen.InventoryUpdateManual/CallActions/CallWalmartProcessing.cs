using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Web.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Walmart.Api;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallWalmartProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private WalmartApi _walmartApi;
        private CompanyDTO _company;
        private IEmailService _emailService;
        private ISystemActionService _actionService;
        private IItemHistoryService _itemHistoryService;
        private IHtmlScraperService _htmlScraper;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;
        private WalmartOpenApi _openApi;

        public CallWalmartProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IDbFactory dbFactory,
            IEmailService emailService,
            IItemHistoryService itemHistoryService,
            WalmartApi walmartApi,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _walmartApi = walmartApi;
            _company = company;
            _emailService = emailService;
            _itemHistoryService = itemHistoryService;

            _actionService = new SystemActionService(_log, _time);
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);
            _barcodeService = new BarcodeService(log, time, dbFactory);

            _openApi = new WalmartOpenApi(_log, "trn9fdghvb8p9gjj9j6bvjwx");
            _autoCreateListingService = new AutoCreateWalmartListingService(log, 
                time, 
                dbFactory, 
                _cacheService, 
                _barcodeService, 
                _emailService,
                _openApi,
                itemHistoryService,
                AppSettings.IsDebug);
        }

        private class SetupByMatchProduct
        {
            public string ProductIdType1 { get; set; }
            public string ProductId1 { get; set; }
            public string ProductTaxCode { get; set; }

            public string ProductIdType2 { get; set; }
            public string ProductId2 { get; set; }

            public string SKU { get; set; }
            public string ASIN { get; set; }
            public string Currency { get; set; }
            public decimal Price { get; set; }
            public decimal Weight { get; set; }
            public string WeightUnit { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
        }

        public void EnableSecondDayForListings()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var now = _time.GetAppNowTime();

                var listingSalesQuery = (from s in db.StyleItemSales.GetAll()
                                    join sm in db.StyleItemSaleToMarkets.GetAll() on s.Id equals sm.SaleId
                                    join sl in db.StyleItemSaleToListings.GetAll() on sm.Id equals sl.SaleToMarketId
                                    where !s.IsDeleted
                                        && s.SaleStartDate <= now
                                    select new StyleItemSaleToMarketDTO()
                                    {
                                        Id = sl.ListingId,
                                        SalePrice = sm.SalePrice,
                                    });

                var amzAllItems = (from i in db.Items.GetAll()
                                   join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                   join sale in listingSalesQuery on l.Id equals sale.Id into withSale
                                   from sale in withSale.DefaultIfEmpty()
                                   where l.IsPrime
                                       && !l.IsRemoved
                                       && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                   select new ItemDTO()
                                   {
                                       StyleId = i.StyleId,
                                       StyleItemId = i.StyleItemId,
                                       CurrentPrice = l.CurrentPrice,
                                       SalePrice = sale.SalePrice,
                                       SKU = l.SKU,
                                       OnMarketTemplateName = i.OnMarketTemplateName
                                   }).ToList();

                var wmAllItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.Walmart
                    && (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                      || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges)).ToList();
                var wmAllListings = db.Listings.GetAll().Where(i => i.Market == (int)MarketType.Walmart
                    && !i.IsRemoved).ToList();

                var wmSaleToListings = db.StyleItemSaleToListings.GetAll().ToList();
                var wmSaleToMarket = db.StyleItemSaleToMarkets.GetAll().ToList();
                var wmSales = db.StyleItemSales.GetAll().ToList();

                var index = 0;
                foreach (var wmItem in wmAllItems)
                {
                    var wmListing = wmAllListings.FirstOrDefault(l => l.ItemId == wmItem.Id);
                    if (wmListing == null)
                        continue;

                    if (wmListing.IsPrime)
                        continue;

                    var saleToListings = wmSaleToListings.Where(sl => sl.ListingId == wmListing.Id).ToList();
                    List<StyleItemSaleToMarket> saleToMarkets = null;
                    if (saleToListings.Any())
                    {
                        var ids = saleToListings.Select(l => l.SaleToMarketId).ToList();
                        saleToMarkets = wmSaleToMarket.Where(m => ids.Contains(m.Id)).ToList();
                        var saleIds = saleToMarkets.Select(sm => sm.SaleId).ToList();
                        var sales = wmSales.Where(s => saleIds.Contains(s.Id)
                            && !s.IsDeleted).ToList();
                        var actualSaleIds = sales.Select(s => s.Id).ToList();
                        //NOTE: Keep only active sales
                        saleToMarkets = saleToMarkets.Where(sm => actualSaleIds.Contains(sm.SaleId)).ToList();
                    }

                    var amzItems = amzAllItems.Where(i => i.StyleItemId == wmItem.StyleItemId).ToList();
                    if (amzItems.Any())
                    {
                        var newPrice = amzItems.Min(i => i.SalePrice ?? i.CurrentPrice);
                        if (newPrice > 1)
                        {
                            wmListing.IsPrime = true;
                            if (newPrice < wmListing.CurrentPrice)
                                _log.Info("Listing: " + wmListing.SKU + ", price decreased: " + wmListing.CurrentPrice + "=>" + newPrice);
                            _log.Info("Convert to prime, SKU=" + wmListing.SKU + ", price: " + wmListing.CurrentPrice + "=>" + newPrice);
                            wmListing.CurrentPrice = newPrice;
                            if (saleToMarkets != null)
                            {
                                foreach (var sale in saleToMarkets)
                                {
                                    _log.Info("Sale changed, price=" + sale.SalePrice + "=>" + newPrice);
                                    sale.SalePrice = newPrice;
                                }
                            }
                            wmListing.PriceUpdateRequested = true;
                            wmItem.ItemPublishedStatus = (int)PublishedStatuses.HasChanges;                            
                        }
                    }
                    index++;

                    if (index % 100 == 0)
                    {
                        _log.Info("Index: " + index);
                        db.Commit();
                    }
                }

                db.Commit();
            }
        }

        public void GetWalmartSizes(IWalmartApi walmartApi)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAll()
                    .Where(i => i.Market == (int)MarketType.Walmart)
                    .Select(i => new ItemDTO()
                    {
                        Id = i.Id,
                        Barcode = i.Barcode,
                        ASIN = i.ASIN,
                        SourceMarketId = i.SourceMarketId,
                        PublishedStatus = i.ItemPublishedStatus,
                        Color = i.Color,
                        Size = i.Size,
                    })
                    .ToList();

                var existAllAttributes = db.ItemAdditions.GetAll().Where(i => i.Field == "MarketSize" || i.Field == "MarketColor").ToList();

                _log.Info("Total items: " + items.Count);
                var index = 0;
                var pageSize = 100;

                while (index < items.Count)
                {
                    _log.Info("index: " + index);
                    var pageItems = items.Skip(index).Take(pageSize).ToList();

                    foreach (var item in pageItems)
                    {
                        var result = _openApi.LookupProductByJSON(item.SourceMarketId);
                        if (result.Data != null && result.Data.Any())
                        {
                            var sizeAttr = existAllAttributes.Where(i => i.ItemId == item.Id
                                && i.Field == "MarketSize").FirstOrDefault();
                            if (sizeAttr == null)
                            {
                                sizeAttr = new Core.Entities.Listings.ItemAddition()
                                {
                                    Field = "MarketSize",
                                    ItemId = item.Id,
                                    CreateDate = _time.GetAppNowTime()
                                };
                                db.ItemAdditions.Add(sizeAttr);
                            }
                            sizeAttr.Value = result.Data[0].Size;

                            var colorAttr = existAllAttributes.Where(i => i.ItemId == item.Id
                                && i.Field == "MarketColor").FirstOrDefault();
                            if (colorAttr == null)
                            {
                                colorAttr = new Core.Entities.Listings.ItemAddition()
                                {
                                    Field = "MarketColor",
                                    ItemId = item.Id,
                                    CreateDate = _time.GetAppNowTime()
                                };
                                db.ItemAdditions.Add(colorAttr);
                            }
                            colorAttr.Value = result.Data[0].Color;

                            db.Commit();
                        }
                    }
                    index += pageSize;
                }
            }
        }

        public void ExportSetupByMatch()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var allAmazonItems = db.Items.GetAllViewActual()
                    .Where(i => i.Market == (int)MarketType.Amazon
                        && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                    .Select(i => new ItemDTO()
                    {
                        SKU = i.SKU,
                        Barcode = i.Barcode,
                        CurrentPrice = i.CurrentPrice,
                        StyleId = i.StyleId,
                    })
                    .ToList();

                var allWalmartItems = db.Items.GetAllViewActual()
                    .Where(i => i.Market == (int)MarketType.Walmart)
                    .Select(i => new ItemDTO()
                    {
                        SKU = i.SKU,
                        Barcode = i.Barcode
                    })
                    .ToList();

                var results = new List<SetupByMatchProduct>(allAmazonItems.Count);
                foreach (var amzItem in allAmazonItems)
                {
                    var genderValue = amzItem.StyleId.HasValue
                           ? db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(amzItem.StyleId.Value, StyleFeatureHelper.GENDER)?.Value
                           : null;
                    
                    if (allWalmartItems.All(i => i.Barcode != amzItem.Barcode))
                    {
                        var sku = amzItem.SKU;
                        var index = 0;
                        while (allWalmartItems.Any(i => i.SKU == sku))
                        {
                            sku = SkuHelper.SetSKUMiddleIndex(sku, index);
                            index++;
                        }
                        results.Add(new SetupByMatchProduct()
                        {
                            ProductIdType1 = "UPC",
                            ProductId1 = amzItem.Barcode,
                            
                            ProductTaxCode = WalmartUtils.GetProductTaxCode(genderValue, null)?.ToString(),

                            SKU = sku,
                            Currency = "USD",
                            Price = amzItem.CurrentPrice,

                            Weight = PriceHelper.RoundToTwoPrecision((decimal)((amzItem.Weight == null || amzItem.Weight == 0) ? 5 : amzItem.Weight.Value) / (decimal)16),
                            WeightUnit = "lb",

                            Category = "Clothing",
                            SubCategory = "Clothing"
                        });
                    }
                }

                var b = new ExportColumnBuilder<SetupByMatchProduct>();
                var columns = new List<ExcelColumnInfo>()
                {
                    b.Build(p => p.ProductIdType1, "Product Identifier-Product Id Type (#1) *", 15),
                    b.Build(p => p.ProductId1, "Product Identifier-Product Id (#1) *", 15),
                    b.Build(p => p.ProductTaxCode, "Product Tax Code *", 15),
                    b.Build(p => p.ProductIdType2, "Additional Product Attribute-Product Attribute Name (#1) (Optional)", 15),
                    b.Build(p => p.ProductId2, "Additional Product Attribute-Product Attribute Value (#1) (Optional)", 15),
                    b.Build(p => p.SKU, "Sku *", 15),
                    b.Build(p => p.ASIN, "ASIN (Optional)", 15),
                    b.Build(p => p.Currency, "Price-Currency *", 15),
                    b.Build(p => p.Price, "Price-Amount *", 15),
                    b.Build(p => p.Weight, "Shipping Weight-Value *", 15),
                    b.Build(p => p.WeightUnit, "Shipping Weight-Unit *", 15),
                    b.Build(p => p.Category, "Category *", 15),
                    b.Build(p => p.SubCategory, "Sub-category *", 15),
                };

                var outputFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WMSetupByMatch-" + DateTime.Now.ToString("yyyyddMMHHmmss") + ".xls");
                using (var stream = ExcelHelper.Export(results,
                    columns,
                    null))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var fileStream = File.Create(outputFilepath))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }

        public void ReadRefunds(WalmartApi api)
        {
            var walmartReader = new WalmartReturnInfoReader(_log,
                _time,
                (WalmartApi)api,
                _dbFactory,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);

            walmartReader.UpdateReturnInfo();
        }
        
        public void GetProductsInfo(IList<string> skuList)
        {
            IList<ItemDTO> items;
            using (var db = _dbFactory.GetRWDb())
            {
                items = db.Items.GetAllViewAsDto().Where(i => skuList.Contains(i.SKU)
                    && i.Market == (int)MarketType.Walmart).ToList();
            }
            foreach (var item in items)
            {
                var result = _openApi.LookupProductByJSON(item.SourceMarketId);
                _log.Info(item.SKU + ", itemId=" + item.SourceMarketId + ", parentASIN: " + (result.IsSuccess ? result.Data?.FirstOrDefault()?.ParentASIN : ""));
            }
        }

        public void ReducePrice(string filePath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                StreamReader streamReader = new StreamReader(filePath);
                CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimFields = true,
                });
                //Weight,A,B,C,D,E,F,G,H1,H2,I,L,M,J,K,N,P
                //Product, Package, Weight, A, B, ...
                var filename = Path.GetFileName(filePath);

                var results = new List<ItemDTO>();
                while (reader.Read())
                {
                    var sku = reader.GetField<string>("SKU");
                    if (String.IsNullOrEmpty(sku))
                        continue;

                    var price = reader.GetField<decimal>("Reduce by");

                    var item = new ItemDTO()
                    {
                        SKU = sku,
                        CurrentPrice = price,
                    };
                    results.Add(item);
                }

                _log.Info("File was processed, file: " + filename + ", records: " + results.Count);

                foreach (var item in results)
                {
                    var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == item.SKU && l.Market == (int)MarketType.Walmart);
                    if (dbListing != null)
                    {
                        dbListing.CurrentPrice = dbListing.CurrentPrice + item.CurrentPrice;
                        dbListing.PriceUpdateRequested = true;
                    }
                    else
                    {
                        _log.Info("Unable to find SKU=" + item.SKU);
                    }
                }
                db.Commit();
            }
        }

        public void UpdateOrdersTaxInfo()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var startDate = new DateTime(2016, 9, 1);
                var endDate = DateTime.Today;
                IEnumerable<DTOOrder> orders;
                do
                {
                    //orders = _walmartApi.GetOrders(_log, startDate, null);
                    orders = new List<DTOOrder>() {_walmartApi.GetOrder(_log, "2576707987362") };

                    var dtoOrderList = db.Orders.GetAll().Where(o => o.Market == (int)MarketType.Walmart).ToList();
                    var orderIdList = dtoOrderList.Select(o => o.Id).ToList();
                    var dbAllItems = db.OrderItemSources.GetAll().Where(oi => orderIdList.Contains(oi.OrderId)).ToList();

                    foreach (var order in orders)
                    {
                        var dtoOrder = dtoOrderList.FirstOrDefault(o => o.Id == order.Id);
                        if (dtoOrder != null)
                        {
                            var dbItems = dbAllItems.Where(oi => oi.OrderId == dtoOrder.Id).ToList();
                            foreach (var dbItem in dbItems)
                            {
                                var item = order.Items.FirstOrDefault(oi => oi.ItemOrderId == dbItem.ItemOrderIdentifier);
                                if (item != null && !dbItem.ItemTax.HasValue)
                                {
                                    dbItem.ItemTax = item.ItemTax ?? 0;
                                    dbItem.ShippingTax = item.ShippingTax ?? 0;
                                }
                            }
                        }
                    }

                    db.Commit();

                    startDate = orders.Max(o => o.OrderDate ?? DateTime.Now);
                    _log.Info("Till date: " + startDate);

                } while (orders.Count() > 0);
            }
        }

        public void ProcessCancellation(WalmartApi api, string orderString)
        {
            var actionService = new SystemActionService(_log, _time);
            var updater = new WalmartOrderCancellation(api, actionService, _log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                updater.ProcessCancellations(db, orderString);
            }
        }

        public void TestExtractWalmartItem()
        {
            var message = @"Hi Premium Apparel, 

            Lauren Brady wants to connect with you regarding the following order:

            Sales Order ID:  5921679250664
            Item ID:  162457985
            Item:  Barbie Girls' Selfie 2 Piece Pajama Set, Sizes 4-10";

            var itemId = EmailHelper.ExtractWalmartItemId(message);
            _log.Info("ItemId=" + itemId);
        }

        public void TextExtractShortBody()
        {
            var message = @"Hi Premium Apparel, Lauren Brady wants to connect with you regarding the following order: Sales Order ID: 5921679250664 Item ID: 162457985 Item: Barbie Girls' Selfie 2 Piece Pajama Set, Sizes 4-10 Message: I didn't mean to push the order button. Can I cancel this order? I need to do some checking on sizes and was just looking to see what was there. Thank you, Lauren Brady **Note: You have received this message from the customer, who has made a purchase from your company on Walmart.com. Please respond within 1 business day.** Thank you, Walmart Customer Service Team We will send a copy of your message to the customer, but we will not include your email address in that copy. We will keep a copy of each email sent and received, and we may review them to resolve disputes, improve customer experience, and assess seller performance. By using this messaging service, you agree to our retention and review of your messages. A copy of this message will be sent to walmart@premiumapparel.com RefID: CFB3AF9BD4F64299A86742C482665D44";

            var shortBody = EmailHelper.ExtractShortMessageBody(message, 200, true);
            _log.Info("body=" + shortBody);
        }

        public void GetOrders(WalmartApi api)
        {
            var orders = api.GetOrders(_log, _time.GetAppNowTime().AddDays(-7), null);
            var order = orders.FirstOrDefault(o => o.OrderId == "1577092869434");
            _log.Info(order.ToString());
            _log.Info(orders.ToString());
        }

        public void GetOrder(IWalmartApi api, string orderId)
        {
            var orders = api.GetOrder(_log, orderId);
            _log.Info(orders.ToString());
        }

        public void GetInventory(WalmartApi api)
        {
            var sku = "17CI000XDSZA-";

            var result = api.GetInventory(sku);
            _log.Info(result.ToString());
        }

        public void ConvertPricesTo97()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var listings = (from l in db.Listings.GetAll()
                               join i in db.Items.GetAll() on l.ItemId equals i.Id
                               where i.Market == (int)MarketType.Walmart
                                && i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                                && i.ItemPublishedStatusReason.Contains("Reasonable Price Not Satisfied")
                               select l).ToList();

                foreach (var listing in listings)
                {
                    var floorPrice = Math.Floor(listing.CurrentPrice);
                    if (listing.CurrentPrice - floorPrice == 0.97M)
                    {
                        var newPrice = floorPrice + 0.47M;
                        _log.Info("SKU=" + listing.SKU + ", price: " + listing.CurrentPrice + "=>" + newPrice);
                        listing.CurrentPrice = newPrice;
                        listing.PriceUpdateRequested = true;
                    }
                }

                var itemIdList = listings.Select(l => l.ItemId).ToList();
                var items = db.Items.GetAll().Where(i => itemIdList.Contains(i.Id)).ToList();
                items.ForEach(i => i.ItemPublishedStatus = (int)PublishedStatuses.HasChanges);

                db.Commit();
            }
        }

        public void RepublishInactive()
        {
            var itemRepublishService = new ItemRepublishService(_log, 
                _time,
                _dbFactory);

            itemRepublishService.RepublishInactive();
        }

        public void RepublishWithImageIssue()
        {
            var service = new WalmartListingService(_log,
                _time,
                _dbFactory);

            service.RepublishListingWithImageIssue();
        }

        public void RepublishWithSKUIssue()
        {
            var service = new WalmartListingService(_log,
                _time,
                _dbFactory);

            service.RepublishListingWithSKUIssue();
        }

        public void RepublishWithBarcodeIssue()
        {
            var service = new WalmartListingService(_log,
                _time,
                _dbFactory);

            service.RepublishListingWithBarcodeIssue();
        }

        public void RetireToUnpublishItems(WalmartApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int) MarketType.Walmart
                    && i.PublishedStatus == (int)PublishedStatuses.HasUnpublishRequest)
                    .ToList();

                foreach (var item in items)
                {
                    var result = api.RetireItem(item.SKU);
                    if (result.IsSuccess)
                    {
                        _log.Info("SKU was unpublished=" + item.SKU);
                        var dbItem = db.Items.Get(item.Id);
                        dbItem.ItemPublishedStatus = (int) PublishedStatuses.Unpublished;
                        db.Commit();
                    }
                    else
                    {
                        _log.Info("Unable to unpublish SKU=" + item.SKU);
                    }
                }
            }
        }

        public void SubmitTestPriceFeed(WalmartApi api)
        {
            var item1 = new ItemDTO()
            {
                ListPrice = 32M,
                SKU = "886166911080-8", // "21TE062ERDZA-1-3T",
                CurrentPrice = 17.99M
            };

            //var result = api.GetFeed("20F17E0B509143B89023AD9B8036D158@AQYBAQA");
            //_log.Info(result.ToString());

            //var result = api.SendPrice(item1);
            //_log.Info(result.ToString());

            //steps.NextStep();   


            api.SubmitPriceFeed(Guid.NewGuid().ToString(), new List<ItemDTO>() {item1}, AppSettings.WalmartFeedBaseDirectory);
        }

        public void SendTestPromotion(IWalmartApi api)
        {
            var item1 = new ItemDTO()
            {
                StyleId = 1158,
                SKU = "21TE062ERDZA-1-3T", // "21TE062ERDZA-1-3T",
                CurrentPrice = 19.89M
            };

            using (var db = _dbFactory.GetRWDb())
            {
                if (item1.StyleId.HasValue)
                {
                    var itemStyle = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(
                        item1.StyleId.Value,
                        StyleFeatureHelper.ITEMSTYLE);

                    if (!item1.ListPrice.HasValue && itemStyle != null)
                    {
                        item1.ListPrice = PriceHelper.GetDefaultMSRP(itemStyle.Value);
                    }
                }
            }

            //var result = api.GetFeed("20F17E0B509143B89023AD9B8036D158@AQYBAQA");
            //_log.Info(result.ToString());

            //var result = api.SendPrice(item1);
            //_log.Info(result.ToString());

            api.SendPromotion(item1);
        }

        public void SubmitTestInventoryFeed(WalmartApi api)
        {
            var item1 = new ItemDTO()
            {
                SKU = "K182834PP-3T",
                RealQuantity = 5,
            };

            api.SubmitInventoryFeed("1", new List<ItemDTO>() {item1}, AppSettings.WalmartFeedBaseDirectory);
        }

        public void ReadListingInfo(WalmartApi api)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.UpdateListingInfo();
        }

        public void ReadInventoryInfo(WalmartApi api)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.ReadListingInventory();
        }

        public void ResetNotExistListingQty(IWalmartApi api)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.ResetQtyForNotExistListings();
        }

        public void RetireNotExistListings(IWalmartApi api)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.RetireNotExistListings();
        }


        public void FindSecondDayFlagDisparity(IWalmartApi api, string overrideFeedpath)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.FindSecondDayFlagDisparity(overrideFeedpath);
        }

        public void SubmitPriceFeedByOne(WalmartApi api, IList<string> skuList)
        {
            var feed = new WalmartPriceFeedByOne(_log, _time, api, _dbFactory,
                    AppSettings.WalmartFeedBaseDirectory);

            var feedDto = feed.CheckFeedStatus(TimeSpan.Zero);

            if (feedDto == null) //NOTE: no feed to check
            {
                feed.SubmitFeed(skuList);
            }
        }
        public void SubmitPriceFeed(WalmartApi api, IList<string> skuList)
        {
            var feed = new WalmartPriceFeed(_log, _time, api, _dbFactory,
                    AppSettings.WalmartFeedBaseDirectory);

            var feedDto = feed.CheckFeedStatus(TimeSpan.Zero);

            if (feedDto == null) //NOTE: no feed to check
            {
                feed.SubmitFeed(skuList);
            }
        }

        public void SubmitSecondDayItems(WalmartApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var secondDaySKUs = db.Items.GetAllViewAsDto()
                    .Where(i => i.Market == (int)MarketType.Walmart && i.IsPrime)
                    .OrderBy(i => i.Id)
                    .Select(i => i.SKU)
                    //.Skip(200)
                    //.Take(200)
                    .ToList();
                
                SubmitItemsFeed(api, secondDaySKUs, PublishedStatuses.None);
            }
        }

        public void SubmitItemsWithChildFeed(WalmartApi api, List<string> skuList)
        {
            var allSkuList = new List<string>();
            using (var db = _dbFactory.GetRWDb())
            {
                var parentASINs = (from i in db.Items.GetAll()
                                   join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                   where skuList.Contains(l.SKU)
                                     && l.Market == (int)api.Market
                                     && (l.MarketplaceId == api.MarketplaceId
                                        || String.IsNullOrEmpty(api.MarketplaceId))
                                   select i.ParentASIN).ToList();

                allSkuList = (from i in db.Items.GetAll()
                           join l in db.Listings.GetAll() on i.Id equals l.ItemId
                           where parentASINs.Contains(i.ParentASIN)
                            && i.Market == (int)api.Market
                            && (i.MarketplaceId == api.MarketplaceId
                                || String.IsNullOrEmpty(api.MarketplaceId))
                           select l.SKU).ToList();
            }
            SubmitItemsFeed(api, allSkuList, PublishedStatuses.None);
        }

        public void SubmitItemsFeed(WalmartApi api, 
            List<string> asinList,
            PublishedStatuses overrideStatus)
        {
            //api.GetFeedItems("5C8BE412C2754386B8BEEE11D968AE91@AQMBAQA");

            var feed = new WalmartItemsFeed(_log,
                _time,
                api,
                _dbFactory,
                AppSettings.WalmartFeedBaseDirectory,
                AppSettings.SwatchImageDirectory,
                AppSettings.SwatchImageBaseUrl,
                AppSettings.WalmartImageDirectory,
                AppSettings.WalmartImageBaseUrl);

            var steps = new StepSleeper(TimeSpan.FromMinutes(5), 1);

            var feedDto = feed.CheckFeedStatus(TimeSpan.FromHours(12));

            if (feedDto == null)
                feed.SubmitFeed(asinList, overrideStatus);

            //steps.NextStep();        
        }

        public void SubmitInventoryFeed(WalmartApi api, IList<string> skuList)
        {
            var feed = new WalmartInventoryFeed(_log, _time, api, _dbFactory, AppSettings.WalmartFeedBaseDirectory);

            var steps = new StepSleeper(TimeSpan.FromMinutes(5), 1);

            var feedDto = feed.CheckFeedStatus(TimeSpan.Zero);

            if (feedDto == null)
                feed.SubmitFeed(skuList);

            //steps.NextStep();                
        }

        public void GetFeedItems(WalmartApi api)
        {
            var feedItems = api.GetFeedItems("1725915FF4EF4616AB097CDA1D2908D6@AQkBAAA");
            var item1 = feedItems.Data.FirstOrDefault(i => i.ItemId == "K182277PP-3T");
            var item2 = feedItems.Data.FirstOrDefault(i => i.ItemId == "K182277PP-4T");
            _log.Info(item1.Status);
            _log.Info(item2.Status);
            _log.Info(feedItems.ToString());
        }

        public void GetFeed(WalmartApi api, string feedId)
        {
            var feed = api.GetFeed(feedId);
            _log.Info(feed.ToString());
        }

        public void GetFeedList(WalmartApi api)
        {
            var feeds = api.GetFeeds();
            var lastFeedId = feeds.First().AmazonIdentifier;
            var feedItems = api.GetFeedItems(lastFeedId);
            _log.Info(feedItems.ToString());
        }



        public void GetAllItems(WalmartApi api)
        {
            api.GetAllItems();
        }

        public void GetItemBySKU(WalmartApi api)
        {
            api.GetItemBySKU("US213-3T");
        }

        

        public void GetItemsReport(WalmartApi api)
        {
            var reportPath = api.GetItemsReport(AppSettings.WalmartReportBaseDirectory);
            var report = new WalmartReport(reportPath);
            var items = report.GetItems();
            _log.Info("Items count: " + items.Count);
            _log.Info("Report: " + reportPath);
        }

        public void GetApsentItemsFromReport(WalmartApi api)
        {
            var reportPath = api.GetItemsReport(AppSettings.WalmartReportBaseDirectory);
            var report = new WalmartReport(reportPath);
            var items = report.GetItems();
            _log.Info("Items count: " + items.Count);
            _log.Info("Report: " + reportPath);

            using (var db = _dbFactory.GetRWDb())
            {
                var existListings = db.Listings.GetAll().Where(l => l.Market == (int) MarketType.Walmart).ToList();
                foreach (var item in items)
                {
                    var existListing = existListings.FirstOrDefault(l => l.SKU == item.SKU);
                    if (existListing == null)
                    {
                        _log.Info("SKU=" + item.SKU + ", Qty=" + item.AmazonRealQuantity);
                    }
                }
            }
        }


        public void CreateWalmartListings()
        {
            _autoCreateListingService.CreateListings();
        }

        public void LoadPrices()
        {
            _log.Info("Start importing...");

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/WM_prices_1.csv");
            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });

            using (var db = _dbFactory.GetRWDb())
            {
                while (reader.Read())
                {
                    var priceString = reader.CurrentRecord[3];
                    if (!String.IsNullOrEmpty(priceString))
                        priceString = priceString.Replace(".99", ".97");
                    var newPrice = StringHelper.TryGetDecimal(priceString);
                    
                    var sku = reader.CurrentRecord[6];

                    var listing = db.Listings.GetAll().FirstOrDefault(l => l.Market == 5 && l.SKU == sku);
                    if (newPrice.HasValue
                        && listing.CurrentPrice != newPrice)
                    {
                        listing.CurrentPrice = newPrice.Value;
                        listing.PriceUpdateRequested = true;
                    }
                }

                db.Commit();
            }
        }



        public void CallProcessRefunds(string tag)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var actionService = new SystemActionService(_log, _time);

                var service = new BaseOrderRefundService(_walmartApi, actionService, _emailService, _log, _time);
                service.ProcessRefunds(db, tag);
            }
        }

        public void CallProcessCancellations(string tag)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var actionService = new SystemActionService(_log, _time);

                var service = new WalmartOrderCancellation(_walmartApi, actionService, _log, _time);
                service.ProcessCancellations(db, tag);
            }
        }

        public void CallUpdatFulfillmentData(string orderId)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new WalmartOrderUpdater(_walmartApi, _log, _time);
                service.UpdateOrders(db, orderId);
            }
        }

        public void CallOrderAcknowledgement()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new WalmartOrderAcknowledgement(_walmartApi, _log, _time);
                service.UpdateOrders(db);
            }
        }

        public void CallProcessOrders()
        {
            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Orders, "", MarketType.Walmart, String.Empty);
            var settings = new SettingsService(_dbFactory);
            var dbFactory = new DbFactory();
            var quantityManager = new QuantityManager(_log, _time);
            var priceService = new PriceService(dbFactory);
            var serviceFactory = new ServiceFactory();
            var addressCheckServices = serviceFactory.GetAddressCheckServices(_log,
                _time,
                dbFactory,
                _company.AddressProviderInfoList);
            var companyAddress = new CompanyAddressService(_company);
            var addressService = new AddressService(addressCheckServices, 
                companyAddress.GetReturnAddress(MarketIdentifier.Empty()), 
                companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            var weightService = new WeightService();
            var messageService = new SystemMessageService(_log, _time, dbFactory);

            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                _time,
                _dbFactory,
                weightService,
                _company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);

            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var validatorService = new OrderValidatorService(_log, 
                _dbFactory,
                _emailService, 
                settings,
                orderHistoryService,
                _actionService, 
                priceService,
                _htmlScraper, 
                addressService, 
                companyAddress.GetReturnAddress(MarketIdentifier.Empty()), 
                stampsRateProvider, 
                _time,
                _company);
            
            var actionService = new SystemActionService(_log, _time);
            var cacheService = new CacheService(_log, _time, actionService, quantityManager);

            using (var db = _dbFactory.GetRWDb())
            {
                //if (!syncInfo.IsSyncInProgress())
                {
                    try
                    {
                        syncInfo.SyncBegin(null);

                        var synchronizer = new WalmartOrdersSynchronizer(_log,
                            _walmartApi,
                            _company,
                            settings,
                            syncInfo,
                            rateProviders,
                            quantityManager,
                            _emailService,
                            validatorService,
                            orderHistoryService,
                            cacheService,
                            _actionService,
                            companyAddress,
                            _time,
                            weightService,
                            messageService);

                        synchronizer.Sync(OrderSyncModes.Full, null);
                    }
                    finally
                    {
                        syncInfo.SyncEnd();
                    }
                }
            }
        }
    }
}
