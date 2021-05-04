using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Model.Implementation.Trackings;
using Amazon.Web.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Walmart.Api;
using WalmartCA.Api;
using WalmartReport = Walmart.Api.WalmartReport;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallWalmartCAProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private WalmartCAApi _walmartCAApi;
        private CompanyDTO _company;
        private IEmailService _emailService;
        private ISystemActionService _actionService;
        private IItemHistoryService _itemHistoryService;
        private IHtmlScraperService _htmlScraper;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallWalmartCAProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IDbFactory dbFactory,
            IEmailService emailService,
            WalmartCAApi walmartCAApi,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _walmartCAApi = walmartCAApi;
            _company = company;
            _emailService = emailService;

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _actionService = new SystemActionService(_log, _time);
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _autoCreateListingService = new AutoCreateWalmartCAListingService(log, time, dbFactory, _cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
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
                    orders = new List<DTOOrder>() { _walmartCAApi.GetOrder(_log, "2576707987362") };

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

        public void ProcessCancellation(WalmartApi api)
        {
            var actionService = new SystemActionService(_log, _time);
            var updater = new WalmartOrderCancellation(api, actionService, _log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                updater.ProcessCancellations(db);
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

        public void GetOrders(WalmartCAApi api, string orderId)
        {
            IList<DTOOrder> orders;
            if (!String.IsNullOrEmpty(orderId))
                orders = new List<DTOOrder>() {api.GetOrder(_log, orderId)};
            else
                orders = api.GetOrders(_log, _time.GetAppNowTime().AddDays(-5), null).ToList();
            var order = orders.FirstOrDefault(o => o.MarketOrderId == orderId);
            _log.Info(order.ToString());
            _log.Info(orders.ToString());
        }

        public void GetInventory(WalmartCAApi api)
        {
            var sku = "17CI000XDSZA-";

            var result = api.GetInventory(sku);
            _log.Info(result.ToString());
        }

        public void RepublishInactive()
        {
            var itemRepublishService = new ItemRepublishService(_log, 
                _time,
                _dbFactory);

            itemRepublishService.RepublishInactive();
        }


        public void RetireUnpublishedItems(WalmartCAApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int) MarketType.WalmartCA
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
                }
            }
        }

        public void SubmitTestPriceFeed(WalmartCAApi api)
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

            api.SubmitPriceFeed("1", new List<ItemDTO>() {item1}, AppSettings.WalmartFeedBaseDirectory);
        }

        public void SubmitTestInventoryFeed(WalmartCAApi api)
        {
            var item1 = new ItemDTO()
            {
                SKU = "K182834PP-3T",
                RealQuantity = 5,
            };

            api.SubmitInventoryFeed("1", new List<ItemDTO>() {item1}, AppSettings.WalmartFeedBaseDirectory);
        }

        public void UpdateListingInfo(WalmartCAApi api, string reportPath)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.UpdateListingInfo(reportPath);
        }

        public void ResetNotExistListingQty(WalmartCAApi api, string reportPath)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.ResetQtyForNotExistListings(reportPath);
        }

        public void SubmitPriceFeed(WalmartCAApi api, IList<string> skuList)
        {
            var feed = new WalmartPriceFeedByOne(_log, _time, api, _dbFactory,
                    AppSettings.WalmartFeedBaseDirectory);

            //using (var db = _dbFactory.GetRWDb())
            //{
            //    skuList =
            //        db.Items.GetAll().Where(i => i.Market == (int) MarketType.WalmartCA).Select(i => i.ASIN).ToList();
            //}
            feed.SubmitFeed(skuList);
        }

        public void SubmitItemsFeed(WalmartCAApi api, List<string> asinList, PublishedStatuses overrideItemStatus)
        {
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

            var feedDto = feed.CheckFeedStatus(TimeSpan.Zero);

            if (feedDto == null)
                feed.SubmitFeed(asinList, overrideItemStatus);

            //steps.NextStep();                
        }

        public void SubmitInventoryFeed(WalmartCAApi api)
        {
            var feed = new WalmartInventoryFeed(_log, _time, api, _dbFactory, AppSettings.WalmartFeedBaseDirectory);

            var steps = new StepSleeper(TimeSpan.FromMinutes(5), 1);

            var feedDto = feed.CheckFeedStatus(TimeSpan.Zero);

            if (feedDto == null)
                feed.SubmitFeed();

            //steps.NextStep();                
        }

        public void GetFeedItems(WalmartCAApi api)
        {
            var feedItems = api.GetFeedItems("1725915FF4EF4616AB097CDA1D2908D6@AQkBAAA");
            var item1 = feedItems.Data.FirstOrDefault(i => i.ItemId == "K182277PP-3T");
            var item2 = feedItems.Data.FirstOrDefault(i => i.ItemId == "K182277PP-4T");
            _log.Info(item1.Status);
            _log.Info(item2.Status);
            _log.Info(feedItems.ToString());
        }

        public void GetFeed(WalmartCAApi api, string feedId)
        {
            var feed = api.GetFeed(feedId);
            _log.Info(feed.ToString());
        }

        public void GetFeedList(WalmartCAApi api)
        {
            var feeds = api.GetFeeds();
            var lastFeedId = feeds.First().AmazonIdentifier;
            var feedItems = api.GetFeedItems(lastFeedId);
            _log.Info(feedItems.ToString());
        }



        public void GetAllItems(WalmartCAApi api)
        {
            api.GetAllItems();
        }

        public void GetItemBySKU(WalmartCAApi api)
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

                var service = new BaseOrderRefundService(_walmartCAApi, actionService, _emailService, _log, _time);
                service.ProcessRefunds(db, tag);
            }
        }

        public void CallProcessCancellations()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var actionService = new SystemActionService(_log, _time);

                var service = new WalmartOrderCancellation(_walmartCAApi, actionService, _log, _time);
                service.ProcessCancellations(db);
            }
        }

        public void CallUpdateFulfillmentData(string orderString)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new WalmartOrderUpdater(_walmartCAApi, _log, _time);
                service.UpdateOrders(db, orderString);
            }
        }

        public void CallOrderAcknowledgement()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new WalmartOrderAcknowledgement(_walmartCAApi, _log, _time);
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
            var addressService = new AddressService(addressCheckServices, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            
            var weightService = new WeightService();
            var messageService = new SystemMessageService(_log, _time, _dbFactory);

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
            var validatorService = new OrderValidatorService(_log, _dbFactory, _emailService, settings, orderHistoryService, _actionService, priceService, _htmlScraper, addressService, 
                companyAddress.GetReturnAddress(MarketIdentifier.Empty()), stampsRateProvider, _time, _company);
            
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
                            _walmartCAApi,
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
                            null);

                        synchronizer.Sync(OrderSyncModes.Full, null);
                    }
                    finally
                    {
                        syncInfo.SyncEnd();
                    }
                }
            }
        }

        public void ResetNotExistListingQty(IWalmartApi api, string overrideReportPath)
        {
            var service = new WalmartListingInfoReader(_log, _time, api, _dbFactory,
                _actionService,
                _itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);
            service.ResetQtyForNotExistListings(overrideReportPath);
        }
    }
}
