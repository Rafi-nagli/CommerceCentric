using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Shopify;
using Amazon.Model.Implementation.Markets.SupplierOasis;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.Implementation.Trackings;
using Amazon.Web.Models;
using Ccen.Core.Contracts;
using Moq;
using Shopify.Api;
using Supplieroasis.Api;
using Supplieroasis.Api.Models.Schemas.GetOrders;
using Supplieroasis.Api.Models.Schemas.Inventory;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallSupplieroasisProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private CompanyDTO _company;
        private ICacheService _cacheService;
        private IEmailService _emailService;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        private ISystemActionService _actionService;
        private IHtmlScraperService _htmlScraper;
        private IWeightService _weightService;
        private ITrackingNumberService _trackingNumberService;
        private ISystemMessageService _messageService;
        private IAddressService _addressService;

        public CallSupplieroasisProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IEmailService emailService,
            IDbFactory dbFactory,
            CompanyDTO company)
        {
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _trackingNumberService = new TrackingNumberService(log, time, dbFactory);
            _log = log;
            _time = time;
            _emailService = emailService;
            _company = company;

            _weightService = new WeightService();
            _messageService = new SystemMessageService(_log, _time, dbFactory);
            _actionService = new SystemActionService(_log, _time);
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);

            var itemHistoryService = new ItemHistoryService(log, time, dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, dbFactory, cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void SyncItemsFromSample(SupplieroasisApi api)
        {
            var coreApi = new Mock<ISupplieroasisCoreApi>(MockBehavior.Strict);
            var asinsWithErrors = new List<string>();
            coreApi.Setup(p => p.GetItems()).Returns(() =>
            {
                using (var sr = new StringReader(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/OverstockSampleResponses/overstock_get_inventory_response.xml"))))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GetInventoryResponse));
                    return CallResult<GetInventoryResponse>.Success((GetInventoryResponse)serializer.Deserialize(sr));
                }
            });
            api.OverrideCoreApi(coreApi.Object);

            var itemService = new SupplierOasisItemsSync(_log, _time, _dbFactory);
            itemService.ReadItemsInfo(api);
        }

        public void SendOrdersUpdate(SupplieroasisApi api)
        {
            var updater = new BaseOrderUpdater(api, _log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                updater.UpdateOrders(db);
            }
        }

        public void SyncItems(SupplieroasisApi api)
        {
            var itemService = new SupplierOasisItemsSync(_log, _time, _dbFactory);
            itemService.ReadItemsInfo(api);
        }

        public void SendInventoryUpdates(SupplieroasisApi api, IList<string> skuList)
        {
            var itemService = new SupplierOasisItemsSync(_log, _time, _dbFactory);
            itemService.SendInventoryUpdates(api, skuList);
        }

        public void SyncOrders(SupplieroasisApi api)
        {
            var coreApi = new Mock<ISupplieroasisCoreApi>(MockBehavior.Strict);
            var asinsWithErrors = new List<string>();
            coreApi.Setup(p => p.GetOrders(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns((DateTime d1, DateTime d2) =>
            {
                using (var sr = new StringReader(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/OverstockSampleResponses/overstock_get_orders_response.xml"))))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GetOrdersResponse));
                    return CallResult<GetOrdersResponse>.Success((GetOrdersResponse)serializer.Deserialize(sr));
                }
            });
            api.OverrideCoreApi(coreApi.Object);

            var syncInfo = new DbSyncInformer(_dbFactory,
                                _log,
                                _time,
                                SyncType.Orders,
                                api.MarketplaceId,
                                api.Market,
                                String.Empty);
            var settings = new SettingsService(_dbFactory);
            var dbFactory = new DbFactory();
            var quantityManager = new QuantityManager(_log, _time);
            var priceService = new PriceService(dbFactory);
            var companyAddressList = new CompanyAddressService(_company, null);
            var serviceFactory = new ServiceFactory();
            var addressCheckServices = serviceFactory.GetAddressCheckServices(_log,
                _time,
                dbFactory,
                _company.AddressProviderInfoList);
            var addressService = new AddressService(addressCheckServices,
                companyAddressList.GetReturnAddress(MarketIdentifier.Empty()),
                companyAddressList.GetPickupAddress(MarketIdentifier.Empty()));

            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                _time,
                dbFactory,
                _weightService,
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
                companyAddressList.GetReturnAddress(MarketIdentifier.Empty()),
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

                        var orderSyncFactory = new OrderSyncFactory();
                        var synchronizer = orderSyncFactory.GetForMarket(api,
                            _log,
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
                            companyAddressList,
                            _time,
                            _weightService,
                            _messageService);

                        //if (!String.IsNullOrEmpty(orderNumber))
                        //    synchronizer.ProcessSpecifiedOrder(db, orderNumber);
                        //else
                            synchronizer.Sync(Core.Models.Orders.OrderSyncModes.Full, null);
                    }
                    finally
                    {
                        syncInfo.SyncEnd();
                    }
                }
            }
        }


        public void GetOrders(SupplieroasisApi api)
        {
            //var api = new SupplieroasisApi(_log,
            //    _time,
            //    "UFJFTUlVTUFQUEFSRUw6T3ZlcnN0b2NrMSE=",
            //    "Hallandale",
            //    "https://api.test.supplieroasis.com"); 

            var orders = api.GetOrders(_log, _time.GetAppNowTime().AddDays(-14), null);
            _log.Info(orders.Count().ToString());

            //foreach (var order in orders)
            //{
            //    api.SubmitTrackingInfo(order.OrderId,
            //        "785687451763",
            //        null,
            //        null,
            //        "Express",
            //        ShippingTypeCode.Priority,
            //        "FedEx",
            //        _time.GetAppNowTime(),
            //        order.Items.Select(oi => new OrderItemDTO()
            //        {
            //            ItemOrderIdentifier = oi.ItemOrderId,
            //            QuantityOrdered = oi.QuantityOrdered,
            //        }).ToList(),
            //        null);
            //}

            //List<string> asinWithErrors = null;
            //var products = api.GetItems(_log, _time, null, ItemFillMode.Defualt, out asinWithErrors);
            //foreach (var product in products)
            //{
            //    foreach (var child in product.Variations)
            //    {
            //        api.UpdateQuantity(child.SKU, child.Barcode, 5);
            //    }
            //}

        }
    }
}
