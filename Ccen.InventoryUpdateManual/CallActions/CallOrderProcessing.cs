using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.InventoryUpdateManual.Models;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Markets.Amazon.Feeds;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.Implementation.Validation;
using Amazon.Model.SyncService.Threads.Simple;
using Amazon.Web.Models;
using eBay.Api;
using Magento.Api.Wrapper;
using Shopify.Api;
using Walmart.Api;
using SettingsBuilder = Amazon.InventoryUpdateManual.Models.SettingsBuilder;

namespace Amazon.InventoryUpdateManual.TestCases
{
    public class CallOrderProcessing
    {
        private eBayApi _eBayApi;
        private Magento20MarketApi _magentoApi;
        private WalmartApi _walmartApi;
        private AmazonApi _amazonApiCom;
        private AmazonApi _amazonApiCA;

        private ILogService _log;
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ISystemActionService _actionService;
        private ITime _time;
        private CompanyDTO _company;

        private IHtmlScraperService _htmlScraper;
        private IWeightService _weightService;
        private ISystemMessageService _messageService;
        private IAddressService _addressService;

        public CallOrderProcessing(eBayApi eBayApi,
            Magento20MarketApi magentoApi,
            WalmartApi walmartApi,
            AmazonApi amazonApiCom,
            AmazonApi amazonApiCA,
            ILogService log,
            IDbFactory dbFactory,
            IEmailService emailService,
            ITime time,
            CompanyDTO company)
        {
            _eBayApi = eBayApi;
            _amazonApiCA = amazonApiCA;
            _amazonApiCom = amazonApiCom;
            _magentoApi = magentoApi;
            _walmartApi = walmartApi;

            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _company = company;
            _emailService = emailService;

            _weightService = new WeightService();
            _messageService = new SystemMessageService(_log, _time, _dbFactory);
            _actionService = new SystemActionService(_log, _time);
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);

            _addressService = emailService.AddressService;
        }

        public void CallOrdersSyncThread(IMarketApi api)
        {
            var thread = new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiGroupon", api, _company.Id, null, TimeSpan.FromMinutes(15));
            thread.Run();
        }

        public void MoveToOrderPageUnprintedOrders()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIdsToMove = (from o in db.Orders.GetAll()
                                      join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                      where o.BatchId.HasValue
                                         && o.OrderStatus == OrderStatusEnumEx.Unshipped
                                         && sh.IsActive
                                         //&& !String.IsNullOrEmpty(sh.LabelPurchaseMessage)
                                         && sh.LabelPurchaseResult == (int)LabelPurchaseResultType.Error
                                      select o.Id)
                                      .Distinct()
                                      .ToList();
                var dbOrdersToMove = db.Orders.GetAll().Where(o => orderIdsToMove.Contains(o.Id)).ToList();
                _log.Info("Orders to move: " + dbOrdersToMove.Count);
                _log.Info("OrderIds to move: " + String.Join(", ", dbOrdersToMove.Select(o => o.AmazonIdentifier).ToList()));
                dbOrdersToMove.ForEach(o => o.BatchId = null);
                db.Commit();
            }
        }

        public void UpdateIsFulfilledFlag(IMarketOrderUpdaterApi api)
        {
            var service = new BaseOrderUpdater(api, _log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                service.ResetIsFulfilledStatus(db);
            }
        }

        public void ChangeShippingOption()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIds = (from o in db.Orders.GetAll()
                                             join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                             join oi in db.OrderItems.GetAll() on o.Id equals oi.OrderId
                                             where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                                 && sh.IsActive
                                                 && (sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope
                                                     || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId)
                                                 && (oi.StyleItemId == 28705
                                                     || oi.StyleItemId == 28706)
                                             select o.Id)
                                             .Distinct()
                                             .ToList();

                foreach (var orderId in orderIds)
                {
                    _log.Info("Processing orderId: " + orderId);

                    var shippings = db.OrderShippingInfos.GetAll().Where(sh => sh.OrderId == orderId).ToList();
                    var actionShipping = shippings.FirstOrDefault(sh => sh.IsActive);

                    if (!String.IsNullOrEmpty(actionShipping.TrackingNumber))
                    {
                        _log.Info("Exist tracking number");
                        continue;
                    }
                    if (actionShipping.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope)
                    {
                        var newActiveShipping = shippings.FirstOrDefault(sh => sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak);
                        newActiveShipping.IsActive = true;
                        actionShipping.IsActive = false;
                        db.Commit();
                    }
                    if (actionShipping.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId)
                    {
                        var newActiveShipping = shippings.FirstOrDefault(sh => sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId);
                        newActiveShipping.IsActive = true;
                        actionShipping.IsActive = false;
                        db.Commit();
                    }
                }
            }
        }

        public void CallCheckOverdue()
        {
            //Checking email service, sent test message
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(_company);
            var emailService = new EmailService(_log, emailSmtpSettings, _addressService);

            var checker = new AlertChecker(_log, _time, _dbFactory, emailService, _company);
            using (var db = _dbFactory.GetRWDb())
            {
                checker.CheckOverdue(db, _time.GetAppNowTime(), TimeSpan.FromHours(2));
            }
        }

        public void CallCheckOrderItemPrices()
        {

        }

        public void CallUpdateRates(string orderNumber)
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.Orders.GetByOrderNumber(orderNumber);
                var orderIdList = new[] { order.Id };
                DTOOrder dtoOrder = db.ItemOrderMappings.GetSelectedOrdersWithItems(_weightService, orderIdList, includeSourceItems: true).FirstOrDefault();
                                
                var markets = new MarketplaceKeeper(_dbFactory, false);
                markets.Init();
                var companyAddress = new CompanyAddressService(_company, markets.GetAll());
                var synchronizer = new AmazonOrdersSynchronizer(_log,
                    _company,
                    syncInfo,
                    rateProviders,
                    companyAddress,
                    _time,
                    _weightService,
                    _messageService);
                var result = synchronizer.UIUpdate(db, dtoOrder, false, false, keepCustomShipping: false, switchToMethodId: null);
                _log.Info(result.ToString());
            }
        }

        public void ChangeOrderProviderToDhlECom()
        {
            var dhlAutoSwitchService = new DhlECommerceSwitchService(_log,
                _time,
                _company,
                _dbFactory,
                _emailService,
                _weightService,
                _messageService);

            dhlAutoSwitchService.SwitchToECommerce();
        }

        public void ChangeOrderProviderToStamps()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

            var companyAddressList = new CompanyAddressService(_company);
            var synchronizer = new AmazonOrdersSynchronizer(_log,
                        _company,
                        syncInfo,
                        rateProviders,
                        companyAddressList,
                        _time,
                        _weightService,
                        _messageService);

            using (var db = _dbFactory.GetRWDb())
            {
                var orderIds = (from o in db.Orders.GetAll()
                                join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                     && o.ShippingState == "FL"
                                     && o.InitialServiceType == "Standard"
                                     && o.ShipmentProviderType == (int)ShipmentProviderType.Amazon
                                select o.Id).ToList();
                
                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetFilteredOrdersWithItems(_weightService, new OrderSearchFilter()
                {
                    EqualOrderIds = orderIds.ToArray(),
                }).ToList();// orderIds.ToArray(), includeSourceItems: true).ToList();

                //IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetFilteredOrdersWithItems(new OrderSearchFilter()
                //{
                //    BatchId = 1461,
                //    OrderStatus = OrderStatusEnumEx.AllUnshipped
                //}).ToList()
                //.Where(o => o.LatestShipDate < new DateTime(2017, 2, 3, 6, 0, 0))
                //.ToList();// orderIds.ToArray(), includeSourceItems: true).ToList();

                dtoOrders = dtoOrders.Where(o => o.WeightD < 16.9).ToList();



                foreach (var dtoOrder in dtoOrders)
                {
                    _log.Info("Change provider for order: " + dtoOrder.OrderId + " (" + dtoOrder.SourceShippingService + ")");
                    //Update into DB, after success update
                    var order = db.Orders.GetById(dtoOrder.Id);
                    if (order.ShipmentProviderType == (int) ShipmentProviderType.Amazon
                        && !ShippingUtils.IsServiceTwoDays(order.SourceShippingService)
                        && !ShippingUtils.IsServiceSameDay(order.SourceShippingService))
                    {
                        order.ShipmentProviderType = (int) ShipmentProviderType.Stamps;
                        db.Commit();
                        dtoOrder.ShipmentProviderType = (int) ShipmentProviderType.Stamps;

                        if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, switchToMethodId: null))
                        {
                            _log.Info("Success");
                        }
                        else
                        {
                            _log.Info("Failed");
                        }
                    }
                }
            }
        }

        public void ChangeOrderProviderToSkyPostal()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);
            var companyAddressList = new CompanyAddressService(_company);
            var synchronizer = new AmazonOrdersSynchronizer(_log,
                        _company,
                        syncInfo,
                        rateProviders,
                        companyAddressList,
                        _time,
                        _weightService,
                        _messageService);

            using (var db = _dbFactory.GetRWDb())
            {
                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetFilteredOrdersWithItems(_weightService, new OrderSearchFilter()
                {
                    OrderStatus = new string[] { OrderStatusEnumEx.Unshipped },
                }).ToList();// orderIds.ToArray(), includeSourceItems: true).ToList();

                //dtoOrders = dtoOrders.Where(o => o.ShipmentProviderType != (int)ShipmentProviderType.IBC).ToList();

                foreach (var dtoOrder in dtoOrders)
                {
                    if (dtoOrder.ShipmentProviderType != (int)ShipmentProviderType.IBC)
                        continue;

                    //if (dtoOrder.LatestShipDate.Value.Date > DateTime.Today.AddHours(30))
                    //    continue;

                    if (dtoOrder.FinalShippingCountry != "CA")
                        continue;

                    _log.Info("Change provider for order: " + dtoOrder.OrderId + " (" + dtoOrder.SourceShippingService + ")");
                    //Update into DB, after success update
                    var order = db.Orders.GetById(dtoOrder.Id);
                    order.ShipmentProviderType = (int)ShipmentProviderType.SkyPostal;
                    db.Commit();
                    dtoOrder.ShipmentProviderType = (int)ShipmentProviderType.SkyPostal;

                    if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, switchToMethodId: null))
                    {
                        _log.Info("Success");
                    }
                    else
                    {
                        _log.Info("Failed");
                    }
                }
            }
        }

        public void ChangeOrderProviderToIBC()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);
            var companyAddressList = new CompanyAddressService(_company);
            var synchronizer = new AmazonOrdersSynchronizer(_log,
                        _company,
                        syncInfo,
                        rateProviders,
                        companyAddressList,
                        _time,
                        _weightService,
                        _messageService);

            using (var db = _dbFactory.GetRWDb())
            {
                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetFilteredOrdersWithItems(_weightService, new OrderSearchFilter()
                {
                    OrderStatus = new[] { OrderStatusEnumEx.Unshipped }
                }).ToList();// orderIds.ToArray(), includeSourceItems: true).ToList();

                //dtoOrders = dtoOrders.Where(o => o.ShipmentProviderType != (int)ShipmentProviderType.IBC).ToList();

                foreach (var dtoOrder in dtoOrders)
                {
                    if (dtoOrder.ShipmentProviderType != (int)ShipmentProviderType.SkyPostal)
                        continue;

                    _log.Info("Change provider for order: " + dtoOrder.OrderId + " (" + dtoOrder.SourceShippingService + ")");
                    //Update into DB, after success update
                    var order = db.Orders.GetById(dtoOrder.Id);
                    order.ShipmentProviderType = (int)ShipmentProviderType.IBC;
                    db.Commit();
                    dtoOrder.ShipmentProviderType = (int)ShipmentProviderType.IBC;

                    if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, switchToMethodId: null))
                    {
                        _log.Info("Success");
                    }
                    else
                    {
                        _log.Info("Failed");
                    }
                }
            }
        }

        public void ChangeOrderProviderToAuto()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);
            var companyAddressList = new CompanyAddressService(_company);
            var synchronizer = new AmazonOrdersSynchronizer(_log,
                        _company,
                        syncInfo,
                        rateProviders,
                        companyAddressList,
                        _time,
                        _weightService,
                        _messageService);

            using (var db = _dbFactory.GetRWDb())
            {
                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetDisplayOrdersWithItems(_weightService, new OrderSearchFilter()
                {
                    OrderStatus = new string[] {  OrderStatusEnumEx.Unshipped },
                    Market = MarketType.Groupon
                }).Items;

                //dtoOrders = dtoOrders.Where(o => o.ShipmentProviderType != (int)ShipmentProviderType.IBC).ToList();

                foreach (var dtoOrder in dtoOrders)
                {
                    if (dtoOrder.ShipmentProviderType != (int)ShipmentProviderType.FedexSmartPost)
                        continue;

                    _log.Info("Change provider for order: " + dtoOrder.OrderId + " (" + dtoOrder.SourceShippingService + ")");
                    //Update into DB, after success update
                    var order = db.Orders.GetById(dtoOrder.Id);
                    order.ShipmentProviderType = (int)ShipmentProviderType.None;
                    db.Commit();
                    dtoOrder.ShipmentProviderType = (int)ShipmentProviderType.None;

                    if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, switchToMethodId: null))
                    {
                        _log.Info("Success");
                    }
                    else
                    {
                        _log.Info("Failed");
                    }
                }
            }
        }

        public void ChangeOrderProviderToFedexOneRate()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);
            var companyAddressList = new CompanyAddressService(_company);
            var synchronizer = new AmazonOrdersSynchronizer(_log,
                        _company,
                        syncInfo,
                        rateProviders,
                        companyAddressList,
                        _time,
                        _weightService,
                        _messageService);

            using (var db = _dbFactory.GetRWDb())
            {
                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetDisplayOrdersWithItems(_weightService, new OrderSearchFilter()
                {
                    //OrderStatus = new string[] { OrderStatusEnumEx.Unshipped },
                    BatchId = 13789
                }).Items;// orderIds.ToArray(), includeSourceItems: true).ToList();

                //dtoOrders = dtoOrders.Where(o => o.ShipmentProviderType != (int)ShipmentProviderType.IBC).ToList();

                foreach (var dtoOrder in dtoOrders)
                {
                    if (ShippingUtils.IsInternational(dtoOrder.FinalShippingCountry))
                        continue;

                    if (ShippingUtils.IsInternationalState(dtoOrder.FinalShippingState))
                        continue;

                    if (dtoOrder.ShipmentProviderType != 3)
                        continue;

                    if (dtoOrder.ShippingInfos.Count(sh => sh.IsActive) > 1)
                        continue;


                    var activeShipping = dtoOrder.ShippingInfos.FirstOrDefault(sh => sh.IsActive);

                    if (activeShipping.ShipmentProviderType == (int)ShipmentProviderType.Amazon
                        && (activeShipping.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                            || activeShipping.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId))
                    {
                        //if (dtoOrder.OrderType == (int)OrderTypeEnum.Prime
                        //  || ShippingUtils.IsServiceTwoDays(dtoOrder.SourceShippingService))
                        {
                            _log.Info("Change provider for order: " + dtoOrder.OrderId + " (" + dtoOrder.SourceShippingService + ")");
                            //Update into DB, after success update
                            var order = db.Orders.GetById(dtoOrder.Id);
                            order.ShipmentProviderType = (int)ShipmentProviderType.FedexOneRate;
                            db.Commit();
                            dtoOrder.ShipmentProviderType = (int)ShipmentProviderType.FedexOneRate;

                            if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, switchToMethodId: null))
                            {
                                _log.Info("Success");
                            }
                            else
                            {
                                _log.Info("Failed");
                            }
                        }
                    }
                }
            }
        }

        public void CallValidationRules(string orderNumber)
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var addressProviders = _company.AddressProviderInfoList
                    .Where(a => a.Type != (int)AddressProviderType.SelfCorrection)
                    .ToList(); //NOTE: exclude self correction
            var serviceFactory = new ServiceFactory();
            var addressCheckService = serviceFactory.GetAddressCheckServices(_log,
                _time,
                _dbFactory,
                addressProviders);

            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                _time,
                _dbFactory,
                _weightService,
                _company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);
            var companyAddressList = new CompanyAddressService(_company);
            var addressService = new AddressService(addressCheckService, companyAddressList.GetReturnAddress(MarketIdentifier.Empty()), companyAddressList.GetPickupAddress(MarketIdentifier.Empty()));
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var priceService = new PriceService(_dbFactory);
            var settings = new SettingsService(_dbFactory);

            var validatorService = new OrderValidatorService(_log,
                    _dbFactory,
                    _emailService,
                    settings,
                    orderHistoryService,
                    _actionService,
                    priceService,
                    htmlScraper,
                    addressService,
                    companyAddressList.GetReturnAddress(MarketIdentifier.Empty()),
                    stampsRateProvider,
                    _time,
                    _company);

            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(_weightService, orderNumber, false, true);
                var dbOrder = db.Orders.GetById(order.Id);

                //validatorService.OrderValidationStepInitial(db,
                //    _time,
                //    _company,
                //    order,
                //    order.Items,
                //    dbOrder);

                validatorService.ShippingValidationStep(db,
                    order.Items,
                    order.SourceItems,
                    order.ShippingInfos,
                    order,
                    dbOrder);
            }
        }

        public void CallAddressValidation()
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var addressProviders = _company.AddressProviderInfoList
                    .Where(a => a.Type != (int)AddressProviderType.SelfCorrection)
                    .ToList(); //NOTE: exclude self correction
            var serviceFactory = new ServiceFactory();
            var addressCheckService = serviceFactory.GetAddressCheckServices(_log,
                _time,
                _dbFactory,
                addressProviders);
            var companyAddressList = new CompanyAddressService(_company);
            var addressService = new AddressService(addressCheckService, companyAddressList.GetReturnAddress(MarketIdentifier.Empty()), companyAddressList.GetPickupAddress(MarketIdentifier.Empty()));
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var priceService = new PriceService(_dbFactory);
            var settingsService = new SettingsService(_dbFactory);

            var validatorService = new OrderValidatorService(_log,
                    _dbFactory,
                    _emailService,
                    settingsService,
                    orderHistoryService,
                    _actionService,
                    priceService,
                    htmlScraper,
                    addressService,
                    null,
                    null,
                    _time,
                    _company);

            using (var db = _dbFactory.GetRWDb())
            {
                var searchResult = db.ItemOrderMappings.GetDisplayOrdersWithItems(_weightService, new OrderSearchFilter()
                {
                    OrderStatus = OrderStatusEnumEx.AllUnshipped
                });
                foreach (var order in searchResult.Items)
                {
                    var dbOrder = db.Orders.GetById(order.Id);

                    AddressDTO correctedAddress = null;
                    var sourceAddress = db.Orders.GetAddressInfo(dbOrder.AmazonIdentifier);// dbOrder.GetAddressDto();
                    var checkResults = validatorService.CheckAddress(
                        CallSource.Service,
                        db,
                        sourceAddress,
                        dbOrder.Id,
                        out correctedAddress);


                    var checkStatus = (int)Core.Models.AddressValidationStatus.None;
                    var stampsStatus = checkResults.FirstOrDefault(r => r.AdditionalData != null
                                                                        && r.AdditionalData.Any()
                                                                        && r.AdditionalData[0] == OrderNotifyType.AddressCheckStamps.ToString());

                    if (stampsStatus != null) //Get main checker status (by default: stamps.com)
                        checkStatus = stampsStatus.Status;

                    if (dbOrder.AddressValidationStatus != checkStatus) //NOTE: If changed then reset dismiss status
                    {
                        dbOrder.AddressValidationStatus = checkStatus;
                        //dbOrder.IsDismissAddressValidation = false;
                        //dbOrder.DismissAddressValidationBy = null;
                        //dbOrder.DismissAddressValidationDate = null;
                    }
                    db.Commit();
                }
            }
        }

        public void CallProcessDhlRates(string filename)
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/DhlInvoices/" + filename);
            var dhlInvoiceService = new DhlInvoiceService(_log, _time, _dbFactory);
            var records = dhlInvoiceService.GetRatesFromFile(filepath);
            dhlInvoiceService.ProcessRates(records);
        }

        public void CallProcessDhlInvoice(string filename)
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/DhlInvoices/" + filename);
            var dhlInvoiceService = new DhlInvoiceService(_log, _time, _dbFactory);
            var records = dhlInvoiceService.GetRecordsFromFile(filepath);
            dhlInvoiceService.ProcessRecords(records, ShipmentProviderType.Dhl);
        }

        public void CallProcessIBCInvoice(string filename)
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            var dhlInvoiceService = new DhlInvoiceService(_log, _time, _dbFactory);
            var records = dhlInvoiceService.GetIBCRecordsFromFile(filepath);
            dhlInvoiceService.ProcessIBCRecords(records, ShipmentProviderType.IBC);
        }


        public void ReValidateAddressForUnshippedOrders()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var settings = new SettingsService(_dbFactory);
                var quantityManager = new QuantityManager(_log, _time);
                var priceService = new PriceService(_dbFactory);
                var emailService = new EmailService(_log, 
                    SettingsBuilder.GetSmtpSettingsFromAppSettings(),
                     _addressService);

                var companyAddressList = new CompanyAddressService(_company);
                var stampsProvider = _company.AddressProviderInfoList.FirstOrDefault(p => p.Type == (int) AddressProviderType.Stamps);
                var addressService = new AddressService(new List<IAddressCheckService>()
                    {
                        new StampsAddressCheckService(_log, stampsProvider)
                    }, 
                    companyAddressList.GetReturnAddress(MarketIdentifier.Empty()), 
                    companyAddressList.GetPickupAddress(MarketIdentifier.Empty()));

                var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);                

                var validatorService = new OrderValidatorService(_log, _dbFactory, emailService, settings, orderHistoryService, _actionService, priceService, _htmlScraper, addressService, null, null, _time, _company);
                var actionService = new SystemActionService(_log, _time);
                var cacheService = new CacheService(_log, _time, actionService, quantityManager);
                AddressDTO correctedAddress = null;

                var orders = db.ItemOrderMappings.GetFilteredOrdersWithItems(_weightService,
                    new OrderSearchFilter()
                    {
                        BatchId = null,
                        OrderStatus = new[] { OrderStatusEnumEx.Unshipped.ToString() },
                        FulfillmentChannel = "MFN"
                    });

                foreach (var dtoOrder in orders)
                {
                    var sourceAddress = dtoOrder.GetAddressDto();
                    var checkResults = validatorService.CheckAddress(CallSource.Service,
                        db, 
                        sourceAddress, 
                        dtoOrder.Id, 
                        out correctedAddress);
                    
                    //var dbOrder = db.Orders.GetById(dtoOrder.Id);
                    //dbOrder.AddressValidationStatus = result.Status;
                    //dbOrder.DismissAddressValidationBy = null;
                    //dbOrder.DismissAddressValidationDate = null;
                    //db.Commit();
                }
            }
        }

        public void CallTestDbInclude()
        {
            using (var db = new UnitOfWork(_log))
            {
                var query = from o in db.Context.Orders
                    join sh in db.Context.OrderShippingInfos on o.Id equals sh.OrderId into withSh
                    select new DTOOrder()
                    {
                        OrderId = o.AmazonIdentifier,
                        ShippingInfos = withSh.Select(sh => new OrderShippingInfoDTO()
                        {
                            StampsShippingCost = sh.StampsShippingCost
                        }).ToList()
                    };
                var items = query.ToList();
                _log.Info("items=" + items.Count);


                var listings = db.Listings.GetPriceUpdateRequiredList(_amazonApiCom.Market, _amazonApiCA.MarketplaceId);
                _log.Info("count=" + listings.Count);
            }
        }

        public void CallUpdateEBayTrackingNumbers()
        {
            using (var db = new UnitOfWork(_log))
            {
                new eBayOrderUpdater(_eBayApi, _log, _time).UpdateOrders(db);
            }
        }

        public void CallGetEuropeOrders(AmazonApi api)
        {
            var itemsWithError = new List<ItemDTO>();
            api.FillWithAdditionalInfo(_log, 
                _time,
                new List<ItemDTO>()
                {
                    new ItemDTO() { ASIN = "B00M8LE5QE" }, //Parent
                    new ItemDTO() { ASIN = "B00LE3Y5OY" }, //Child
                    //new ItemDTO() { ASIN = "B00LE421QC" } //Child
                },
                IdType.ASIN,
                ItemFillMode.Defualt, 
                out itemsWithError); //Item: B00LE3Y5OY, Parent ASIN: B00M8LE5QE

            var orders = api.GetOrders(_log, DateTime.UtcNow.AddDays(-7), new List<string>() {OrderStatusEnumEx.Unshipped, OrderStatusEnumEx.PartiallyShipped});

            var itemListings = api.GetOrderItems("026-0929019-2237929");
            _log.Info(itemListings.Count().ToString());

            var items = new List<ItemDTO>()
            {
                new ItemDTO() {ASIN = "B00M8LE5QE"}
            };

            api.FillWithAdditionalInfo(_log, 
                _time, 
                items, 
                IdType.ASIN,
                ItemFillMode.Defualt,
                out itemsWithError);
            _log.Info(items.Count.ToString());

            //var result = api.RetrieveItemDetailsBySKU(_log, "FZ001GLVP"); //.com: Parent SKU: "S4GGT05BG");
            //_log.Info(result.Items.Count().ToString());
        }


        public void CallGetSpecificOrders(IMarketApi api, List<string> orderIds)
        {
            var orders = api.GetOrders(_log, orderIds);
            Console.WriteLine("Orders count=" + orders.Count());
            foreach (var order in orders)
            {
                IEnumerable<ListingOrderDTO> items;
                if (api.Market == MarketType.Amazon || api.Market == MarketType.AmazonEU || api.Market == MarketType.AmazonAU)
                    items = api.GetOrderItems(order.OrderId);
                else
                    items = api.GetOrderItems(order.MarketOrderId);
                Console.WriteLine("Items count=" + items.Count());
            }
        }


        private AmazonOrdersSynchronizer GetAmazonSyncService()
        {
            var syncInfo = new DbSyncInformer(_dbFactory, 
                _log, 
                _time,
                SyncType.Orders, 
                MarketplaceKeeper.DefaultMarketplaceId, 
                MarketType.Amazon,
                String.Empty);
            var settings = new SettingsService(_dbFactory);
            var dbFactory = new DbFactory();
            var quantityManager = new QuantityManager(_log, _time);
            var priceService = new PriceService(dbFactory);
            var companyAddressList = new CompanyAddressService(_company);
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
            
            return new AmazonOrdersSynchronizer(_log,
                _amazonApiCom,
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
        }

        public void CallProcessSpecifiedOrder(IMarketApi api, string orderNumber)
        {
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
                        
                        if (!String.IsNullOrEmpty(orderNumber))
                            synchronizer.ProcessSpecifiedOrder(db, orderNumber);
                        else
                            synchronizer.Sync(OrderSyncModes.Full, null);
                    }
                    finally
                    {
                        syncInfo.SyncEnd();
                    }
                }
            }
        }

        public void CallSendNoWeightEmail(string orderNumber)
        {
            var noWeightChecker = new NoWeightChecker(_log, _emailService, _time, _company);
            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(_weightService, orderNumber, unmaskReferenceStyle: false, includeSourceItems: false);
                noWeightChecker.Check(db, order, order.Items);
            }
        }
        
        public void CallProcessMagentoOrders()
        {
            _magentoApi.Connect();

            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Orders, MarketplaceKeeper.eBayAll4Kids, MarketType.eBay, null);
            var settings = new SettingsService(_dbFactory);
            var dbFactory = new DbFactory();
            var quantityManager = new QuantityManager(_log, _time);
            var priceService = new PriceService(dbFactory);
            var serviceFactory = new ServiceFactory();
            var addressCheckServices = serviceFactory.GetAddressCheckServices(_log,
                _time,
                dbFactory,
                _company.AddressProviderInfoList);
            var companyAddressList = new CompanyAddressService(_company);
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

                        var synchronizer = new MagentoOrdersSynchronizer(_log,
                            _magentoApi,
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

                        synchronizer.Sync(OrderSyncModes.Full, null);
                    }
                    finally
                    {
                        syncInfo.SyncEnd();
                    }
                }
            }
        }

        public void CheckDupl(string orderId)
        {
            var emailService = new EmailService(_log, SettingsBuilder.GetSmtpSettingsFromAppSettings(), _addressService);
            using (var db = new UnitOfWork(_log))
            {
                var dto = db.ItemOrderMappings.GetOrderWithItems(_weightService, orderId, unmaskReferenceStyle: false, includeSourceItems: false);

                var result = new DuplicateChecker(_log, emailService, _time).Check(db, dto, dto.Items);
                if (!result.IsSuccess)
                {

                }
            }
        }


        public void CallUpdateFulfillmentData(AmazonApi api, IList<string> orderStrings)
        {
            var updated = false;
            while (!updated)
            {
                long? feedId = null;
                var updater = new FulfillmentDataUpdater(_log, _time, _dbFactory);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = updater.GetUnprocessedFeedId(feedId.Value);

                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    updated = true;
                }
                else
                {
                    _log.Info("Submit new feed");
                    feedId = updater.SubmitFeed(api, _company.Id, orderStrings, AppSettings.FulfillmentRequestDirectory);
                }
                Thread.Sleep(60000);
            }
        }

        public void CallUpdateAcknowledgementData(AmazonApi api)
        {
            var actionService = new SystemActionService(_log, _time);

            //using (var db = new UnitOfWork())
            //{
            //    actionService.AddAction(db, SystemActionType.CancelOrder, new CancelOrderInput()
            //    {
            //        OrderNumber = "102-2035752-2069058"
            //    },
            //    null);
            //    db.Commit();
            //}

            var updated = false;
            while (!updated)
            {
                var updater = new AcknowledgementDataUpdater(actionService, _log, _time, _dbFactory);
                var unprocessedFeed = updater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    updated = true;
                }
                else
                {
                    _log.Info("Submit new feed");
                    updater.SubmitFeed(api, _company.Id, null, AppSettings.FulfillmentRequestDirectory);
                }
                Thread.Sleep(60000);
            }
        }

        public void CallUpdateAdjustmentData(AmazonApi api, string orderId)
        {
            var actionService = new SystemActionService(_log, _time);

            //using (var db = new UnitOfWork())
            //{
            //    actionService.AddAction(db, SystemActionType.CancelOrder, new CancelOrderInput()
            //    {
            //        OrderNumber = "102-2035752-2069058"
            //    },
            //    null);
            //    db.Commit();
            //}

            var updated = false;
            long? feedId = null;
            while (!updated)
            {
                var updater = new AdjustmentDataUpdater(actionService, _emailService, _log, _time, _dbFactory);
                var unprocessedFeed = feedId.HasValue ? updater.GetUnprocessedFeedId(feedId.Value) : null;
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    updated = true;
                }
                else
                {
                    _log.Info("Submit new feed");
                    feedId = updater.SubmitFeed(api, _company.Id, String.IsNullOrEmpty(orderId) ? null : new List<string>() { orderId }, AppSettings.FulfillmentRequestDirectory);
                }
                Thread.Sleep(60000);
            }
        }
    }
}
