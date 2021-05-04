using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Jet;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Web.Models;
using Jet.Api;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallJetProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private JetApi _jetApi;
        private CompanyDTO _company;
        private IEmailService _emailService;
        private ISystemActionService _actionService;
        private IHtmlScraperService _htmlScraper;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallJetProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IDbFactory dbFactory,
            IEmailService emailService,
            JetApi jetApi,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _jetApi = jetApi;
            _company = company;
            _emailService = emailService;

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _actionService = new SystemActionService(_log, _time);
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);
            _barcodeService = new BarcodeService(log, _time, _dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, dbFactory, cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void GetOrders(JetApi api)
        {
            var orders = api.GetOrders(_log, DateTime.Now.AddDays(-7), new List<string>() {OrderStatusEnumEx.Pending});
            _log.Info("GetOrders: " + orders.Count());
        }

        public void GetItemBySku(JetApi api)
        {
            var sku = "MK210-4T";
            var item = api.GetProduct(sku);
            _log.Info(item.ToString());
        }

        public void SubmitItems(JetApi api)
        {
            var sync = new JetItemsSync(_log,
                _time,
                api,
                _dbFactory,
                AppSettings.JetImageDirectory,
                AppSettings.JetImageBaseUrl);
            
            sync.SendItemUpdates();
        }

        public void SubmitInventory(JetApi api)
        {
            var sync = new JetItemsSync(_log,
                _time,
                api,
                _dbFactory,
                AppSettings.JetImageDirectory,
                AppSettings.JetImageBaseUrl);

            sync.SendInventoryUpdates();
        }

        public void SubmitPrice(JetApi api)
        {
            var sync = new JetItemsSync(_log,
                _time,
                api,
                _dbFactory,
                AppSettings.JetImageDirectory,
                AppSettings.JetImageBaseUrl);

            sync.SendPriceUpdates();
        }



        public void CreateJetListings()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
                                   group siCache by siCache.StyleId
                                   into byStyle
                                   select new
                                   {
                                       StyleId = byStyle.Key,
                                       Qty = byStyle.Sum(s => s.RemainingQuantity)
                                   };

                var query = from s in db.Styles.GetAll()
                            join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
                            join qty in groupByStyle on s.Id equals qty.StyleId
                            orderby qty.Qty descending
                            where !s.Deleted
                            select new
                            {
                                StyleId = s.Id,
                                StyleString = s.StyleID,
                                ParentASIN = sCache.AssociatedASIN,
                                Market = sCache.AssociatedMarket,
                                MarketplaceId = sCache.AssociatedMarketplaceId,
                                Qty = qty.Qty
                            };

                var styleInfoList = query
                    .Where(p => p.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                        && p.Qty > 100)
                    .Take(1000)
                    .ToList();

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.Jet).ToList();

                var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

                var styleBlackList = new string[]
                {
                    //"21FZ045TBFDZ" //Created manually
                };

                IList<MessageString> messages;
                foreach (var styleInfo in newStyleInfoes)
                {
                    _log.Info("Creating styleId=" + styleInfo.StyleString + " (" + styleInfo.StyleId + ")" + ", ASIN=" +
                              styleInfo.ParentASIN);
                    if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
                    {
                        _log.Info("Skipped, style already exists");
                        continue;
                    }

                    //NOTE: temporary skip
                    if (styleBlackList.Contains(styleInfo.StyleString))
                        continue;
                    

                    var model = _autoCreateListingService.CreateFromParentASIN(db,
                        styleInfo.ParentASIN,
                        styleInfo.Market.Value,
                        styleInfo.MarketplaceId,
                        false,
                        false,
                        0,
                        out messages);

                    if (model.Variations == null)
                    {
                        _log.Info("Skipped, variations is NULL");
                        continue;
                    }

                    if (model.Variations.Count == 0)
                    {
                        _log.Info("Skipped, no variations");
                        continue;
                    }

                    if (model.Variations.Count > 12)
                    {
                        _log.Info("Skipped, a lot of variations");
                        continue;
                    }

                    model.Market = (int)MarketType.Jet;
                    model.MarketplaceId = null;

                    _autoCreateListingService.PrepareData(model);
                    _autoCreateListingService.Save(model, null, db, _time.GetAppNowTime(), null);

                    //Add to exist list the new items
                    existMarketItems.AddRange(model.Variations.Select(i => new Item()
                    {
                        StyleId = i.StyleId
                    }));
                }
            }
        }




        //public void CallProcessRefunds(string tag)
        //{
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        var actionService = new SystemActionService(_log, _time);

        //        var service = new JetOrderRefund(_walmartApi, actionService, _emailService, _log, _time);
        //        service.ProcessRefunds(db, tag);
        //    }
        //}

        public void CallProcessCancellations()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var actionService = new SystemActionService(_log, _time);

                var service = new JetOrderCancellation(_jetApi, actionService, _log, _time);
                service.ProcessCancellations(db);
            }
        }

        public void CallProcessReturns()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var actionService = new SystemActionService(_log, _time);

                var service = new JetOrderReturn(_jetApi, actionService, _emailService, _log, _time);
                service.ProcessReturns(db);
            }
        }

        public void CallCompleteReturns(JetApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var actionService = new SystemActionService(_log, _time);
                var service = new BaseOrderRefundService(api, actionService, _emailService, _log, _time);
                service.ProcessRefunds(db, null);
            }
        }

        public void CallUpdateFulfillmentData()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new JetOrderUpdater(_jetApi, _log, _time);
                service.UpdateOrders(db);
            }
        }

        public void CallOrderAcknowledgement()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new JetOrderAcknowledgement(_jetApi, _log, _time);
                service.UpdateOrders(db);
            }
        }

        public void CallProcessOrders()
        {
            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Orders, "", MarketType.Jet, String.Empty);
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
            var messageService = new SystemMessageService(_log, _time, dbFactory);

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

                        var synchronizer = new JetOrdersSynchronizer(_log,
                            _jetApi,
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
