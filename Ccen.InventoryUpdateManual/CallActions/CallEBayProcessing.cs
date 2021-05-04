using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Items;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.InventoryUpdateManual.Models;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Model.Implementation.Trackings;
using Amazon.Templates;
using Amazon.Web.Models;
using eBay.Api;
using Jet.Api;

namespace Amazon.InventoryUpdateManual.CallActions
{
    class CallEBayProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IStyleManager _styleManager;
        private string _descriptionTemplatePath;
        private string _descriptionMultiListingTemplatePath;
        private eBayApi _eBayApi;

        private CompanyDTO _company;
        private IHtmlScraperService _htmlScraper;
        private IEmailService _emailService;
        private ISystemActionService _actionService;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallEBayProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IDbFactory dbFactory,
            IStyleManager styleManager,
            eBayApi eBayApi,
            IEmailService emailService,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _styleManager = styleManager;
            _descriptionTemplatePath = Path.Combine(AppSettings.TemplateDirectory, TemplateHelper.EBayDescriptionTemplateName);
            _descriptionMultiListingTemplatePath = Path.Combine(AppSettings.TemplateDirectory, TemplateHelper.EBayDescriptionMultiListingTemplateName);

            _eBayApi = eBayApi;
            _company = company;
            _emailService = emailService;

            _actionService = new SystemActionService(_log, _time);
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _autoCreateListingService = new AutoCreateEBayListingService(_log, _time, dbFactory, cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }


        public void FindListingsSuitableForPromotion()
        {
            var today = _time.GetAppNowTime().Date;
            using (var db = _dbFactory.GetRWDb())
            {
                var parentItems = db.ParentItems.GetAllAsDto(MarketType.eBay, MarketplaceKeeper.eBayAll4Kids).ToList();
                var items = db.Items.GetAllViewActual().Where(i => i.Market == (int) MarketType.eBay).ToList();
                foreach (var parentItem in parentItems)
                {
                    var childItems = items.Where(i => i.ParentASIN == parentItem.ASIN).ToList();
                    if (childItems.Count > 0 && childItems.All(i => i.SalePrice.HasValue && i.SaleStartDate <= today))
                    {
                        var discountValue = childItems[0].CurrentPrice - childItems[0].SalePrice.Value;
                        if (childItems.All(i => i.CurrentPrice - i.SalePrice.Value == discountValue))
                        {
                            _log.Info("Enable discount, ParentASIN=" + parentItem.ASIN);
                            //TODO: set update price flag
                            var listingIds = childItems.Select(l => l.ListingEntityId).ToList();
                            var dbListings = db.Listings.GetAll().Where(l => listingIds.Contains(l.Id)).ToList();
                            foreach (var dbListing in dbListings)
                            {
                                dbListing.PriceUpdateRequested = true;
                            }
                        }
                    }
                    db.Commit();
                }
            }
        }

        public void SyncItemsInfo(eBayApi api, IList<string> itemIds)
        {
            var sync = new eBayItemsSync(_log,
                _time,
                api,
                _dbFactory,
                _styleManager,
                AppSettings.eBayImageDirectory,
                AppSettings.eBayImageBaseUrl,
                AppSettings.LabelDirectory);

            sync.SyncItemsInfo(itemIds); 
        }

        public void SubmitItemsFromFile(eBayApi api, string filename)
        {
            var sourceSkus = ExcelReader.LoadSKUs(filename, 0).Select(i => i.SKU).ToList();
            var skus = new List<string>();
            using (var db = _dbFactory.GetRWDb())
            {
                var eBaySkus = db.Listings.GetAll().Where(l => l.Market == (int)MarketType.eBay)
                    .Where(l => !String.IsNullOrEmpty(l.SKU))
                    .Select(l => l.SKU)
                    .ToList();
                skus = eBaySkus.Where(s => sourceSkus.Any(so => s.StartsWith(so))).ToList();
            }
            _log.Info("Count: " + skus.Count());
            SubmitItems(api, skus);
        }

        public void SubmitItems(eBayApi api, IList<string> skuList)
        {
            List<long> itemIds = null;
            if (skuList != null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    //itemIds =
                    //    db.Items.GetAll().Where(i => i.ItemPublishedStatus == (int)PublishedStatuses.Published
                    //                                 && i.Market == (int)MarketType.eBay)
                    //        .Select(i => i.Id)
                    //        .ToList()
                    //        .Select(l => (long)l)
                    //    .ToList();

                    itemIds = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)
                                                              && l.Market == (int)MarketType.eBay)
                        .Select(l => l.ItemId)
                        .ToList()
                        .Select(l => (long)l)
                        .ToList();
                }
            }

            var sync = new eBayItemsSync(_log,
                _time,
                api,
                _dbFactory,
                _styleManager,
                AppSettings.eBayImageDirectory,
                AppSettings.eBayImageBaseUrl,
                AppSettings.LabelDirectory);

            sync.SendItemUpdates(itemIds);
        }

        public void Republish(eBayApi api)
        {
            var service = new ItemRepublishService(_log, _time, _dbFactory);
            service.PublishUnpublishedListings(MarketType.eBay, null);
        }

        public void SubmitInventory(eBayApi api, IList<string> skuList)
        {
            List<long> itemIds = null;

            if (skuList != null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    //itemIds =
                    //    db.Items.GetAll().Where(i => i.ItemPublishedStatus == (int)PublishedStatuses.Published
                    //                                 && i.Market == (int)MarketType.eBay)
                    //        .Select(i => i.Id)
                    //        .ToList()
                    //        .Select(l => (long)l)
                    //    .ToList();

                    itemIds = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)
                                                                  && l.Market == (int)api.Market
                                                                  && l.MarketplaceId == api.MarketplaceId)
                        .Select(l => l.ItemId)
                        .ToList()
                        .Select(l => (long)l)
                        .ToList();
                }
            }

            var sync = new eBayItemsSync(_log,
                _time,
                api,
                _dbFactory,
                _styleManager,
                AppSettings.eBayImageDirectory,
                AppSettings.eBayImageBaseUrl,
                AppSettings.LabelDirectory);

            sync.SendInventoryUpdates(itemIds);
        }

        public void SubmitPrice(eBayApi api)
        {
            var sync = new eBayItemsSync(_log,
                 _time,
                 api,
                 _dbFactory,
                 _styleManager,
                 AppSettings.eBayImageDirectory,
                 AppSettings.eBayImageBaseUrl,
                 AppSettings.LabelDirectory);

            sync.SendPriceUpdates();
        }

        public void CallUpdateParentItemEBay(IMarketApi api, string parentAsin)
        {
            var list = new List<string>() { parentAsin };
            var parentASINsWithError = new List<string>();
            var parents = api.GetItems(_log, _time, MarketItemFilters.Build(list), ItemFillMode.Defualt, out parentASINsWithError);
        }

        public void CreateEBayListings()
        {
            _autoCreateListingService.CreateListings();
        }



        public void CallGetEBaySpecificOrders(List<string> orderIds)
        {
            var result = _eBayApi.GetOrders(_log, orderIds);
            var asinsWithError = new List<string>();
            _eBayApi.GetItems(_log, _time, MarketItemFilters.Build(new List<string>() { result.First().Items[0].ParentASIN }), ItemFillMode.Defualt, out asinsWithError);
            Console.WriteLine("count=" + result.Count());
        }

        public void CallGetItem(string itemId)
        {
            var asinsWithError = new List<string>();
            var diffStyleItems = new List<ParentItemDTO>();
            var apiItem = _eBayApi.GetItems(_log, _time, MarketItemFilters.Build(new List<string>() { itemId }), ItemFillMode.Defualt, out asinsWithError);
            if (apiItem != null && apiItem.Any())
            {
                Console.WriteLine(apiItem);
            }
        }

        public void CallGetAllEBayItem()
        {
            var diffStyleItems = new List<ParentItemDTO>();
            using (var db = new UnitOfWork(_log))
            {
                var items = db.ParentItems.GetAllAsDto(MarketType.eBay, "");
                foreach (var item in items)
                {
                    try
                    {
                        var asinsWithError = new List<string>();
                        var apiItem = _eBayApi.GetItems(_log, _time, MarketItemFilters.Build(new List<string>() { item.ASIN }), ItemFillMode.Defualt, out asinsWithError);
                        if (apiItem != null && apiItem.Any())
                        {
                            if (apiItem.First().Variations != null)
                            {
                                var apiStyleString = apiItem.First().Variations.First().StyleString;
                                var childItems = db.Items.GetByParentASINAsDto(MarketType.eBay, "", item.ASIN);
                                if (childItems.Any())
                                {
                                    if (childItems.First().StyleString != apiStyleString)
                                        diffStyleItems.Add(item);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            Console.WriteLine("Result: " + diffStyleItems.Count());
        }


        public void CallProcessEBayOrders(string orderNumber)
        {
            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Orders, "", MarketType.eBay, String.Empty);
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
                dbFactory,
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

                        var synchronizer = new EBayOrdersSynchronizer(_log,
                            _eBayApi,
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

                        if (String.IsNullOrEmpty(orderNumber))
                            synchronizer.Sync(OrderSyncModes.Full, null);
                        else
                            synchronizer.ProcessSpecifiedOrder(db, orderNumber);
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
