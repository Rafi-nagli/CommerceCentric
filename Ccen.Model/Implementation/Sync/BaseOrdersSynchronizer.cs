using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.Validation;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Users;
using Stamps.Api;
using Amazon.Utils;
using Amazon.Model.Models;
using Amazon.Core.Models.Calls;
using Amazon.Api;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.SystemMessages;
using Amazon.DTO.Listings;

namespace Amazon.Model.Implementation.Sync
{
    public abstract class BaseOrdersSynchronizer : IOrderSynchronizer
    {
        protected ILogService _log;
        protected IMarketApi _api;
        protected CompanyDTO _company;
        protected ISettingsService _settings;
        protected ISyncInformer _syncInfo;
        protected IList<IShipmentApi> _rateProviders;
        protected IQuantityManager _quantityManager;
        protected IEmailService _emailService;
        protected IOrderValidatorService _validatorService;
        protected IOrderHistoryService _orderHistoryService;
        protected ICacheService _cacheService;
        protected ISystemActionService _systemAction;
        protected ICompanyAddressService _companyAddress;
        protected ITime _time;
        protected IWeightService _weightService;
        protected IShippingService _shippingService;
        protected ISystemMessageService _messageService;

        public BaseOrdersSynchronizer(ILogService log,
            IMarketApi api,
            CompanyDTO company,
            ISettingsService settings,
            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,
            IOrderHistoryService orderHistoryService,
            ICacheService cacheService,
            ISystemActionService systemAction,
            ICompanyAddressService companyAddress,
            ITime time,
            IWeightService weightService,
            ISystemMessageService messageService)
        {
            _log = log;
            _api = api;
            _company = company;
            _settings = settings;
            _syncInfo = syncInfo;
            _rateProviders = rateProviders;
            _quantityManager = quantityManager;
            _emailService = emailService;
            _validatorService = validatorService;
            _orderHistoryService = orderHistoryService;
            _cacheService = cacheService;
            _systemAction = systemAction;
            _companyAddress = companyAddress;
            _time = time;
            _weightService = weightService;
            _messageService = messageService;
            _shippingService = new ShippingService(new DbFactory(), false); //TODO: move to parameters
        }

        #region Interface to Impelement

        public abstract SyncResult Sync(OrderSyncModes syncMode, CancellationToken? cancel);

        public abstract SyncResult ProcessSpecifiedOrder(IUnitOfWork db, string orderId);

        #endregion

        public IShipmentApi GetRateProviderForOrder(IUnitOfWork db,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            IList<IShipmentApi> rateProviders)
        {
            order.WeightD = _weightService.ComposeOrderWeight(orderItems);

            //if (order.Market == (int)MarketType.Groupon)
            //    return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.FedexSmartPost);
            
            if (order.OrderType == (int)OrderTypeEnum.Prime) //For All Prime
                //NOTE: always Amazon, this is Amazon requirement
                return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Amazon);

            //NOTE: 
            /*1.	If shipping =standard
            State=fl
            Weight<1lb
            First class shipping not available from amazon
            Switch those orders automatically to stamps. Please do it for all pending orders before morning.
            */
            if (ShippingUtils.IsServiceStandard(order.InitialServiceType)
                && order.Market == (int)MarketType.Amazon
                && order.ShippingState == "FL"
                && ((_weightService != null
                    && _weightService.AdjustWeight(order.WeightD, orderItems.Sum(i => i.QuantityOrdered)) < 16)
                        || order.WeightD < 16.9))
            {
                return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);
            }

            if (ShippingUtils.IsServiceSameDay(order.InitialServiceType)) //All Same Day
                //NOTE: check, before Prime checking, so Prime + SameDay => Amazon, all other Prime => Stamps
                return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Amazon);

            if (ShippingUtils.IsServiceTwoDays(order.InitialServiceType)
                && order.Market == (int)MarketType.Amazon)
                return rateProviders.FirstOrDefault(p => p.Type == ShipmentProviderType.Amazon);

            if ((ShippingUtils.IsServiceTwoDays(order.InitialServiceType)
                || ShippingUtils.IsServiceNextDay(order.InitialServiceType))
                    && order.Market == (int)MarketType.Amazon)
            {
                try
                {
                    var amazonProvider = rateProviders.FirstOrDefault(p => p.Type == ShipmentProviderType.Amazon);
                    if (amazonProvider != null)
                    {
                        var allRates = GetAllRates(_log,
                            db,
                            amazonProvider,
                            _companyAddress,
                            _time,
                            order,
                            orderItems,
                            sourceOrderItems,
                            order.Id);

                        var hasSameDay = allRates != null &&
                                         allRates.Rates != null &&
                                         allRates.Rates.Any(
                                             r => r.ServiceTypeUniversal == (int)ShippingTypeCode.SameDay);

                        if (hasSameDay)
                            return amazonProvider;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("When check exists of SameDay rate", ex);
                }
            }

            if (!ShippingUtils.IsInternational(order.FinalShippingCountry))
            {
                if (AddressHelper.IsPOBox(order.FinalShippingAddress1, order.FinalShippingAddress2)
                    || AddressHelper.IsUSIsland(order.FinalShippingCountry, order.FinalShippingState)
                    || AddressHelper.IsAPO(order.FinalShippingCountry, order.FinalShippingState, order.FinalShippingCity))
                {
                    if (order.Market == (int)MarketType.Amazon)
                        return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Amazon);
                    else
                        return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);
                }
            }

            var fedexOneRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.FedexOneRate);
            if (fedexOneRateProvider != null)
            {
                if (!ShippingUtils.IsInternational(order.FinalShippingCountry)
                    && !ShippingUtils.IsInternationalState(order.FinalShippingState)
                    //NOTE: Exclude Amazon Rates
                    && !((ShippingUtils.IsServiceTwoDays(order.InitialServiceType)
                            || ShippingUtils.IsServiceNextDay(order.InitialServiceType)
                            || ShippingUtils.IsServiceSameDay(order.InitialServiceType))
                        && order.Market == (int)MarketType.Amazon))
                {
                    //var shippingPrice = orderItems.Sum(oi => oi.ShippingPrice);
                    //if (shippingPrice == 8.49M || shippingPrice == 12.49M || shippingPrice == 10.49M)
                    //    return fedexOneRateProvider;

                    var adjustedWeight = _weightService.AdjustWeight(orderItems.Sum(oi => oi.Weight ?? 0), orderItems.Sum(i => i.QuantityOrdered));

                    if (order.InitialServiceType != "Standard" || adjustedWeight > 16)
                    {
                        //NOTE: for amazon return amazon FEDEX One rates
                        if (order.Market == (int)MarketType.Amazon)
                            return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Amazon);
                        return fedexOneRateProvider;
                    }
                }
            }

            if ((!ShippingUtils.IsInternational(order.FinalShippingCountry)
                        && order.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId) //All Domestic and US marketplace (ex: order from Canada marketplace with US delivery country can't be buy through Amazon: 701-0967874-7649854)
                    || (ShippingUtils.IsMexico(order.FinalShippingCountry)
                        && order.Market == (int)MarketType.Amazon)) //DHLMX, for Mexico 
                return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Amazon);

            //var dhlProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Dhl);
            var fedexProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.FedexGeneral);
            if (fedexProvider != null)
            {
                if (ShippingUtils.IsInternational(order.ShippingCountry)
                    && (ShippingUtils.IsServiceTwoDays(order.InitialServiceType)
                        || ShippingUtils.IsServiceNextDay(order.InitialServiceType)
                        || ShippingUtils.IsServiceExpedited(order.InitialServiceType)))
                    return fedexProvider;
            }

            var skyPostalProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.SkyPostal);
            if (skyPostalProvider != null)
            {
                //а вес должен быть настоящим весом. Он кстати не может превышать 3 lb.
                //NOTE: 3lb condition not actual, removed.
                if (ShippingUtils.IsInternational(order.ShippingCountry)
                    && (ShippingUtils.IsServiceStandard(order.InitialServiceType))
                    && ShippingUtils.IsSkyPostalSupportedCountry(order.FinalShippingCountry))
                    return skyPostalProvider;
            }

            var ibcProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.IBC);
            if (ibcProvider != null)
            {
                //а вес должен быть настоящим весом. Он кстати не может превышать 3 lb.
                //NOTE: 3lb condition not actual, removed.
                if (ShippingUtils.IsInternational(order.ShippingCountry)
                    && (ShippingUtils.IsServiceStandard(order.InitialServiceType)))
                    return ibcProvider;
            }

            return rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);
        }

        public bool UIUpdate(IUnitOfWork db,
                    DTOOrder order,
                    bool isForceOverride,
                    bool keepActiveShipping,
                    bool keepCustomShipping,
                    int? switchToMethodId)
        {
            if (!ShippingUtils.IsOrderCanceled(order.OrderStatus)
                && !ShippingUtils.IsOrderPending(order.OrderStatus)) //Skip updating shippings for pending orders (they havn't address info)
            {
                var dtoShipping = order.ShippingInfos.ToList();
                if (isForceOverride //override include label infos
                    || dtoShipping.All(s => String.IsNullOrEmpty(s.LabelPath))) //Can force update only if haven't print labels
                {
                    //if (order.Market == (int) MarketType.Amazon 
                    //    || order.Market == (int) MarketType.AmazonEU)
                    //{
                    //    //NOTE: Originally without getting result of function (no erasing)
                    //    db.Listings.FillOrderItemsBySKU(order.Items, (MarketType)order.Market, order.MarketplaceId);
                    //}

                    var calculationResult = ShippingCalculationStatusEnum.NoCalculation;
                    int resultRateProviderType;

                    IShipmentApi rateProvider = null;
                    if (order.ShipmentProviderType != (int) ShipmentProviderType.None)
                    {
                        rateProvider = _rateProviders.FirstOrDefault(p => (int)p.Type == order.ShipmentProviderType);
                    }
                    else
                    {
                        rateProvider = GetRateProviderForOrder(db, order, order.Items, order.SourceItems, _rateProviders);
                    }

                    var isSuccess = StoreMappingWithAdditionalRates(_log, 
                        db, 
                        rateProvider,
                        _rateProviders,
                        _companyAddress,
                        _time, 
                        order, 
                        order.Items,
                        order.SourceItems,
                        order.Id,
                        keepActiveShipping,
                        keepCustomShipping,
                        switchToMethodId,
                        out calculationResult,
                        out resultRateProviderType);

                    var dbOrder = db.Orders.Get(order.Id);
                    dbOrder.ShipmentProviderType = resultRateProviderType;
                    dbOrder.ShippingCalculationStatus = (int)calculationResult;
                    db.Commit();

                    return isSuccess; //NOTE: otherwise system didn't get rates
                }
            }
            return false;
        }

        protected IList<long> GetAdditionalProviderIds(long? providerType, 
            MarketType market, 
            string shippingService,
            bool isPrime,
            bool isIntl,
            bool isPOBoxOrIsland,
            string countryCode)
        {
            if (!providerType.HasValue)
                return new List<long>();

            IList<long> providers = new List<long>();
            //if (providerType == (int)ShipmentProviderType.Amazon 
            //    && !isIntl
            //    && !ShippingUtils.IsServiceTwoDays(shippingService)
            //    && !ShippingUtils.IsServiceSameDay(shippingService)
            //    && !ShippingUtils.IsServiceNextDay(shippingService)
            //    && !isPrime)
            //    providers = new List<long> { (int)ShipmentProviderType.FedexOneRate };

            if (providerType == (int)ShipmentProviderType.Stamps && !isIntl && !isPOBoxOrIsland)
                providers = new List<long> { (int)ShipmentProviderType.FedexOneRate };

            if (providerType == (int)ShipmentProviderType.FedexOneRate)
            {
                if (!isIntl)
                {
                    if (market == MarketType.Amazon)
                        providers = new List<long> { (int)ShipmentProviderType.Amazon };
                    else
                        providers = new List<long> { (int)ShipmentProviderType.Stamps };
                }
            }

            if (market != MarketType.Amazon
                && market != MarketType.AmazonEU
                && market != MarketType.AmazonPrime
                && market != MarketType.Groupon)
            {
                if (!isPOBoxOrIsland)
                {
                    if (providerType != (int)ShipmentProviderType.FedexGeneral)
                        providers.Add((int)ShipmentProviderType.FedexGeneral);
                }
            }

            if (isIntl)
            {
                if (providerType == (int)ShipmentProviderType.FedexGeneral)
                {
                    providers.Add((int)ShipmentProviderType.IBC);
                    providers.Add((int)ShipmentProviderType.FIMS);
                }
                if (providerType == (int)ShipmentProviderType.IBC)
                {
                    providers.Add((int)ShipmentProviderType.FedexGeneral);
                    providers.Add((int)ShipmentProviderType.FIMS);
                }
                if (providerType == (int)ShipmentProviderType.FIMS)
                {
                    providers.Add((int)ShipmentProviderType.IBC);
                    providers.Add((int)ShipmentProviderType.FedexGeneral);
                }

                if (ShippingUtils.IsCanada(countryCode))
                {
                    providers.Add((int)ShipmentProviderType.SkyPostal);
                }
            }

            return providers.Distinct().ToList();
        }


        protected SyncResult ProcessSpecifiedOrderInner(IUnitOfWork db, 
            string orderId,
            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc)
        {
            try
            {
                var result = new SyncResult();
                var existDbOrder = db.Orders.GetByOrderNumber(orderId);
                if (existDbOrder == null)
                {
                    var orders = _api.GetOrders(_log, new List<string>() { orderId }).ToList();
                    result = ProcessNewOrdersPack(null,
                        _log,
                        _api,

                        getOrderItemsFromMarketFunc,
                        _rateProviders,
                        _validatorService,
                        _syncInfo,
                        _settings,
                        _quantityManager,
                        _emailService,
                        _cacheService,

                        _companyAddress,
                        orders,
                        _time);
                }
                else
                {
                    var orders = new List<Order>() { existDbOrder };

                    var success = ProcessExistOrdersPack(null,
                        _log,
                        _api,

                        getOrderItemsFromMarketFunc,
                        _syncInfo,
                        db,

                        _rateProviders,
                        //_settings,
                        _quantityManager,
                        _emailService,
                        //_cacheService,
                        _validatorService,

                        _companyAddress,
                        orders,
                        _time);

                    result = new SyncResult() { IsSuccess = success };
                }

                if (!result.IsSuccess)
                {
                    _syncInfo.AddWarning("", "The orders pack processing has failed");
                    return new SyncResult() { IsSuccess = false };
                }

                return new SyncResult()
                {
                    IsSuccess = true,
                    //ProcessedOrders = orders.Select(o => new SyncResultOrderInfo()
                    //                                    {
                    //                                        OrderId = o.OrderId,
                    //                                        OrderStatus = o.OrderStatus
                    //                                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _syncInfo.AddError(null, "Error when storing new orders", ex);
                _log.Error("Error when storing new orders", ex);
                return new SyncResult() { IsSuccess = false };
            }
        }

        #region Store Shippings with Mappings

        protected bool StoreMappingWithAdditionalRates(ILogService log,
            IUnitOfWork db,
            IShipmentApi defaultRateProvider,
            IList<IShipmentApi> allRateProviders,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> items,
            IList<ListingOrderDTO> sourceItems,
            long orderId,
            bool keepActiveShipping,
            bool keepCustomShipping,
            int? switchToMethodId,
            out ShippingCalculationStatusEnum calculateResult,
            out int resultRateProviderType)
        {
            var calculationResultForMainProvider = ShippingCalculationStatusEnum.NoCalculation;

            IShipmentApi mainRateProvider = null;
            if (order.ShipmentProviderType != (int)ShipmentProviderType.None)
            {
                mainRateProvider = _rateProviders.FirstOrDefault(p => (int)p.Type == order.ShipmentProviderType);
            }
            else
            {
                mainRateProvider = GetRateProviderForOrder(db, order, order.Items, order.SourceItems, _rateProviders);
            }

            var isPOBoxOrIsland = (AddressHelper.IsPOBox(order.FinalShippingAddress1, order.FinalShippingAddress2)
                    || AddressHelper.IsUSIsland(order.FinalShippingCountry, order.FinalShippingState)
                    || AddressHelper.IsAPO(order.FinalShippingCountry, order.FinalShippingState, order.FinalShippingCity));

            var additionalProviders = GetAdditionalProviderIds((int?)mainRateProvider?.Type, 
                (MarketType)order.Market, 
                order.InitialServiceType,
                order.OrderType == (int)OrderTypeEnum.Prime,
                ShippingUtils.IsInternational(order.FinalShippingCountry),
                isPOBoxOrIsland,
                order.FinalShippingCountry);

            additionalProviders = additionalProviders.Where(p => p != (int)mainRateProvider.Type).ToList();
            
            var isSuccessForMainProvider = StoreMappingWithRates(_log,
                db,
                mainRateProvider,
                _rateProviders,
                _companyAddress,
                _time,
                order,
                items,
                sourceItems,
                order.Id,
                keepActiveShipping,
                keepCustomShipping,
                switchToMethodId,
                false,
                out calculationResultForMainProvider);

            if (isSuccessForMainProvider) //NOTE: otherwise all old rates keeps as is
            {
                foreach (var additionalProvider in additionalProviders)
                {
                    var calculationResultForAdditionalProvider = ShippingCalculationStatusEnum.NoCalculation;

                    var additionalRateProvider = _rateProviders.FirstOrDefault(p => (int)p.Type == additionalProvider);

                    if (additionalRateProvider != null)
                    {
                        StoreMappingWithRates(_log,
                            db,
                            additionalRateProvider,
                            _rateProviders,
                            _companyAddress,
                            _time,
                            order,
                            items,
                            sourceItems,
                            order.Id,
                            keepActiveShipping,
                            keepCustomShipping,
                            switchToMethodId,
                            true,
                            out calculationResultForAdditionalProvider);
                    }
                }
            }

            UpdateDefaultShippingMethodPostProcessing(db, mainRateProvider, order);

            calculateResult = calculationResultForMainProvider;
            resultRateProviderType = order.ShippingInfos.FirstOrDefault(sh => sh.IsActive)?.ShipmentProviderType 
                ?? order.ShipmentProviderType;

            return isSuccessForMainProvider;
        }

        protected void UpdateDefaultShippingMethodPostProcessing(IUnitOfWork db,
            IShipmentApi mainRateProvider,
            DTOOrder order)
        {
            if (mainRateProvider.Type == ShipmentProviderType.IBC
                 && ShippingUtils.IsInternational(order.FinalShippingCountry))
            {
                var ibcMinRate = order.ShippingInfos.OrderBy(sh => sh.StampsShippingCost)
                    .FirstOrDefault(sh => sh.ShipmentProviderType == (int)ShipmentProviderType.IBC);
                var fedexMinRate = order.ShippingInfos.OrderBy(sh => sh.StampsShippingCost)
                    .FirstOrDefault(sh => sh.ShipmentProviderType == (int)ShipmentProviderType.FedexGeneral);
                if (ibcMinRate != null && fedexMinRate != null)
                {
                    if (ibcMinRate.StampsShippingCost > fedexMinRate.StampsShippingCost)
                    {
                        fedexMinRate.IsActive = true;
                        ibcMinRate.IsActive = false;

                        var dbFedexRate = db.OrderShippingInfos.Get(fedexMinRate.Id);
                        dbFedexRate.IsActive = true;
                        var dbIbcRate = db.OrderShippingInfos.Get(ibcMinRate.Id);
                        dbIbcRate.IsActive = false;

                        db.Commit();
                    }
                }
            }

            //TASK: If system choose Fedex One Rate pak ($7.5), but USPS Regional or Regular cheaper choose USPS (Regular most preferable)
            if (mainRateProvider.Type == ShipmentProviderType.FedexOneRate
                && order.ShippingInfos.Any(sh => sh.IsActive && sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak))
            {
                var fedexPak = order.ShippingInfos.FirstOrDefault(sh => sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak);

                var minUsps = order.ShippingInfos.OrderBy(sh => sh.StampsShippingCost)
                    .Where(sh => sh.ShippingMethodId == ShippingUtils.RegionalRateBoxAMethodId
                        || sh.ShippingMethodId == ShippingUtils.AmazonPriorityRegularShippingMethodId
                        || sh.ShippingMethodId == ShippingUtils.PriorityReqularShippingMethodId)
                    .FirstOrDefault();

                if (minUsps != null
                    && fedexPak.StampsShippingCost > minUsps.StampsShippingCost)
                {
                    minUsps.IsActive = true;
                    fedexPak.IsActive = false;

                    var dbUspsRate = db.OrderShippingInfos.Get(minUsps.Id);
                    dbUspsRate.IsActive = true;
                    var dbFedexRate = db.OrderShippingInfos.Get(fedexPak.Id);
                    dbFedexRate.IsActive = false;                    

                    db.Commit();
                }
            }
        }


        protected bool StoreMappingWithRates(ILogService log,
            IUnitOfWork db,
            IShipmentApi defaultRateProvider,
            IList<IShipmentApi> allRateProviders,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> items,
            IList<ListingOrderDTO> sourceItems,
            long orderId,
            bool keepActiveShipping,
            bool keepCustomShipping,
            int? switchToMethodId,
            bool isAdditionalRates,
            out ShippingCalculationStatusEnum calculateResult)
        {
            calculateResult = ShippingCalculationStatusEnum.NoCalculation;

            if (items == null)
                throw new ArgumentNullException("items");

            var oldShippings = db.OrderShippingInfos.GetByOrderId(order.Id).ToList();
            var lastShippingNumber = oldShippings.Any() ? oldShippings.Max(sh => sh.ShippingNumber) ?? 0 : 0;

            ItemPackageDTO previousPackageSize = null;

            //NOTE: exclude empty shipping
            var activeShipping = oldShippings.FirstOrDefault(sh => sh.IsActive && sh.StampsShippingCost.HasValue);
            if (activeShipping != null)
            {
                previousPackageSize = new ItemPackageDTO()
                {
                    PackageLength = activeShipping.PackageLength,
                    PackageWidth = activeShipping.PackageWidth,
                    PackageHeight = activeShipping.PackageHeight
                };
            }

            var toKeepNumberInBatch = activeShipping?.NumberInBatch ?? 
                oldShippings.FirstOrDefault()?.NumberInBatch; //NOTE: in case when no active shippings
            var toKeepShippingMethodId = activeShipping != null ? activeShipping.ShippingMethodId : null;
            if (switchToMethodId.HasValue && switchToMethodId > 0)
            {
                keepActiveShipping = true;
                toKeepShippingMethodId = switchToMethodId.Value;
            }

            var toKeepGroupId =  activeShipping != null ? (int?)activeShipping.ShippingGroupId : null;
            if (toKeepGroupId != RateHelper.FlatPartialInternationalGroupId
                && toKeepGroupId != RateHelper.FlatPartialGroupId
                && toKeepGroupId != RateHelper.ExpressFlatPartialGroupId
                && toKeepGroupId != RateHelper.StandardPartialGroupId
                && toKeepGroupId != RateHelper.RegularPartialInternationalGroupId)
                toKeepGroupId = null;


            order.WeightD = _weightService.ComposeOrderWeight(items);
            try
            {
                if (!isAdditionalRates)
                {
                    order.ShippingInfos = new List<OrderShippingInfoDTO>();
                } //Otherwise preserve rates

                
                #region GetRates

                GetRateResult rateResult = null;

                if ((ShippingUtils.IsOrderUnshipped(order.OrderStatus)
                    || ShippingUtils.IsOrderPartial(order.OrderStatus))
                    && items.Any() 
                    && items.All(i => i.Weight.HasValue))
                {
                    if (ShippingUtils.IsInternational(order.FinalShippingCountry))
                        rateResult = GetMappingInternational(log, db, defaultRateProvider, companyAddress, time, order, items, sourceItems, previousPackageSize, orderId);
                    else
                        rateResult = GetMappingLocal(log, db, defaultRateProvider, companyAddress, time, order, items, sourceItems, previousPackageSize, orderId);

                    if (rateResult.Result == GetRateResultType.Success)
                    {
                        if (rateResult.Rates != null
                            && rateResult.Rates.Count == 0)
                        {
                            calculateResult = ShippingCalculationStatusEnum.FullWithNoRateCalculation;
                        }
                        else
                        {
                            calculateResult = ShippingCalculationStatusEnum.FullCalculation;
                        }
                    }
                }
                
                //Fixup rateResult with noRates
                if (rateResult == null || 
                    rateResult.Rates == null ||
                    rateResult.Rates.Count == 0)
                    //(!oldShippings.Any() && (rateResult.Rates == null || !rateResult.Rates.Any()))) //If no old and new rates going to create empty rates
                {
                    var hasError = rateResult != null && rateResult.Result == GetRateResultType.Error;

                    if (String.IsNullOrEmpty(order.FinalShippingCountry)
                        || hasError) //NOTE: when normal rate calculation has failed, setting $0 rate, skipping w/o weight calculation
                    {
                        rateResult = GetMappingEmptyWoWeight(log, db, defaultRateProvider, companyAddress, time, order, items, sourceItems, orderId);
                    }
                    else
                    {
                        if (ShippingUtils.IsInternational(order.FinalShippingCountry))
                        {
                            rateResult = GetMappingEmptyWoWeight(log, db, defaultRateProvider, companyAddress, time, order, items, sourceItems, orderId);
                        }
                        else
                        {
                            rateResult = GetMappingLocalWoWeight(log, db, defaultRateProvider, companyAddress, time, order, items, sourceItems, orderId);

                            if (rateResult.Result == GetRateResultType.Success
                                && rateResult.Rates != null
                                && rateResult.Rates.Count > 1)
                                //NOTE: if success get priority rates for no weight, otherwise none
                            {
                                calculateResult = ShippingCalculationStatusEnum.WoWeightCalculation;
                            }
                        }
                    }
                }

                if (defaultRateProvider.Type != ShipmentProviderType.Stamps
                    && rateResult.Rates != null
                    && rateResult.Rates.Any(r => r.ServiceTypeUniversal == (int)ShippingTypeCode.Priority
                        && r.PackageTypeUniversal == (int)PackageTypeCode.Flat))
                {
                    var stampsRateProvider = allRateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);
                    if (stampsRateProvider != null)
                    {
                        var stampsFlatRates = GetMappingLocalWoWeight(log, db, stampsRateProvider, companyAddress, time, order, items, sourceItems, orderId);

                        //NOTE: multiple in case +2xFlat packages
                        var flatRatesToUpdate = rateResult.Rates.Where(r => r.ServiceTypeUniversal == (int) ShippingTypeCode.Priority
                                                        && r.PackageTypeUniversal == (int) PackageTypeCode.Flat)
                                .ToList();
                        var stampsPriorityFlat = stampsFlatRates.Rates != null ? stampsFlatRates.Rates.FirstOrDefault(
                                r => r.ServiceTypeUniversal == (int) ShippingTypeCode.Priority
                                     && r.PackageTypeUniversal == (int) PackageTypeCode.Flat) : null;

                        if (stampsPriorityFlat != null)
                        {
                            foreach (var rate in flatRatesToUpdate)
                            {
                                _log.Info(String.Format("Flat rate updated delivery info, DeliveryDate: {0} => {1}, DaysInfo: {2} => {3}",
                                    rate.DeliveryDate,
                                    stampsPriorityFlat.DeliveryDate,
                                    rate.DeliveryDaysInfo,
                                    stampsPriorityFlat.DeliveryDaysInfo));
                                rate.DeliveryDaysInfo = stampsPriorityFlat.DeliveryDaysInfo;
                                rate.DeliveryDate = stampsPriorityFlat.DeliveryDate;
                            }
                        }
                    }
                }

                #endregion


                #region Update shipping info based on new rates
                
                ShippingMethodDTO defaultShippingMethod = null;
                var shippingMethodList = db.ShippingMethods.GetAllAsDto().Where(m => m.IsActive).ToList();

                if (keepActiveShipping && !isAdditionalRates)
                {
                    if (toKeepShippingMethodId.HasValue)
                    {
                        var withShippingMethodAnalogs = ShippingUtils.GetShippingMethodAnalogs(toKeepShippingMethodId.Value);
                        defaultShippingMethod = shippingMethodList.FirstOrDefault(m => withShippingMethodAnalogs.Contains(m.Id)
                            && ShippingUtils.ConvertToMainProviderType(m.ShipmentProviderType) == (int)defaultRateProvider.Type);
                    }
                }

                var shippingNumber = lastShippingNumber;

                if (rateResult != null
                    && rateResult.Rates != null
                    && rateResult.Result == GetRateResultType.Success)
                {
                    var rates = rateResult.Rates;

                    #region Set Visible/Default/NumberInBatch
                    if (!isAdditionalRates)
                    {
                        //If keep custom shipping enabled and we have active custom shipping, skip default updating
                        if (keepCustomShipping &&
                            oldShippings.Any(sh => sh.ShippingGroupId == RateHelper.CustomPartialGroupId && sh.IsActive))
                        {
                            //Keep exist Default values
                            rates.ForEach(r => r.IsDefault = false);
                        }
                        else
                        {
                            //If already has default value and we have rate for it, updates default
                            if (defaultShippingMethod != null
                                && rates.Any(r => r.ServiceIdentifier == defaultShippingMethod.ServiceIdentifier))
                            {
                                foreach (var rate in rates)
                                {
                                    if (toKeepGroupId.HasValue)
                                        rate.IsDefault = rate.GroupId == toKeepGroupId.Value;
                                    else
                                        rate.IsDefault = rate.GroupId == 0  //NOTE: W/o groups
                                                         && rate.ServiceIdentifier == defaultShippingMethod.ServiceIdentifier;

                                    if (rate.IsDefault)
                                        rate.IsVisible = true;
                                }
                                _log.Debug("Restore default, method=" + defaultShippingMethod.Name
                                           + ", serviceIdentifier=" + defaultShippingMethod.ServiceIdentifier
                                           + "or groupId=" + toKeepGroupId);
                            }
                        }
                    }
                    else
                    {
                        rates.ForEach(r => r.IsDefault = false);
                        rates.ForEach(r => r.IsVisible = false);
                    }

                    //NOTE: Force mark all rates as visible
                    rates.ForEach(r => r.IsVisible = true);

                    //Prepare rate groups
                    foreach (var rate in rates.OrderBy(r => r.GroupId))
                    {
                        if (rate.GroupId == 0)
                        {
                            var shipingMethod = shippingMethodList.FirstOrDefault(m => m.ServiceIdentifier == rate.ServiceIdentifier);
                            if (shipingMethod != null)
                                rate.GroupId = shipingMethod.Id;
                        }
                    }

                    if (rates.Any(r => r.IsDefault))
                        rates.Where(r => r.IsDefault).ForEach(r => r.NumberInBatch = toKeepNumberInBatch);
                    else
                        rates.ForEach(r => r.NumberInBatch = toKeepNumberInBatch);
                    #endregion

                    #region
                    ////Restore manually package size
                    //foreach (var rate in rates)
                    //{
                    //    if (!rate.PackageLength.HasValue
                    //        && !rate.PackageWidth.HasValue
                    //        && !rate.PackageHeight.HasValue)
                    //    {
                    //        rate.PackageLength = previousPackageSize.PackageLength;
                    //        rate.PackageWidth = previousPackageSize.PackageWidth;
                    //        rate.PackageHeight = previousPackageSize.PackageHeight;
                    //    }
                    //}

                    #endregion


                    foreach (var rate in rates)
                    {
                        _log.Debug("store rate, service" + rate.ServiceTypeUniversal
                                   + ", package=" + rate.PackageTypeUniversal
                                   + ", cost=" + rate.Amount
                                   + ", defualt=" + rate.IsDefault
                                   + ", visible=" + rate.IsVisible
                                   + ", groupId=" + rate.GroupId
                                   + ", packageSize (LxWxH)=" + rate.PackageLength + ", " + rate.PackageWidth + ", " + rate.PackageHeight
                                   + ", shipDate=" + rate.ShipDate
                                   + ", deliveryDate=" + rate.DeliveryDate
                                   + ", daysInfo=" + rate.DeliveryDaysInfo
                                   + ", numberInBatch=" + rate.NumberInBatch);
                        var currentRate = rate;

                        shippingNumber++;
                        var method =
                            shippingMethodList.FirstOrDefault(m => m.ServiceIdentifier == currentRate.ServiceIdentifier);
                        if (method != null)
                        {
                            currentRate.DeliveryDays = _time.GetBizDaysCount(currentRate.ShipDate,
                                currentRate.DeliveryDate);
                            currentRate.UpChargeCost = _shippingService.GetUpCharge(method.Id, currentRate.Amount);

                            var shippingInfo = db.OrderShippingInfos.CreateShippingInfo(currentRate,
                                orderId,
                                shippingNumber,
                                method.Id);

                            if (currentRate.ItemOrderIds != null && currentRate.ItemOrderIds.Any())
                            {
                                log.Debug("store partial, items="
                                          + String.Join(", ", currentRate.ItemOrderIds.Select(i => (i.OrderItemId.ToString() + "-" + i.Quantity.ToString())).ToList()));
                                db.ItemOrderMappings.StorePartialShippingItemMappings(shippingInfo.Id,
                                    currentRate.ItemOrderIds,
                                    items,
                                    time.GetAppNowTime());
                            }
                            else
                            {
                                db.ItemOrderMappings.StoreShippingItemMappings(shippingInfo.Id,
                                    items,
                                    time.GetAppNowTime());
                            }

                            //Update DTO object
                            order.ShippingInfos.Add(new OrderShippingInfoDTO
                            {
                                ShippingNumber = shippingNumber,
                                Id = shippingInfo.Id,

                                ShippingMethod = method,
                                StampsShippingCost = shippingInfo.StampsShippingCost,
                                InsuranceCost = shippingInfo.InsuranceCost,
                                SignConfirmationCost = shippingInfo.SignConfirmationCost,
                                UpChargeCost = shippingInfo.UpChargeCost,

                                ShipmentProviderType = shippingInfo.ShipmentProviderType,

                                IsActive = shippingInfo.IsActive,
                                IsVisible = shippingInfo.IsVisible,

                                NumberInBatch = shippingInfo.NumberInBatch,                                
                            });
                        }
                    }

                    //Remove old shippings
                    //NOTE: always if exist any new rates, otherwise order may be hide
                    if (rates.Any()
                        && !isAdditionalRates) //NOTE: skip when additional rates, when we recalc main rates we already removed all rates
                    {
                        //Remove old mappings
                        foreach (var oldShipping in oldShippings)
                        {
                            if (!keepCustomShipping || oldShipping.ShippingGroupId != RateHelper.CustomPartialGroupId)
                            {
                                db.OrderShippingInfos.Remove(oldShipping);
                                var toRemoveShDto = order.ShippingInfos.FirstOrDefault(sh => sh.Id == oldShipping.Id);
                                if (toRemoveShDto != null)
                                    order.ShippingInfos.Remove(toRemoveShDto);
                            }
                        }
                        db.Commit();

                        //NOTE: Usefull for UIUpdate
                        //order.ShippingService = order.InitialServiceType;

                        _syncInfo.AddSuccess(order.OrderId, "Order shipppings was successfully updated from UI");
                    }
                    else
                    {
                        _syncInfo.AddInfo(order.OrderId, "Shipping was not calculated");
                    }
                }
                #endregion

                if (rateResult != null)
                {
                    db.OrderNotifies.StoreGetRateMessage(orderId,
                        rateResult.Result,
                        rateResult.Message,
                        time.GetAppNowTime());
                }

                return rateResult != null
                    && rateResult.Result == GetRateResultType.Success;// rateResult != null ? rateResult.Rates : new List<RateDTO>(); //new shipping rates
            }
            catch (SqlException ex)
            {
                log.Error("StoreMappingWithRates.SqlException", ex);
                //In this case dbContext stay invalid
                throw;
            }
            catch (DbUpdateException ex)
            {
                log.Error("StoreMappingWithRates.DbUpdateException", ex);
                //In this case dbContext stay invalid
                throw;
            }
            catch (Exception ex)
            {
                log.Error("StoreMappingWithRates.Exception", ex);
                return false;
            }
        }


        /// <summary>
        /// Get mapping for Amazon Pending orders (w/o CountryName)
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        protected GetRateResult GetMappingEmptyWoWeight(
            ILogService log,
            IUnitOfWork db,
            IShipmentApi rateProvider,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            long orderId)
        {
            var emptyRate = rateProvider.GetEmptyRate(order.InitialServiceType, order.FinalShippingCountry);
            
            return new GetRateResult()
            {
                Result = GetRateResultType.Success,
                Rates = new List<RateDTO>() { emptyRate },
                Message = null,
            };
        }

        protected GetRateResult GetMappingLocalWoWeight(
            ILogService log,
            IUnitOfWork db,
            IShipmentApi rateProvider,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            long orderId)
        {
            var defaultEmptyRate = rateProvider.GetEmptyRate(order.InitialServiceType, order.FinalShippingCountry);
            var resultRates = new List<RateDTO>() { };
            
            //Only if no Pending and exist items (m.b. not found linked listings)
            if (orderItems.Any()
                && ShippingUtils.IsOrderUnshipped(order.OrderStatus))
            {
                var toAddress = db.Orders.GetAddressInfo(order.Id);
                var shipDate = db.Dates.GetOrderShippingDate(null);

                var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(orderItems);
                var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(sourceOrderItems);

                var flatLocalRateResult = rateProvider.GetAllFlatLocalRate(
                    companyAddress.GetReturnAddress(order.GetMarketId()),
                    companyAddress.GetPickupAddress(order.GetMarketId()),
                    toAddress,
                    shipDate,
                    rateProvider.DefaultWeightForNoWeigth,
                    null,
                    order.IsInsured ? order.TotalPrice : 0,
                    order.IsSignConfirmation,
                    new OrderRateInfo()
                    {
                        OrderNumber = order.OrderId,

                        Items = orderItemInfoes,
                        SourceItems = sourceOrderItemInfoes,

                        EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(order.LatestShipDate, (MarketType)order.Market),
                        ShippingService = order.InitialServiceType,
                        TotalPrice = order.TotalPrice,
                        Currency = order.TotalPriceCurrency,
                    },
                    RetryModeType.Random);

                foreach (var rate in flatLocalRateResult.Rates)
                {
                    rate.IsDefault = false;
                    rate.IsVisible = false;
                }

                if (flatLocalRateResult.Result == GetRateResultType.Success)
                    resultRates.AddRange(flatLocalRateResult.Rates);
            }

            //If not rates results insert empty
            if (resultRates.All(r => r.ServiceTypeUniversal != defaultEmptyRate.ServiceTypeUniversal
                                     && r.PackageTypeUniversal != defaultEmptyRate.PackageTypeUniversal))
            {
                resultRates.Insert(0, defaultEmptyRate);
            }
            else
            {
                resultRates.ForEach(r =>
                {
                    r.IsVisible = true;
                    r.IsDefault = false;
                });
            }
            
            return new GetRateResult()
            {
                Result = GetRateResultType.Success,
                Rates = resultRates,
                Message = null,
            };
        }

        protected GetRateResult GetMappingInternational(
            ILogService log,
            IUnitOfWork db,
            IShipmentApi rateProvider,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            ItemPackageDTO overridePackageSize,
            long orderId)
        {
            var addressTo = db.Orders.GetAddressInfo(order.Id);
            var shipDate = db.Dates.GetOrderShippingDate(null);
            var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(orderItems);
            var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(sourceOrderItems);

            var internationalPackage = ShippingServiceUtils.GetDefaultInternationalPackage(orderItems);

            log.Info("GetInternationalRates, orderId=" + orderId 
                + ", internationalPackage=" + internationalPackage);

            var rateResult = rateProvider.GetInternationalRates(companyAddress.GetReturnAddress(order.GetMarketId()),
                companyAddress.GetPickupAddress(order.GetMarketId()),
                addressTo,
                shipDate,
                order.WeightD,
                overridePackageSize,
                order.IsInsured ? order.TotalPrice : 0,
                order.IsSignConfirmation,
                new OrderRateInfo()
                {
                    OrderNumber = order.OrderId,

                    Items = orderItemInfoes,
                    SourceItems = sourceOrderItemInfoes,

                    EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(order.LatestShipDate, (MarketType)order.Market),
                    ShippingService = order.InitialServiceType,
                    InternationalPackage = internationalPackage,
                    TotalPrice = order.TotalPrice,
                    Currency = order.TotalPriceCurrency,
                },
                RetryModeType.Random);

            return rateResult;
        }

        protected GetRateResult GetMappingLocal(ILogService log,
            IUnitOfWork db,
            IShipmentApi rateProvider,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            ItemPackageDTO overridePackageSize,
            long orderId)
        {
            var correctedInitialShippingType = ShippingUtils.CorrectInitialShippingService(order.InitialServiceType, order.SourceShippingService, (OrderTypeEnum)order.OrderType);
            var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(correctedInitialShippingType, order.UpgradeLevel); //order.ShippingService
            
            //TODO: Group combined items
            var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(orderItems);
            var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(sourceOrderItems ?? orderItems);

            var addressTo = db.Orders.GetAddressInfo(order.Id);
            var shipDate = db.Dates.GetOrderShippingDate(null);

            GetRateResult rateResult = null;

            log.Info("GetSpecificLocalRate, orderId=" + orderId);
            rateResult = rateProvider.GetLocalRate(companyAddress.GetReturnAddress(order.GetMarketId()),
                companyAddress.GetPickupAddress(order.GetMarketId()),
                addressTo,
                shipDate,
                order.WeightD,
                overridePackageSize,
                order.IsInsured ? order.TotalPrice : 0,
                order.IsSignConfirmation,
                new OrderRateInfo()
                {
                    OrderNumber = order.OrderId,
                    OrderType = (OrderTypeEnum)order.OrderType,

                    Items = orderItemInfoes,
                    SourceItems = sourceOrderItemInfoes,

                    EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(order.LatestShipDate, (MarketType)order.Market),
                    ShippingService = shippingService,
                    InitialServiceType = order.InitialServiceType,
                    TotalPrice = order.TotalPrice,
                    Currency = order.TotalPriceCurrency,
                },
                RetryModeType.Random);

            RateListValidation(log, rateResult.Rates);

            return rateResult;
        }


        protected GetRateResult GetAllRates(ILogService log,
            IUnitOfWork db,
            IShipmentApi rateProvider,
            ICompanyAddressService companyAddress,
            ITime time,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            long orderId)
        {
            var correctedInitialShippingType = ShippingUtils.CorrectInitialShippingService(order.InitialServiceType, order.SourceShippingService, (OrderTypeEnum)order.OrderType);
            var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(correctedInitialShippingType, order.UpgradeLevel); //order.ShippingService

            var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(orderItems);
            var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(sourceOrderItems);

            var addressTo = db.Orders.GetAddressInfo(order.Id);
            var shipDate = db.Dates.GetOrderShippingDate(null);

            GetRateResult rateResult = null;

            log.Info("GetSpecificLocalRate, orderId=" + orderId);
            rateResult = rateProvider.GetAllRate(companyAddress.GetReturnAddress(order.GetMarketId()),
                companyAddress.GetPickupAddress(order.GetMarketId()),
                addressTo,
                shipDate,
                order.WeightD,
                null,
                order.IsInsured ? order.TotalPrice : 0,
                order.IsSignConfirmation,
                new OrderRateInfo()
                {
                    OrderNumber = order.OrderId,

                    Items = orderItemInfoes,
                    SourceItems = sourceOrderItemInfoes,

                    EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(order.LatestShipDate, (MarketType)order.Market),
                    ShippingService = shippingService,
                    TotalPrice = order.TotalPrice,
                    Currency = order.TotalPriceCurrency,
                },
                RetryModeType.Random);

            return rateResult;
        }


        protected bool UpdateShippings(ILogService log,
            IUnitOfWork db,
            IShipmentApi rateProvider,
            IList<IShipmentApi> allRateProviders,
            ISyncInformer syncInfo, 
            ICompanyAddressService companyAddress,
            ITime time,
            Order dbOrder,
            DTOOrder marketOrder,
            IList<ListingOrderDTO> marketOrderItems,
            IList<ListingOrderDTO> marketSourceOrderItems,
            bool isForceRecalc)
        {
            var isSuccess = false;
            var beforeRecalcShippings = db.OrderShippingInfos.GetByOrderId(dbOrder.Id).ToList();
            if (isForceRecalc 
                || ((beforeRecalcShippings.Any(s => s.StampsShippingCost == null) 
                    && beforeRecalcShippings.All(s => String.IsNullOrEmpty(s.LabelPath)))
                || beforeRecalcShippings.Count == 0))
            {
                var calculationResult = ShippingCalculationStatusEnum.NoCalculation;
                int resultRateProviderType;

                isSuccess = StoreMappingWithAdditionalRates(log,
                    db,
                    rateProvider,
                    allRateProviders,
                    companyAddress,
                    time,
                    marketOrder,
                    marketOrderItems,
                    marketSourceOrderItems,
                    dbOrder.Id,
                    false,
                    false,
                    null,
                    out calculationResult,
                    out resultRateProviderType);

                dbOrder.ShipmentProviderType = resultRateProviderType;
                dbOrder.ShippingCalculationStatus = (int)calculationResult;
                db.Commit();

                if (isSuccess)
                {
                    syncInfo.AddSuccess(marketOrder.OrderId, "Shippings was successfully generated, status=" + calculationResult);
                    _log.Debug("Shipping was updated, new count="
                               + (marketOrder.ShippingInfos != null ? marketOrder.ShippingInfos.Count.ToString() : "[null]")
                               + ", prev count=" + beforeRecalcShippings.Count);
                }
                else
                {
                    syncInfo.AddInfo(marketOrder.OrderId, "Shippings was not calculated, status=" + calculationResult);
                    _log.Debug("Shipping was not calculated");
                }
            }
            else
            {
                syncInfo.AddInfo(marketOrder.OrderId, "Shipping calculation was skipped");
            }

            return isSuccess;
        }

        protected void UpdateOrderItems(ILogService log,
            IUnitOfWork db,
            long orderId,
            IList<ListingOrderDTO> sourceItems,
            DateTime? when)
        {
            foreach (var sourceItem in sourceItems)
            {
                var sourceOrderItems =
                    db.OrderItemSources.GetFiltered(m => m.ItemOrderIdentifier == sourceItem.ItemOrderId
                                                         && m.OrderId == orderId).ToList();

                if (sourceOrderItems.Count > 0)
                {
                    foreach (var sourceOrderItem in sourceOrderItems)
                    {
                        sourceOrderItem.ShippingPrice = sourceItem.ShippingPrice;
                        sourceOrderItem.ShippingTax = sourceItem.ShippingTax;
                        sourceOrderItem.ShippingPriceCurrency = sourceItem.ShippingPriceCurrency;
                        sourceOrderItem.ShippingPriceInUSD = sourceItem.ShippingPriceInUSD;

                        sourceOrderItem.ItemPrice = sourceItem.ItemPrice;
                        sourceOrderItem.ItemTax = sourceItem.ItemTax;
                        sourceOrderItem.ItemPriceCurrency = sourceItem.ItemPriceCurrency;
                        sourceOrderItem.ItemPriceInUSD = sourceItem.ItemPriceInUSD;

                        sourceOrderItem.QuantityOrdered = sourceItem.QuantityOrdered;
                        sourceOrderItem.QuantityShipped = sourceItem.QuantityShipped;

                        sourceOrderItem.ShippingDiscount = sourceItem.ShippingDiscount;
                        sourceOrderItem.ShippingDiscountCurrency = sourceItem.ShippingDiscountCurrency;
                        sourceOrderItem.ShippingDiscountInUSD = sourceItem.ShippingDiscountInUSD;

                        sourceOrderItem.PromotionDiscount = sourceItem.PromotionDiscount;
                        sourceOrderItem.PromotionDiscountCurrency = sourceItem.PromotionDiscountCurrency;
                        sourceOrderItem.PromotionDiscountInUSD = sourceItem.PromotionDiscountInUSD;

                        if (sourceOrderItem.ItemStatus != sourceItem.ItemStatus)
                            sourceOrderItem.ItemStatusDate = when;
                        sourceOrderItem.ItemStatus = sourceItem.ItemStatus;


                        sourceOrderItem.UpdateDate = when;
                    }
                }
                else
                {
                    log.Fatal("Missing source order mapping, orderId=" + orderId + " for ItemOrderId=" +
                              sourceItem.ItemOrderId);
                }
            }

            foreach (var item in sourceItems)
            {
                var orderItems = db.OrderItems.GetFiltered(m => m.SourceItemOrderIdentifier == item.ItemOrderId
                    && m.OrderId == orderId).ToList();

                var balanceFactor = orderItems.Count == 0 ? 1M : 1/(decimal)orderItems.Count;

                if (orderItems.Count > 0)
                {
                    //NOTE: Update only mappings with original ItemOrderId
                    foreach (var orderItem in orderItems)
                    {
                        orderItem.ShippingPrice = item.ShippingPrice * balanceFactor;
                        orderItem.ShippingTax = item.ShippingTax * balanceFactor;
                        orderItem.ShippingPriceCurrency = item.ShippingPriceCurrency;
                        orderItem.ShippingPriceInUSD = item.ShippingPriceInUSD * balanceFactor;

                        orderItem.ItemPrice = item.ItemPrice * balanceFactor;
                        orderItem.ItemTax = item.ItemTax * balanceFactor;
                        orderItem.ItemPriceCurrency = item.ItemPriceCurrency;
                        orderItem.ItemPriceInUSD = item.ItemPriceInUSD * balanceFactor;

                        orderItem.QuantityOrdered = item.QuantityOrdered;
                        orderItem.QuantityShipped = item.QuantityShipped;

                        orderItem.ShippingDiscount = item.ShippingDiscount * balanceFactor;
                        orderItem.ShippingDiscountCurrency = item.ShippingDiscountCurrency;
                        orderItem.ShippingDiscountInUSD = item.ShippingDiscountInUSD * balanceFactor;

                        orderItem.PromotionDiscount = item.PromotionDiscount * balanceFactor;
                        orderItem.PromotionDiscountCurrency = item.PromotionDiscountCurrency;
                        orderItem.PromotionDiscountInUSD = item.PromotionDiscountInUSD * balanceFactor;

                        if (orderItem.ItemStatus != item.ItemStatus)
                            orderItem.ItemStatusDate = when;
                        orderItem.ItemStatus = item.ItemStatus;
                        
                        orderItem.UpdateDate = when;
                    }
                }
                else
                {
                    log.Fatal("Missing order mapping, orderId=" + orderId + " for ItemOrderId=" + item.ItemOrderId);
                }
            }

            db.Commit();
        }
        #endregion


        #region Process Orders (Exist and New)
        protected SyncResult ProcessNewOrdersPack(CancellationToken? cancel, 
            ILogService log,
            IMarketApi api,
            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc,
            IList<IShipmentApi> rateProviders,
            IOrderValidatorService validatorService,
            ISyncInformer syncInfo,
            ISettingsService settings,
            IQuantityManager quantityManager,
            IEmailService emailService,
            ICacheService cacheService,
            ICompanyAddressService companyAddress,
            List<DTOOrder> orders,
            ITime time)
        {
            try
            {
                IList<DTOOrder> newOrders = new List<DTOOrder>();
                IList<DTOOrder> existOrders = new List<DTOOrder>();
                var market = api.Market;

                using (var db = new UnitOfWork(_log))
                {
                    log.Info("Start search new orders");
                    newOrders = db.Orders.ExcludeExistOrderDtos(orders, api.Market, api.MarketplaceId);
                    existOrders = orders.Where(o => newOrders.All(n => n.OrderId != o.OrderId)).ToList();
                    log.Info("End search new orders");
                    log.Info("New orders count:" + newOrders.Count() + ", exist orders:" + existOrders.Count());
                    log.Info("Start get items for new orders");

                    var index = 0;
                    var requestUpdateItems = false;
                    foreach (var marketOrder in newOrders)
                    {
                        if (cancel.HasValue && cancel.Value.IsCancellationRequested)
                        {
                            _log.Info("Cancellation Requested!");
                            cancel.Value.ThrowIfCancellationRequested();
                        }

                        _log.Info("Creating order=" + marketOrder.OrderId + ", status=" + marketOrder.OrderStatus);

                        IList<ListingOrderDTO> sourceOrderItems = new List<ListingOrderDTO>();

                        //1. Get Items (from Market)
                        if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                        {
                            sourceOrderItems = getOrderItemsFromMarketFunc(log, api, syncInfo, marketOrder.OrderId);
                        }
                        if (market == MarketType.eBay 
                            || market == MarketType.Magento
                            || market == MarketType.Shopify
                            || market == MarketType.WooCommerce
                            || market == MarketType.Groupon
                            || market == MarketType.Walmart
                            || market == MarketType.WalmartCA
                            || market == MarketType.Jet                            
                            || market == MarketType.DropShipper
                            || market == MarketType.OverStock
                            || market == MarketType.OfflineOrders)
                        {
                            sourceOrderItems = marketOrder.Items; //Already have items information
                        }

                        try
                        {
                            //2. Checking listings (inventory may haven't some listing, ex. not synced new listings)
                            //In this case skipping calculation, waiting listing
                            var allListingsUpdated = FillOrderListings(log, db, api, marketOrder, sourceOrderItems, marketOrder.MarketplaceId);
                            if (allListingsUpdated)
                            {
                                PrepareOrderInfo(marketOrder, sourceOrderItems);

                                var sourceOrderItemsGroups = DivideOrderItemsByDropShippers(sourceOrderItems);

                                var groupIndex = 1;
                                foreach (var sourceOrderItemGroup in sourceOrderItemsGroups)
                                {
                                    marketOrder.AutoDSSelection = true;
                                    marketOrder.DropShipperId = sourceOrderItemGroup.Select(oi => oi.DropShipperId).FirstOrDefault();
                                    var orderItems = BuildOrderItemsFromSource(db, sourceOrderItemGroup);

                                    decimal? subOrderAmountPercent = null;
                                    if (sourceOrderItemsGroups.Count > 0)
                                    {
                                        subOrderAmountPercent = AdjustOrderAmounts(marketOrder, orderItems, sourceOrderItems);

                                        marketOrder.SubOrderNumber = groupIndex;
                                        marketOrder.SubOrderAmountPercent = subOrderAmountPercent;
                                    }

                                    //3. Create Order
                                    CreateOrder(log,
                                        db,
                                        syncInfo,
                                        quantityManager,
                                        emailService,
                                        validatorService,
                                        cacheService,
                                        companyAddress,
                                        marketOrder,
                                        orderItems,
                                        sourceOrderItems,
                                        time);

                                    RemoveMissingOrder(marketOrder.MarketOrderId);
                                }
                            }
                            else
                            {
                                var missingSKUs = sourceOrderItems.Where(s => !s.StyleItemId.HasValue).Select(s => s.SKU + "-" + s.SourceMarketId);
                                var message = "Not all listing items were found for order, orderId=" + marketOrder.OrderId +
                                         ", missing SKUs: " + String.Join("; ", missingSKUs);

                                log.Warn(message);
                                syncInfo.AddWarning(marketOrder.OrderId, "Not all listing items were found for order");
                                emailService.SendSystemEmailToAdmin("Listing items weren't found for order, orderId=" + marketOrder.OrderId, message);

                                AddMissingOrder(marketOrder, message, missingSKUs.ToArray());
                            }

                            requestUpdateItems = requestUpdateItems || !allListingsUpdated;
                        }
                        catch (SqlException ex)
                        {
                            LogError(log, syncInfo, ex, marketOrder.OrderId, "SqlException when creating order");
                            //In this case dbContext stay invalid, exit and going to recreate context
                            return new SyncResult() { IsSuccess = false };
                        }
                        catch (DbUpdateException ex)
                        {
                            LogError(log, syncInfo, ex, marketOrder.OrderId, "DbUpdateException when creating order");
                            //In this case dbContext stay invalid, exit and goint to recreate context
                            return new SyncResult() {IsSuccess = false};
                        }
                        catch (Exception ex)
                        {
                            LogError(log, syncInfo, ex, marketOrder.OrderId, "Exception when creating order");
                        }
                        index++;
                        if (index % 10 == 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(2));
                            syncInfo.PingSync();
                        }
                    }
                    syncInfo.PingSync();

                    if (requestUpdateItems)
                    {
                        log.Info("Item update requested");

                        settings.SetListingsManualSyncRequest(true, _api.Market, _api.MarketplaceId);
                    }

                    log.Info("End store new orders with items");
                }

                return new SyncResult()
                {
                    IsSuccess = true,
                    ProcessedOrders = newOrders.Select(o => new SyncResultOrderInfo()
                    {
                        OrderId = o.OrderId,
                        OrderStatus = o.OrderStatus
                    }).ToList(),
                    SkippedOrders = existOrders.Select(o => new SyncResultOrderInfo()
                    {
                        OrderId = o.OrderId,
                        OrderStatus = o.OrderStatus
                    }).ToList(),
                };
            }
            catch (Exception ex)
            {
                syncInfo.AddError("", "Exception when processing orders pack", ex);
                log.Error("Error when processing orders", ex);
                return new SyncResult() { IsSuccess = false };
            }
        }

        private void RemoveMissingOrder(string orderId)
        {
            if (_messageService != null)
                _messageService.Remove(SystemMessageServiceHelper.MissingOrderKey, orderId);
        }

        private void AddMissingOrder(DTOOrder order,
            string message,
            string[] missingSkuList)
        {
            if (_messageService != null)
                _messageService.AddOrUpdateError(SystemMessageServiceHelper.MissingOrderKey,
                    order.MarketOrderId,
                    message,
                    new MissingOrderMessageData()
                    {
                        OrderId = order.OrderId,
                        OrderDate = order.OrderDate,
                        Market = (MarketType)order.Market,
                        MarketplaceId = order.MarketplaceId,
                        MissingSKUList = missingSkuList,
                    });
        }

        public bool FillOrderListings(ILogService log,
            IUnitOfWork db,
            IMarketApi api,
            DTOMarketOrder order,
            IList<ListingOrderDTO> orderItems,
            string orderMarketplaceId) //NOTE: only using for listings lookup (on Amazon)
        {
            //Check order listings
            var allListingsUpdated = FillOrderItemsBy(log, db, api, orderItems);
            
            return allListingsUpdated;
        }

        protected abstract bool FillOrderItemsBy(ILogService log,
            IUnitOfWork db,
            IMarketApi api,
            IList<ListingOrderDTO> orderItems);

        protected virtual void PrepareOrderInfo(DTOOrder marketOrder, IList<ListingOrderDTO> orderItems)
        {
            //Nothing
        }

        private IList<List<ListingOrderDTO>> DivideOrderItemsByDropShippers(IList<ListingOrderDTO> orderItems)
        {
            return orderItems.GroupBy(oi => oi.DropShipperId, oi => oi)
                    .Select(g => g.ToList())
                    .ToList();
        }


        private decimal AdjustOrderAmounts(DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> allItems)
        {
            var sum = (decimal)allItems.Sum(i => i.ItemPrice);
            var percent = sum == 0 ? 1 : orderItems.Sum(i => i.ItemPrice) / sum;
            marketOrder.TotalPaid = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceTotalPaid);
            marketOrder.TotalPrice = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceTotalPrice);
            marketOrder.TaxAmount = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceTaxAmount);

            marketOrder.ShippingPrice = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceShippingPrice);
            marketOrder.ShippingPaid = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceShippingPaid);
            marketOrder.ShippingTaxAmount = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceShippingTaxAmount);
            marketOrder.ShippingDiscountAmount = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceShippingDiscountAmount);

            marketOrder.DiscountAmount = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceDiscountAmount);
            marketOrder.DiscountTax = PriceHelper.RoundToTwoPrecision(percent * marketOrder.SourceDiscountTax);

            //foreach (var orderItem in orderItems)
            //{
            //    orderItem.ShippingPaid = percent * orderItem.SourceShippingPaid;
            //    orderItem.ShippingPrice = percent * orderItem.SourceShippingPrice;
            //    orderItem.ShippingDiscount = percent * orderItem.SourceShippingDiscount;
            //}

            return percent;
        }

        protected bool ProcessExistOrdersPack(CancellationToken? cancel,
            ILogService log,
            IMarketApi api,
            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc,
            ISyncInformer syncInfo,
            IUnitOfWork db,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,
            ICompanyAddressService companyAddress,
            IList<Order> ordersToProcess,
            ITime time)
        {
            try
            {
                var market = api.Market;

                var marketOrders = 
                    api.GetOrders(log,
                            ordersToProcess
                            .Select(e => String.IsNullOrEmpty(e.MarketOrderId) ? e.AmazonIdentifier : e.MarketOrderId)
                            .ToList()
                    )
                    .ToList();

                log.Info("Start get items for orders: " + marketOrders.Count());

                foreach (var dbOrder in ordersToProcess)
                {
                    if (cancel.HasValue && cancel.Value.IsCancellationRequested)
                    {
                        _log.Info("Cancellation Requested!");
                        cancel.Value.ThrowIfCancellationRequested();
                    }

                    var marketOrder = marketOrders.FirstOrDefault(o => o.OrderId == dbOrder.AmazonIdentifier);
                    if (marketOrder == null && api.Market == MarketType.eBay)
                    {
                        log.Info("No market order for=" + dbOrder.AmazonIdentifier + ", changed orderStatus: " + dbOrder.OrderStatus + "=>" + OrderStatusEnumEx.Canceled);
                        //dbOrder.OrderStatus = OrderStatusEnumEx.Canceled;
                        db.Commit();
                        continue;
                    }

                    if (marketOrder == null)
                    {
                        log.Info("No market order for=" + dbOrder.AmazonIdentifier);
                        //dbOrder.OrderStatus = OrderStatusEnumEx.Canceled;
                        db.Commit();
                        continue;
                    }

                    marketOrder.Id = dbOrder.Id;

                    log.Info("Update order: " + marketOrder.OrderId + "; exist status: " + dbOrder.OrderStatus + "; new status: " + marketOrder.OrderStatus);

                    //NOTE: reason to skip: status stay unchanged and order has stamps price
                    var hasCalculatedShippings = dbOrder.ShippingCalculationStatus == (int)ShippingCalculationStatusEnum.FullCalculation
                        || dbOrder.ShippingCalculationStatus == (int)ShippingCalculationStatusEnum.FullWithNoRateCalculation;
                    var needStampsPrice = ShippingUtils.IsOrderUnshipped(dbOrder.OrderStatus);

                    //NOTE: Example when order Canceled status was updated by system (it will be not equal to SourceOrderStatus),
                    //applicable only to Amazon
                    var hasModifiedOrderStatus = (api.Market == MarketType.Amazon 
                                                    || api.Market == MarketType.AmazonEU
                                                    || api.Market == MarketType.AmazonAU)
                                                      && dbOrder.OrderStatus != dbOrder.SourceOrderStatus;

                    //NOTE: Items changed (Amazon also provide this information w/o additional item request)
                    var hasModifiedItems = dbOrder.Quantity != marketOrder.Quantity
                                           || dbOrder.TotalPrice != marketOrder.TotalPrice;

                    if (marketOrder.OrderStatus == dbOrder.OrderStatus
                        && !hasModifiedOrderStatus
                        && !hasModifiedItems
                        && (!needStampsPrice || hasCalculatedShippings)
                        && api.Market != MarketType.eBay
                        && api.Market != MarketType.Magento
                        && api.Market != MarketType.Walmart
                        && api.Market != MarketType.Shopify
                        && api.Market != MarketType.WooCommerce
                        && api.Market != MarketType.Groupon
                        && api.Market != MarketType.WalmartCA
                        && api.Market != MarketType.Jet
                        && api.Market != MarketType.OverStock) //NOTE: eBay/Magento/Walmart/Jet orders was updates everytime
                    {
                        continue;
                    }

                    #region Process Order Items changes, retrieve actual version from Db

                    if ((!ShippingUtils.IsOrderUnshipped(dbOrder.OrderStatus) //if CURRENT status Unshipped
                        || hasModifiedItems
                        || ShippingUtils.IsOrderPartial(marketOrder.OrderStatus)) //NEW satus Partial
                        && !ShippingUtils.IsOrderCanceled(marketOrder.OrderStatus)) //Prevent any item request/updates for canceled orders (Amazon may return no items)
                    {
                        //1. Get Items (from Market)
                        if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                        {
                            _log.Info("Request Amazon Items");
                            marketOrder.Items = getOrderItemsFromMarketFunc(log, api, syncInfo, marketOrder.OrderId);
                        }
                        if (market == MarketType.eBay)
                        {
                            //Already have items information
                        }
                        if (market == MarketType.Magento)
                        {
                            //Already have items information
                        }
                        if (market == MarketType.Groupon)
                        {
                            //Already have items information
                        }
                        if (market == MarketType.Walmart)
                        {
                            //Already have items information
                        }
                        if (market == MarketType.WalmartCA)
                        {
                            //Already have items information
                        }
                        if (market == MarketType.Jet)
                        {
                            //Already have items information
                        }

                        _log.Info("UpdateOrderItems");
                        UpdateOrderItems(log, db, dbOrder.Id, marketOrder.Items, time.GetAppNowTime());

                        if (hasModifiedItems
                            && !ShippingUtils.IsOrderShipped(marketOrder.OrderStatus)
                            && !ShippingUtils.IsOrderCanceled(marketOrder.OrderStatus)
                            && !ShippingUtils.IsOrderPartial(marketOrder.OrderStatus)) //Prevent recalculation for Shipped, Canceled, PartialShipped orders
                        {
                            _log.Info("Reset ShippingCalculationStatus");
                            //Force shipping recalculation
                            if (dbOrder.ShippingCalculationStatus != (int)ShippingCalculationStatusEnum.WoWeightCalculation)
                                dbOrder.ShippingCalculationStatus = (int) ShippingCalculationStatusEnum.NoCalculation;
                        }
                        dbOrder.Quantity = marketOrder.Quantity;
                        dbOrder.TotalPrice = marketOrder.TotalPrice;
                    }

                    marketOrder.Items = db.Listings.GetOrderItems(dbOrder.Id);
                    marketOrder.SourceItems = db.Listings.GetOrderItemSources(dbOrder.Id);

                    if (!marketOrder.Items.Any())
                    {
                        log.Debug("No items found for order: " + marketOrder.OrderId);
                    }

                    validatorService.OrderValidationStepAlwaysInitial(db,
                        time,                        
                        marketOrder,
                        marketOrder.Items,
                        dbOrder);

                    log.Debug("marketOrder.Items=" + marketOrder.Items.Count);
                    #endregion

                    if (!ShippingUtils.IsOrderCanceled(marketOrder.OrderStatus))
                    {
                        //3. Move order status from PENDING to NEW (not pending), happen once (INITIAL PART)
                        var convertFromPending = ShippingUtils.IsOrderPending(dbOrder.OrderStatus)
                                                 && !ShippingUtils.IsOrderPending(marketOrder.OrderStatus);

                        //NOTE: Exclude any validation for Amazon Fulfillment orders
                        //perform validation only to Unshipped / Partial shipped
                        var enableValidation = marketOrder.FulfillmentChannel != "AFN"
                                               && (marketOrder.OrderStatus == OrderStatusEnumEx.Unshipped
                                                   || marketOrder.OrderStatus == OrderStatusEnumEx.PartiallyShipped);

                        if (convertFromPending)
                        {
                            UpdateOrder(log, db, dbOrder, marketOrder, time.GetAppNowTime());

                            if (!String.IsNullOrEmpty(dbOrder.BuyerEmail))
                                db.Buyers.CreateIfNotExistFromOrderDto(marketOrder, time.GetUtcTime());

                            //NOTE: Exclude any validation for Amazon Fulfillment orders
                            //perform validation only to Unshipped / Partial shipped
                            if (enableValidation)
                            {
                                validatorService.OrderValidationStepInitial(db,
                                    time,
                                    _company,
                                    marketOrder,
                                    marketOrder.Items,
                                    dbOrder);
                            }

                            syncInfo.AddSuccess(marketOrder.OrderId,
                                "Order was converted from Pending to " + marketOrder.OrderStatus);
                        }

                       
                        //NOTE: Updates rates in cases:
                        //- when no rates calculated
                        //- when all items have weight and only woWeight rates calculated
                        var allItemsHasWeight = marketOrder.Items.All(i => i.Weight.HasValue);
                        if (!ShippingUtils.IsOrderShipped(marketOrder.OrderStatus)
                            && (dbOrder.ShippingCalculationStatus == (int)ShippingCalculationStatusEnum.NoCalculation
                                || (dbOrder.ShippingCalculationStatus == (int)ShippingCalculationStatusEnum.WoWeightCalculation
                                    && allItemsHasWeight)))
                        {
                            marketOrder.IsInsured = dbOrder.IsInsured;
                            marketOrder.InsuredValue = dbOrder.InsuredValue;
                            marketOrder.IsSignConfirmation = dbOrder.IsSignConfirmation;

                            var rateProvider = GetRateProviderForOrder(db, marketOrder, marketOrder.Items, marketOrder.SourceItems, _rateProviders);

                            marketOrder.ShipmentProviderType = (int)rateProvider.Type; //NOTE: update provider, in case when CreateOrder ShippingCountry may be empty

                            if (UpdateShippings(log, 
                                db, 
                                rateProvider, 
                                rateProviders,
                                syncInfo, 
                                companyAddress,
                                time, 
                                dbOrder, 
                                marketOrder, 
                                marketOrder.Items, 
                                marketOrder.SourceItems,
                                false))
                            {
                                validatorService.ShippingValidationStep(db,                                     
                                    marketOrder.Items,
                                    marketOrder.SourceItems,
                                    marketOrder.ShippingInfos, 
                                    marketOrder,
                                    dbOrder);
                            }
                        }

                        //3. Move order status from PENDING to NEW (not pending), happen once (FINAL PART)
                        if (convertFromPending)
                        {
                            //NOTE: Exclude any validation for Amazon Fulfillment orders
                            //perform validation only to Unshipped / Partial shipped
                            if (enableValidation)
                            {
                                validatorService.OrderValidationStepFinal(db,
                                    time,
                                    _company,
                                    marketOrder,
                                    marketOrder.Items,
                                    marketOrder.ShippingInfos,
                                    dbOrder);
                            }
                        }
                    }
                    else
                    {
                        if (ShippingUtils.IsOrderCanceled(marketOrder.OrderStatus))
                        {
                            foreach (var item in marketOrder.Items)
                            {
                                log.Info("Getting back canceled quantity for item: " + item.ASIN + "; listing: " + item.ListingId + "; canceled quantity: " + item.QuantityOrdered);

                                //NOTE: skip FBA orders
                                if (marketOrder.FulfillmentChannel != "AFN")
                                {
                                    quantityManager.Process(db,
                                        item.Id,
                                        item.QuantityOrdered,
                                        QuantityChangeSourceType.OrderCancelled,
                                        marketOrder.OrderId,
                                        item.StyleId,
                                        item.StyleID,
                                        item.StyleItemId,
                                        item.SKU,
                                        item.Size);

                                    if (item.StyleId.HasValue)
                                    {
                                        SystemActionHelper.RequestQuantityDistribution(db,
                                            _systemAction,
                                            item.StyleId.Value,
                                            null);
                                    }
                                }
                            }
                        }
                    }

                    validatorService.OrderValidationStepAlways(db,
                                    time,
                                    api,
                                    _company,
                                    marketOrder,
                                    marketOrder.Items,
                                    marketOrder.ShippingInfos,
                                    dbOrder);

                    //Always updating general info
                    _orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.StatusChangedKey, dbOrder.OrderStatus, marketOrder.OrderStatus, null);

                    var skipStatusUpdating = false;

                    //NOTE: if system marked order as cancelled skip any other changes
                    if (dbOrder.OrderStatus == OrderStatusEnumEx.Canceled)
                        skipStatusUpdating = true;

                    //NOTE: when we set status Unshipped (as example direct in DB, skip overriding back to Pending)
                    if ((_api.Market == MarketType.WalmartCA
                        || _api.Market == MarketType.Walmart)
                        && dbOrder.OrderStatus == OrderStatusEnumEx.Unshipped
                        && marketOrder.OrderStatus == OrderStatusEnumEx.Pending)
                        skipStatusUpdating = true;

                    if (!skipStatusUpdating)
                    {
                        dbOrder.OrderStatus = marketOrder.OrderStatus;
                    }

                    dbOrder.SourceOrderStatus = marketOrder.SourceOrderStatus;
                    dbOrder.EstDeliveryDate = marketOrder.EarliestDeliveryDate;
                    dbOrder.LatestDeliveryDate = marketOrder.LatestDeliveryDate;

                    dbOrder.InitialServiceType = marketOrder.InitialServiceType;

                    dbOrder.EarliestShipDate = marketOrder.EarliestShipDate;
                    dbOrder.LatestShipDate = marketOrder.LatestShipDate;

                    if (marketOrder.ShipmentProviderType != (int)ShipmentProviderType.None)
                        dbOrder.ShipmentProviderType = marketOrder.ShipmentProviderType;

                    //eBay
                    dbOrder.PaidDate = marketOrder.PaidDate;

                    dbOrder.UpdateDate = DateHelper.GetAppNowTime();
                   

                    db.Commit();
                }
                log.Info("End get orders from api");
                log.Info("Total orders:" + marketOrders.Count());
                return true;
            }
            catch (Exception ex)
            {
                syncInfo.AddError("", "Exception when processing orders pack", ex);
                log.Error("Error when processing orders", ex);
                return false;
            }
        }

        private IList<ListingOrderDTO> BuildOrderItemsFromSource(IUnitOfWork db, IList<ListingOrderDTO> sourceItems)
        {
            var itemStyleIds = sourceItems.Where(oi => oi.StyleId.HasValue).Select(oi => oi.StyleId).ToList();
            var styles = db.Styles.GetAllActiveAsDto().Where(s => itemStyleIds.Contains(s.Id)).ToList();

            var resultItems = new List<ListingOrderDTO>();
            foreach (var item in sourceItems)
            {
                var wasAdded = false;
                var itemStyle = item.StyleId.HasValue ? styles.FirstOrDefault(s => s.Id == item.StyleId) : null;
                if (itemStyle != null)
                {
                    if (itemStyle.Type == (int)StyleTypes.References)
                    {
                        var refStyles = db.StyleReferences.GetByStyleId(itemStyle.Id);
                        var refStyleItems = db.StyleItemReferences.GetByStyleId(itemStyle.Id).Where(si => si.StyleItemId == item.StyleItemId).ToList();

                        //var balanceFactor = refStyles.Count != 0 ? 1 / (decimal)refStyles.Count : (decimal)1;
                        var balanceFactors = new Dictionary<long, decimal>();

                        var nonZeroStyleCount = refStyles.Where(i => i.Price != 0 || !i.Price.HasValue).Count();
                        var zeroStyleCount = refStyles.Where(i => i.Price == 0).Count();
                        //var notNullSumPrice = refStyles.Where(i => i.Price.HasValue).Sum(i => i.Price.Value);
                        foreach (var refStyle in refStyles)
                        {
                            if (balanceFactors.ContainsKey(refStyle.LinkedStyleId)) //NOTE: when multiple reference for the same styles
                                continue;

                            if (refStyle.Price == 0)
                                balanceFactors.Add(refStyle.LinkedStyleId, 0);
                            else
                                balanceFactors.Add(refStyle.LinkedStyleId, 1 / (decimal)nonZeroStyleCount);
                        }
                        var index = 0;

                        foreach (var refStyleItem in refStyleItems)
                        {
                            index++;
                            var linkStyleItem = db.StyleItems.GetAllAsDto().FirstOrDefault(si => si.StyleItemId == refStyleItem.LinkedStyleItemId);
                            var linkStyle = db.Styles.GetAllAsDto().FirstOrDefault(s => s.Id == linkStyleItem.StyleId);
                            //var refStyleItem = refStyleItems.FirstOrDefault(si => si.StyleItemId == item.StyleItemId);

                            var balance = balanceFactors.ContainsKey(linkStyleItem.StyleId) ? balanceFactors[linkStyleItem.StyleId] : 0;

                            resultItems.Add(new ListingOrderDTO()
                            {
                                Id = item.Id, //NOTE: ListingId
                                OrderId = item.OrderId,

                                ReplaceType = (int)ItemReplaceTypes.Combined,

                                ListingId = item.ListingId,
                                SourceListingId = item.Id,

                                StyleId = linkStyle?.Id,
                                StyleItemId = linkStyleItem?.StyleItemId,
                                StyleID = linkStyle?.StyleID,
                                Weight = linkStyleItem?.Weight, //item.Weight.HasValue ? item.Weight.Value * (double)balanceFactor : (double?)null, //NOTE: this is total combined Weight, divide it

                                SourceStyleItemId = item.StyleItemId,
                                SourceStyleString = item.StyleID,
                                SourceStyleSize = item.StyleSize,
                                SourceStyleColor = item.StyleColor,

                                ItemOrderId = item.ItemOrderId + "_" + linkStyle?.StyleID + "_" + index,
                                SourceItemOrderIdentifier = item.ItemOrderId,

                                RecordNumber = item.RecordNumber,

                                QuantityOrdered = item.QuantityOrdered,
                                QuantityShipped = item.QuantityShipped,

                                ItemPrice = item.ItemPrice * balance,
                                ItemTax = item.ItemTax * balance,
                                ItemPriceCurrency = item.ItemPriceCurrency,
                                ItemPriceInUSD = item.ItemPriceInUSD * balance,

                                PromotionDiscount = item.PromotionDiscount * balance,
                                PromotionDiscountCurrency = item.PromotionDiscountCurrency,
                                PromotionDiscountInUSD = item.PromotionDiscountInUSD * balance,

                                ShippingPrice = item.ShippingPrice * balance,
                                ShippingTax = item.ShippingTax * balance,
                                ShippingPriceCurrency = item.ShippingPriceCurrency,
                                ShippingPriceInUSD = item.ShippingPriceInUSD * balance,

                                ShippingDiscount = item.ShippingDiscount * balance,
                                ShippingDiscountCurrency = item.ShippingDiscountCurrency,
                                ShippingDiscountInUSD = item.ShippingDiscountInUSD * balance,

                                //Copy Full Listing Result
                                SKU = item.SKU,
                                ShippingSize = item.ShippingSize,
                                InternationalPackage = item.InternationalPackage,
                                ItemStyle = item.ItemStyle,
                                RestockDate = item.RestockDate,
                                Size = item.Size,
                                ParentASIN = item.ParentASIN,
                                Picture = item.Picture,

                                RealQuantity = item.RealQuantity
                            });
                        }

                        wasAdded = true;
                    }
                }

                if (!wasAdded)
                {
                    //Fill source fields

                    item.ReplaceType = (int) ItemReplaceTypes.None;
                    item.SourceItemOrderIdentifier = item.ItemOrderId;

                    item.SourceListingId = item.Id;

                    item.SourceStyleItemId = item.StyleItemId;
                    item.SourceStyleString = item.StyleID;
                    item.SourceStyleSize = item.StyleSize;
                    item.SourceStyleColor = item.StyleColor;

                    resultItems.Add(item);
                }
            }

            return resultItems;
        }

        protected bool CreateOrder(ILogService log,
            IUnitOfWork db,
            ISyncInformer syncInfo,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,
            ICacheService cacheService,
            ICompanyAddressService companyAddress,
            DTOOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            ITime time)
        {
            if (ShippingUtils.ShouldCorrectState(marketOrder.ShippingState, marketOrder.ShippingCountry))
            {
                marketOrder.ShippingState = db.States.GetCodeByName(marketOrder.ShippingState);
            }

            if (orderItems.Any())
            {
                //1. Create Order 
                //NOTE: need orderId for Order Notifies
                var dbOrder = db.Orders.CreateFromDto(marketOrder, time.GetAppNowTime());
                _orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.StatusChangedKey, null, marketOrder.OrderStatus, null);

                marketOrder.Id = dbOrder.Id;
                dbOrder.AddressValidationStatus = (int)AddressValidationStatus.None;

                db.OrderItemSources.StoreOrderItems(dbOrder.Id, sourceOrderItems, time.GetAppNowTime());
                db.OrderItems.StoreOrderItems(dbOrder.Id, orderItems, time.GetAppNowTime());
                
                if (!String.IsNullOrEmpty(dbOrder.BuyerEmail))
                    db.Buyers.CreateIfNotExistFromOrderDto(marketOrder, time.GetUtcTime());

                db.Customers.CreateIfNotExistFromOrderDto(marketOrder, time.GetUtcTime());

                dbOrder.CustomerId = marketOrder.CustomerId;

                //1.2 Update quantity log
                //NOTE: skip FBA orders
                if (marketOrder.FulfillmentChannel != "AFN")
                {
                    foreach (var item in orderItems)
                    {
                        log.Info("Write history/subtracting quantity/check sale end for item: " + item.ASIN +
                                 "; listing: " + item.ListingId + "; quantity: " + item.QuantityOrdered);

                        quantityManager.Process(db,
                            item.Id, //NOTE: listing id
                            item.QuantityOrdered,
                            QuantityChangeSourceType.NewOrder,
                            marketOrder.OrderId,
                            item.StyleId,
                            item.StyleID,
                            item.StyleItemId,
                            item.SKU,
                            item.Size);
                    }
                }


                //2. Check/validate order (INITIAL)
                var rateProvider = GetRateProviderForOrder(db, marketOrder,
                    orderItems,
                    sourceOrderItems,
                    _rateProviders);
                marketOrder.ShipmentProviderType = (int)rateProvider.Type; //NOTE: required for validations

                
                //validation happen when Pending => Unshipped in other cases (Canceled, (already) Shipped) validation was skipped 
                //NOTE: Exclude any validation for Amazon Fulfillment orders
                var enableValidation = marketOrder.FulfillmentChannel != "AFN" &&
                                       (marketOrder.OrderStatus == OrderStatusEnumEx.Unshipped ||
                                        marketOrder.OrderStatus == OrderStatusEnumEx.PartiallyShipped);
                if (enableValidation)
                {
                    validatorService.OrderValidationStepInitial(db,
                        time,
                        _company,
                        marketOrder,
                        orderItems,
                        dbOrder);
                }

                //2.1 Create Shippings
                marketOrder.IsInsured = dbOrder.IsInsured;
                marketOrder.InsuredValue = dbOrder.InsuredValue;
                marketOrder.IsSignConfirmation = dbOrder.IsSignConfirmation;
                
                if (UpdateShippings(log, 
                    db, 
                    rateProvider, 
                    _rateProviders,
                    syncInfo, 
                    companyAddress,
                    time, 
                    dbOrder, 
                    marketOrder, 
                    orderItems, 
                    sourceOrderItems,
                    true))
                {
                    validatorService.ShippingValidationStep(db, 
                        orderItems,
                        sourceOrderItems,
                        marketOrder.ShippingInfos, 
                        marketOrder,
                        dbOrder);
                }

                //2.2 Check/validate order (FINAL)
                if (enableValidation)
                {
                    validatorService.OrderValidationStepFinal(db,
                        time,
                        _company,
                        marketOrder,
                        orderItems,
                        marketOrder.ShippingInfos,
                        dbOrder);
                }

                if (marketOrder.ShipmentProviderType != (int) ShipmentProviderType.None)
                    dbOrder.ShipmentProviderType = marketOrder.ShipmentProviderType;

                db.Commit();

                //2.1 Update cache
                cacheService.RequestStyleItemIdUpdates(db,
                    orderItems.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId.Value).ToList(),
                    null);
            }
            else
            {
                log.Warn("The order has no items, orderId=" + marketOrder.OrderId);
                syncInfo.AddWarning(marketOrder.OrderId, "The order has no items");
            }
            return true;
        }

        

        private void UpdateOrder(ILogService log,
            IUnitOfWork db,
            Order dbOrder,
            DTOOrder marketOrder,
            DateTime? when)
        {
            if (ShippingUtils.ShouldCorrectState(marketOrder.ShippingState, marketOrder.ShippingCountry))
            {
                marketOrder.ShippingState = db.States.GetCodeByName(marketOrder.ShippingState);
            }

            dbOrder.BuyerName = marketOrder.BuyerName;
            dbOrder.BuyerEmail = marketOrder.BuyerEmail;
            dbOrder.PersonName = marketOrder.PersonName;
            dbOrder.ShippingAddress1 = marketOrder.ShippingAddress1;
            dbOrder.ShippingAddress2 = marketOrder.ShippingAddress2;
            dbOrder.ShippingCountry = marketOrder.ShippingCountry;
            dbOrder.ShippingCity = marketOrder.ShippingCity;
            dbOrder.ShippingState = marketOrder.ShippingState;
            dbOrder.ShippingPhone = marketOrder.ShippingPhone;
            dbOrder.ShippingZip = marketOrder.ShippingZip;
            dbOrder.ShippingZipAddon = marketOrder.ShippingZipAddon;
            dbOrder.AddressValidationStatus = (int)AddressValidationStatus.None;

            dbOrder.TotalPrice = marketOrder.TotalPrice;
            dbOrder.TotalPriceCurrency = marketOrder.TotalPriceCurrency;

            db.Commit();
        }


        
        #endregion


        #region Helper Methods
        protected void RateListValidation(ILogService log, IList<RateDTO> rateList)
        {
            List<RateDTO> rateInfoList = rateList.Where(r => r.GroupId == 0).ToList();
            rateInfoList.AddRange(rateList.Where(r => r.GroupId != 0)
                .GroupBy(r => r.GroupId, (key, group) => new RateDTO()
                {
                    GroupId = key,
                    PackageTypeUniversal = group.First().PackageTypeUniversal,
                    ServiceTypeUniversal = group.First().ServiceTypeUniversal,
                    Amount = group.Sum(r => r.Amount),
                })
                .ToList());

            //Note: temporary validation
            if (rateInfoList.Count(r => r.IsDefault) > 1)
            {
                log.Fatal("Need review. Rate list have 2 rates/rates group with isDefault flag");
                foreach (var r in rateList)
                    r.IsDefault = false;
            }
            
        }
        
        protected void LogError(ILogService log, ISyncInformer syncInfo, Exception ex, string orderId, string message)
        {
            syncInfo.AddError(orderId, message, ex);
            log.Error(message + ": " + orderId, ex);
        }
        #endregion
    }
}
