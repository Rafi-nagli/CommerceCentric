using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Exceptions;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.DTO.Orders;

namespace Amazon.Common.Helpers
{
    public class RateHelper
    {
        public const int StandardPartialGroupId = 1000;
        public const int FlatPartialGroupId = 2000;
        public const int ExpressFlatPartialGroupId = 3000;
        
        public const int RegularPartialInternationalGroupId = 1100;
        public const int FlatPartialInternationalGroupId = 1200;

        public const int CustomPartialGroupId = 5000;

        public static bool IsMultiPackageGroup(int shippingGroupId)
        {
            return shippingGroupId >= StandardPartialGroupId;
        }

        public static AddressDTO GetSampleUSAddress()
        {
            return new AddressDTO()
            {
                Country = "US",
                Address1 = "19 GRACE CT APT 2A",
                City = "BROOKLYN",
                State = "NY",
                Zip = "11201",
            };
        }

        public static AddressDTO GetSampleCAAddress()
        {
            return new AddressDTO()
            {
                Country = "CA",
                Address1 = "11-10 Sword Street",
                City = "Toronto",
                State = "Ontario",
                Zip = "M5A 3N2",
            };
        }

        public static AddressDTO GetSampleUKAddress()
        {
            return new AddressDTO()
            {
                Country = "GB",
                Address1 = "26 OREGANO WAY",
                City = "GUILDFORD",
                State = "Surrey",
                Zip = "GU2 9YT",
            };
        }

        public static AddressDTO GetSampleAUAddress()
        {
            return new AddressDTO()
            {
                Country = "AU",
                Address1 = "16 Bungonia Street",
                City = "PRESTONS",
                State = "NSW",
                Zip = "2170",
            };
        }

        public static RateDTO GetRougeChipestRate(ILogService log,
            IShipmentApi rateProvider,
            AddressDTO fromAddress,
            AddressDTO toAddress,
            double weight,
            DateTime shipDate,
            ShippingTypeCode shippingService,
            PackageTypeCode packageType)
        {
            var result = rateProvider.GetAllRate(
                fromAddress,
                fromAddress,
                toAddress,
                shipDate,
                weight,
                null,
                0,
                false,
                null,
                RetryModeType.Normal);

            if (result.Rates != null && result.Rates.Any())
            {
                var packageTypes = new List<int>() { (int)packageType };
                if (packageType == PackageTypeCode.Flat
                    || packageType == PackageTypeCode.LargeEnvelopeOrFlat)
                    packageTypes.Add((int)PackageTypeCode.Regular);

                return result.Rates.Where(r => packageTypes.Contains(r.PackageTypeUniversal))
                    .OrderBy(r => r.Amount)
                    .FirstOrDefault();
            }
            return null;
        }


        public static IList<RateDTO> GetRougeChipestUSRate(ILogService log,
            IShipmentApi rateProvider,
            AddressDTO fromAddress,
            DTOMarketOrder order,
            string shippingService,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems)
        {
            var toAddress = GetSampleUSAddress();
            
            //TODO: Group combined items
            var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(orderItems);
            var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(sourceOrderItems);

            var result = rateProvider.GetLocalRate(
                fromAddress,
                fromAddress,
                toAddress,
                DateHelper.GetAppNowTime(),
                order.WeightD,
                null,
                0,
                false,
                new OrderRateInfo()
                {
                     OrderNumber = order.OrderId,

                     Items = orderItemInfoes,
                     SourceItems = sourceOrderItemInfoes,

                     EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(order.LatestShipDate, (MarketType)order.Market),
                     ShippingService = shippingService,
                     InitialServiceType = shippingService,
                     TotalPrice = order.TotalPrice,
                     Currency = order.TotalPriceCurrency,
                },
                RetryModeType.Normal);

            if (result.Rates != null && result.Rates.Any())
            {
                var serviceType = ShippingServiceUtils.AmazonServiceToUniversalServiceType(shippingService);
                var isSupportFlatEnvelope = ShippingServiceUtils.IsSupportFlatEnvelope(orderItemInfoes);
                return GetChipestVisibleRate(result.Rates, isSupportFlatEnvelope, serviceType);
            }
            return null;
        }

        public static IList<RateDTO> GetPureRates(Func<PackageInfo, IList<RateDTO>> getRates,
            ILogService log,
            string shippingService,
            IList<OrderItemRateInfo> orderItems,
            ItemPackageDTO overridePackageSize)
        {
            var defaultPackage = new PackageInfo()
            {
                Items = orderItems,

                PackageLength = overridePackageSize?.PackageLength,
                PackageWidth = overridePackageSize?.PackageWidth,
                PackageHeight = overridePackageSize?.PackageHeight,                
            };

            var retRates = getRates(defaultPackage);

            var rates = retRates.Where(r =>
                (r.PackageTypeUniversal == (int)PackageTypeCode.Regular || r.PackageTypeUniversal == (int)PackageTypeCode.Flat)
                && (r.ServiceTypeUniversal == (int)ShippingTypeCode.IPriority || r.ServiceTypeUniversal == (int)ShippingTypeCode.IPriorityExpress)).ToList();

            rates.ForEach(r => r.IsVisible = true);

            SetDefaultRates(rates, true, true, true, null);

            return rates;
        }


        public static IList<RateDTO> GetInternationalRates(Func<PackageInfo, IList<RateDTO>> getRates,
            ILogService log,
            IWeightService weightService,
            ShipmentProviderType providerType,
            string shippingService,
            PackageTypeCode internationalPackage,
            IList<OrderItemRateInfo> orderItems,
            bool isAllowIntlFlat)
        {
            var isSupportFlatEnvelope = ShippingServiceUtils.IsSupportFlatEnvelope(orderItems);
            var isSupportLargeEnvelopeOrFlat = ShippingServiceUtils.IsSupportLargeEnvelope(orderItems);

            var rates = new List<RateDTO>();
            var skipDefaultCalculation = false;
            var defaultPackage = new PackageInfo()
            {
                Items = orderItems,
            };

            if (ShippingUtils.IsServiceStandard(shippingService))
            {
                //order.WeightD <= 64
                defaultPackage.ServiceTypeUniversal = ShippingTypeCode.IStandard;

                var weight = weightService.AdjustWeight(defaultPackage.Items.Sum(i => i.Weight*i.Quantity),
                    defaultPackage.Items.Sum(i => i.Quantity));

                //NOTE: Customs line restrictions, now the customs line has automatically merge when printing
                //if (orderItems.Count <= 5)

                List<PackageInfo> packages = new List<PackageInfo>();

                if (weight > ShippingUtils.ISTANDART_MAX_WEIGTH)
                {
                    if (orderItems.Sum(i => i.Quantity) > 1)
                    {
                        var standardPackages = RateHelper.GetGroupedRatesByWeight(weightService,
                            orderItems,
                            ShippingUtils.ISTANDART_MAX_WEIGTH,
                            ShippingTypeCode.IStandard,
                            PackageTypeCode.Regular,
                            RateHelper.RegularPartialInternationalGroupId).ToList();

                        packages.AddRange(standardPackages);

                        skipDefaultCalculation = true;
                    }
                }
                else
                {
                    var oneStandardPackage = new PackageInfo()
                    {
                        Items = orderItems,
                        PackageTypeUniversal = PackageTypeCode.Regular,
                        ServiceTypeUniversal = ShippingTypeCode.IStandard
                    };

                    packages.Add(oneStandardPackage);
                }

                if (providerType != ShipmentProviderType.DhlECom
                    && isAllowIntlFlat)
                {
                    //NOTE: for case when pijames fit to one package
                    var oneFlatPackage = new PackageInfo()
                    {
                        Items = orderItems,
                        PackageTypeUniversal = PackageTypeCode.LargeEnvelopeOrFlat,
                        ServiceTypeUniversal = ShippingTypeCode.IStandard
                    };

                    packages.Add(oneFlatPackage);


                    if (!isSupportLargeEnvelopeOrFlat)
                    {
                        var flatPackages = RateHelper.GetGroupedFlatRates(log,
                            orderItems,
                            ShippingTypeCode.IStandard,
                            PackageTypeCode.LargeEnvelopeOrFlat,
                            ShippingServiceUtils.IsSupportLargeEnvelope,
                            RateHelper.FlatPartialInternationalGroupId).ToList();

                        packages.AddRange(flatPackages);

                        skipDefaultCalculation = true;
                    }
                }

                rates = RateHelper.CalculateRatesForGroup(packages, getRates);

                rates.ForEach(r => r.IsVisible = true);
            }
            else
            {
                var retRates = getRates(defaultPackage);

                rates = retRates.Where(r =>
                    (r.PackageTypeUniversal == (int)PackageTypeCode.Regular || r.PackageTypeUniversal == (int)PackageTypeCode.Flat)
                    && (r.ServiceTypeUniversal == (int)ShippingTypeCode.IPriority || r.ServiceTypeUniversal == (int)ShippingTypeCode.IPriorityExpress)).ToList();

                if (!isSupportFlatEnvelope 
                    && providerType != ShipmentProviderType.DhlECom
                    && orderItems.All(oi => oi.ShippingSize != ShippingServiceUtils.ShippingSizeXL))
                {
                    var packages = RateHelper.GetGroupedFlatRates(log,
                        orderItems,
                        ShippingTypeCode.IPriority,
                        PackageTypeCode.Flat, 
                        ShippingServiceUtils.IsSupportFlatEnvelope,
                        RateHelper.FlatPartialInternationalGroupId).ToList();

                    try
                    {
                        var flatGroupedRates = RateHelper.CalculateRatesForGroup(packages,
                            getRates);

                        if (flatGroupedRates.Sum(s => s.Amount) <= rates.Max(r => r.Amount))
                        {
                            rates.AddRange(flatGroupedRates);

                            skipDefaultCalculation = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("CalculateRatesForGroup, group=FlatPartialInternationalGroupId", ex);
                    }
                }

                rates.ForEach(r => r.IsVisible = true);
            }


            //Remove flat rate groups if cost more than regular package cost
            var regularPackage = rates.FirstOrDefault(p => p.PackageTypeUniversal == (int) PackageTypeCode.Regular
                && p.GroupId == 0);
            var flatGroupPackages = rates.Where(p => p.GroupId == RateHelper.FlatPartialInternationalGroupId).ToList();
            if (regularPackage != null && flatGroupPackages.Any())
            {
                if (regularPackage.Amount < flatGroupPackages.Sum(p => p.Amount))
                {
                    rates.RemoveAll(p => p.GroupId == RateHelper.FlatPartialInternationalGroupId);
                }
            }


            if (!skipDefaultCalculation)
            {
                //NOTE: Keep active only suitable package type
                //priority international не должна обрават внимание на International Flat флажок
                if (ShippingUtils.IsServiceStandard(shippingService))
                {
                    if (internationalPackage == PackageTypeCode.Flat &&
                        rates.Any(r => r.PackageTypeUniversal == (int)PackageTypeCode.LargeEnvelopeOrFlat))
                    {
                        SetVisibleAndDefaultFirstMatch(rates, PackageTypeCode.LargeEnvelopeOrFlat, null, false);
                    }
                    if (internationalPackage == PackageTypeCode.Regular &&
                        rates.Any(r => r.PackageTypeUniversal == (int)PackageTypeCode.Regular))
                    {
                        SetVisibleAndDefaultFirstMatch(rates, PackageTypeCode.Regular, null, false);
                    }
                    if (!rates.Any(r => r.IsDefault))
                    {
                        SetVisibleAndDefaultFirstMatch(rates, null, null, false);
                    }
                }
                else
                {
                    //Для Priority International Flat используются те же конверты что и для American Priority Flat
                    var defaultPackageType = isSupportFlatEnvelope && rates.Any()
                        ? PackageTypeCode.Flat
                        : PackageTypeCode.Regular;

                    SetVisibleAndDefaultFirstMatch(rates, defaultPackageType, null, false);
                }
            }

            if (!isAllowIntlFlat)
            {
                rates = rates.Where(r => r.PackageTypeUniversal != (int)PackageTypeCode.LargeEnvelopeOrFlat).ToList();
            }

            if (!rates.Any(r => r.IsVisible))
            {
                rates.ForEach(r => r.IsVisible = true);
            }

            if (rates.Count() == 1)
            {
                rates.ForEach(r => r.IsVisible = true);
                rates.ForEach(r => r.IsDefault = true);
            }

            return rates;
        }

        private static void SetVisibleAndDefaultFirstMatch(IList<RateDTO> rates, PackageTypeCode? package, ShippingTypeCode? service, bool onlyDefault)
        {
            if (!package.HasValue && !service.HasValue
                && rates.Any())
            {
                if (!onlyDefault)
                    rates[0].IsVisible = true;
                rates[0].IsDefault = true;
                return;
            }

            var isFoundDefault = false;
            foreach (var r in rates)
            {
                var isVisible = false;
                var isDefault = false;
                if (package.HasValue)
                {
                    isVisible = r.PackageTypeUniversal == (int)package.Value;
                    isDefault = r.PackageTypeUniversal == (int)package.Value;
                }
                if (service.HasValue)
                {
                    isVisible = isVisible && r.ServiceTypeUniversal == (int)service.Value;
                    isDefault = isDefault && r.ServiceTypeUniversal == (int)service.Value;
                }
                
                if (!onlyDefault)
                    r.IsVisible = isVisible;
                r.IsDefault = !isFoundDefault && isDefault;

                isFoundDefault = isFoundDefault || r.IsDefault;
            }
        }

        private static bool ValidateGetRateResult(IList<RateDTO> rates, ShippingTypeCode requiredShippingType)
        {
            ShipmentProviderType? providerType = null;
            if (rates.Any())
            {
                providerType = (ShipmentProviderType)rates[0].ProviderType;
            }
            if (rates.All(r => r.ServiceTypeUniversal != (int)requiredShippingType))
            {
                return false;
                //throw new RateException(String.Format("\"{0}\" provider's response doesn't contain the required rate type, type={1}",
                //    providerType,
                //    requiredShippingType));
            }
            return true;
        }

        public static IList<RateDTO> GetLocalRatesSimple(Func<PackageInfo, IList<RateDTO>> getRates,
            ILogService log,
            IWeightService weightService,
            Func<DateTime, DateTime, int> getBizDaysFunc,
            AddressDTO toAddress,
            ShipmentProviderType providerType,
            string shippingService,
            string initialShippingService,
            OrderTypeEnum orderType,
            DateTime? estShipDate,
            IList<OrderItemRateInfo> orderItems)
        {
            var serviceType = ShippingServiceUtils.AmazonServiceToUniversalServiceType(shippingService);

            var package = new PackageInfo()
            {
                Items = orderItems
            };
            var retRates = getRates(package);
            var rates = retRates.Where(
                    r => (r.PackageTypeUniversal == (int)PackageTypeCode.Regular || r.PackageTypeUniversal == (int)PackageTypeCode.Flat)
                         && ShippingServiceUtils.LocalTypesUniversal.Contains((ShippingTypeCode)r.ServiceTypeUniversal))
                    .ToList();
            
            //Calculate default package type
            SetDefaultRates(rates,
                true,
                true,
                true,
                serviceType);

            return rates;
        }


        public static IList<RateDTO> GetLocalRates(Func<PackageInfo, IList<RateDTO>> getRates,
            ILogService log,
            IWeightService weightService,
            Func<DateTime, DateTime, int> getBizDaysFunc,
            AddressDTO toAddress,
            ShipmentProviderType providerType,
            string shippingService,
            string initialShippingService,
            OrderTypeEnum orderType,
            DateTime? estShipDate,
            IList<OrderItemRateInfo> orderItems,
            ItemPackageDTO overridePackageSize)
       {
            var isSupportFlatEnvelope = ShippingServiceUtils.IsSupportFlatEnvelope(orderItems);
            var isSupportFedexEnvelope = ShippingServiceUtils.IsSupportFedexEnvelope(orderItems);
            var isSupportFedexPak = ShippingServiceUtils.IsSupportFedexPak(orderItems);
            var serviceType = ShippingServiceUtils.AmazonServiceToUniversalServiceType(shippingService);
            var initialServiceType = ShippingServiceUtils.AmazonServiceToUniversalServiceType(initialShippingService);

            var package = new PackageInfo()
            {
                Items = orderItems,
                PackageLength = overridePackageSize?.PackageLength,
                PackageWidth = overridePackageSize?.PackageWidth,
                PackageHeight = overridePackageSize?.PackageHeight
            };

            var retRates = getRates(package);

            //1. Get All Rates
            var rates = retRates.Where(
                    r => (r.PackageTypeUniversal == (int)PackageTypeCode.Regular 
                        || r.PackageTypeUniversal == (int)PackageTypeCode.Flat
                        || r.PackageTypeUniversal == (int)PackageTypeCode.LargeEnvelopeOrFlat)
                         && ShippingServiceUtils.LocalTypesUniversal.Contains((ShippingTypeCode)r.ServiceTypeUniversal))
                    .ToList();



            //if (orderType != OrderTypeEnum.Prime)
            //{
            //    //Exclude UPS
            //    rates = rates.Where(r => r.CurrierName != ShippingServiceUtils.UPSCarrier).ToList();
            //}

            var packagePriorityService = rates.FirstOrDefault(r => r.ServiceTypeUniversal == (int)ShippingTypeCode.Priority
                                                     && r.PackageTypeUniversal == (int)PackageTypeCode.Regular
                                                     && r.GroupId == 0);

            var hasRequiredField = true;

            if (weightService.AdjustWeight(orderItems.Sum(i => i.Weight * i.Quantity),
                orderItems.Sum(i => i.Quantity)) > ShippingUtils.STANDART_MAX_WEIGTH
                && ShippingUtils.IsServiceStandard(shippingService)) //NOTE: make 2xFirst Class only for standard
            {
                

                if (providerType != ShipmentProviderType.DhlECom) //NOTE: Dhlwhen weight > 25oz there is no rates
                    hasRequiredField = ValidateGetRateResult(rates, ShippingTypeCode.Priority); //NOTE: when overweight no FirstClass rate in list, validate with Priority service type

                //Set Visible Flat, Regular for overweigth
                var priorityRates = retRates.Where(r => (r.ServiceTypeUniversal == (int)ShippingTypeCode.Priority
                                                        || r.ServiceTypeUniversal == (int)ShippingTypeCode.PriorityExpress)
                                                        && (r.PackageTypeUniversal == (int)PackageTypeCode.Regular
                                                            || r.PackageTypeUniversal == (int)PackageTypeCode.Flat))
                    .Select(r => r).ToList();

                foreach (var rate in priorityRates)
                {
                    if (rate.PackageTypeUniversal == (int)PackageTypeCode.Flat)
                        rate.IsVisible = isSupportFlatEnvelope ? true : false;
                    if (rate.PackageTypeUniversal == (int)PackageTypeCode.Regular)
                        rate.IsVisible = true;
                }

                //NOTE: Need to check, before we use >16 oz weight, we haven't FirstClass
                //Wrong: Only if exist Standard rate in list (for some reasons Amazon not retrieve it)
                //if (retRates.Any(r => r.ServiceTypeUniversal == (int) ShippingTypeCode.Standard)) 
                //{
                //Calculate n x FirstClass rate
                var packages = RateHelper.GetGroupedStandardWithFlatRates(log,
                    weightService,
                    orderItems,
                    RateHelper.StandardPartialGroupId)
                    .ToList();

                try
                {
                    var groupedRates = CalculateRatesForGroup(packages, getRates);

                    if (groupedRates.Sum(s => s.Amount) <= priorityRates.Max(r => r.Amount))
                    //NOTE: check with priority rates
                    {
                        rates.AddRange(groupedRates);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("CalculateRatesForGroup, group=StandardPartialGroupId", ex);
                }
                //}
            }
            else
            {
                hasRequiredField = ValidateGetRateResult(rates, initialServiceType); //NOTE: otherwise validate with standard way
            }

            //NOTE: Exclude after validation!
            if (!ShippingUtils.IsServiceStandard(shippingService)) //Exclude standard for Expedited, Second Day service types
            {
                rates = rates.Where(s => s.ServiceTypeUniversal != (int)ShippingTypeCode.Standard).ToList();
            }

            if (!isSupportFlatEnvelope)
            {
                var flatPackages = RateHelper.GetGroupedFlatRates(log,
                    orderItems,
                    ShippingTypeCode.Priority,
                    PackageTypeCode.Flat,
                    ShippingServiceUtils.IsSupportFlatEnvelope,
                    RateHelper.FlatPartialGroupId).ToList();

                try
                {
                    var flatPriorityGroupedRates = CalculateRatesForGroup(flatPackages,
                        getRates);

                    if (packagePriorityService == null ||
                        flatPriorityGroupedRates.Sum(s => s.Amount) < packagePriorityService.Amount)
                    {
                        rates.AddRange(flatPriorityGroupedRates);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("CalculateRatesForGroup, group=FlatPartialGroupId", ex);
                }

                if (serviceType == ShippingTypeCode.PriorityExpress)
                {
                    var expressFlatPackages = RateHelper.GetGroupedFlatRates(log,
                        orderItems,
                        ShippingTypeCode.PriorityExpress,
                        PackageTypeCode.Flat,
                        ShippingServiceUtils.IsSupportFlatEnvelope,
                        RateHelper.ExpressFlatPartialGroupId).ToList();

                    try
                    {
                        var expressFlatPriorityGroupedRates = CalculateRatesForGroup(expressFlatPackages,
                            getRates);

                        if (packagePriorityService == null ||
                            expressFlatPriorityGroupedRates.Sum(s => s.Amount) < packagePriorityService.Amount)
                        {
                            rates.AddRange(expressFlatPriorityGroupedRates);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("CalculateRatesForGroup, group=ExpressFlatPartialGroupId", ex);
                    }
                }
            }


            //Calculate Service type
            if (ShippingUtils.IsServiceTwoDays(shippingService))
            {
                /*Сначала пробуем Priority если он дает <=2 дней берем его.
                Если Priority возвращает 3 дня, делаем Overnight (Priority Mail Express).
                Это всё касается только посылок внутри Америки.
                */

                /*Please change temporarily logic for 2nd business day orders, to be shipped Priority Express Flat, except:
                1.	If order from Florida (send it as before priority).
                2.	If order doesn’t fit Flat, and was supposed to be sent as Priority Express Package.
                */
                if (ShippingUtils.IsFloridaState(toAddress.State))
                {
                    var isPriority = retRates.Any(r => r.ServiceTypeUniversal == (int)ShippingTypeCode.Priority
                        && getBizDaysFunc(DateHelper.Max(r.ShipDate, estShipDate) ?? r.ShipDate, r.DeliveryDate) <= 2); // db.Dates.IsInTwoBizDays(r.ShipDate, r.DeliveryDate));
                    if (isPriority)
                    {
                        serviceType = ShippingTypeCode.Priority;
                    }
                    else
                    {
                        serviceType = ShippingTypeCode.PriorityExpress;
                    }
                }
                else
                {
                    serviceType = ShippingTypeCode.PriorityExpress;
                }
            }



            //Calculate default package type
            SetDefaultRates(rates,
                isSupportFlatEnvelope,
                isSupportFedexEnvelope,
                isSupportFedexPak,
                serviceType);

            if (!hasRequiredField
                && !(providerType == ShipmentProviderType.Amazon))
            {
                rates.ForEach(r =>
                {
                    r.IsVisible = true;
                    r.IsDefault = false;
                });
            }

            return rates;
        }
        
        private static IList<RateDTO> GetChipestVisibleRate(IList<RateDTO> rates,
            bool isSupportFlatEnvelop,
            ShippingTypeCode? minServiceType)
        {
            var defaultRates = rates.Where(r => r.IsDefault).ToList();
            if (defaultRates.Any())
                return defaultRates;

            List<RateDTO> rateInfoList = rates.Where(r => r.GroupId == 0 && r.IsVisible).ToList();
            rateInfoList.AddRange(rates.Where(r => r.GroupId != 0 && r.IsVisible)
                .GroupBy(r => r.GroupId, (key, group) => new RateDTO()
                {
                    GroupId = key,
                    PackageTypeUniversal = group.First().PackageTypeUniversal,
                    ServiceTypeUniversal = group.First().ServiceTypeUniversal,
                    Amount = group.Sum(r => r.Amount),
                })
                .ToList());

            if (minServiceType.HasValue)
            {
                rateInfoList = rateInfoList.Where(r => r.ServiceTypeUniversal >= (int)minServiceType.Value).ToList();
            }

            if (!isSupportFlatEnvelop)
            {
                rateInfoList = rateInfoList.Where(r => r.GroupId != 0 || r.PackageTypeUniversal != (int)PackageTypeCode.Flat).ToList();
            }

            rateInfoList = rateInfoList.OrderBy(r => r.Amount).ToList();


            RateDTO minRateInfo = null;
            foreach (var rate in rateInfoList)
            {
                if (minRateInfo == null
                    || rate.Amount < minRateInfo.Amount
                    //NOTE: Unless difference between Priority and multiple pakages <$0.6, always choose by default cheapest option:
                    || (rate.Amount < minRateInfo.Amount + 0.6M && rate.GroupId == 0 && minRateInfo.GroupId != 0))
                {
                    minRateInfo = rate;
                }
            }

            if (minRateInfo != null && minRateInfo.GroupId != 0)
                return rates.Where(r => r.GroupId == minRateInfo.GroupId).ToList();
            if (minRateInfo != null)
                return rates.Where(r => r.PackageTypeUniversal == minRateInfo.PackageTypeUniversal
                                        && r.ServiceTypeUniversal == minRateInfo.ServiceTypeUniversal
                                        && r.GroupId == 0).ToList();
            return null;
        }

        private static void SetDefaultRates(List<RateDTO> rates, 
            bool isSupportFlatEnvelope,
            bool isSupportFedexEnvelope,
            bool isSupportFedexPak,
            ShippingTypeCode? minServiceType)
        {
            List<RateDTO> rateInfoList = rates.Where(r => r.GroupId == 0).ToList();
            rateInfoList.AddRange(rates.Where(r => r.GroupId != 0)
                .GroupBy(r => r.GroupId, (key, group) => new RateDTO()
                {
                    GroupId = key,
                    PackageTypeUniversal = group.First().PackageTypeUniversal,
                    ServiceTypeUniversal = group.First().ServiceTypeUniversal,
                    Amount = group.Sum(r => r.Amount),
                })
                .ToList());

            if (minServiceType.HasValue)
            {
                rateInfoList = rateInfoList.Where(r => r.ServiceTypeUniversal >= (int)minServiceType.Value).ToList();
            }

            if (!isSupportFlatEnvelope)
            {
                rateInfoList = rateInfoList.Where(r => r.GroupId != 0 || r.PackageTypeUniversal != (int) PackageTypeCode.Flat).ToList();
            }
            if (!isSupportFedexEnvelope)
            {
                rateInfoList = rateInfoList.Where(r => !(r.CurrierName == "FEDEX" && StringHelper.ContainsNoCase(r.ServiceIdentifier, "Envelope"))).ToList();
            }
            if (!isSupportFedexPak)
            {
                rateInfoList = rateInfoList.Where(r => !(r.CurrierName == "FEDEX" && StringHelper.ContainsNoCase(r.ServiceIdentifier, "Pak"))).ToList();
            }

            rateInfoList = rateInfoList.OrderBy(r => r.Amount).ToList();
                        
            RateDTO minRateInfo = null;
            foreach (var rate in rateInfoList)
            {
                if (rate.ServiceIdentifier == "STAMPS_USPS_1_3")
                {
                    continue;
                }

                if (minRateInfo == null
                    || rate.Amount < minRateInfo.Amount
                    //NOTE: Unless difference between Priority and multiple pakages <$0.6, always choose by default cheapest option:
                    || (rate.Amount < minRateInfo.Amount + 0.6M && rate.GroupId == 0 && minRateInfo.GroupId != 0))
                {
                    minRateInfo = rate;
                }
            }

            //Set Default/Visible
            if (minRateInfo != null)
            {
                if (minRateInfo.GroupId != 0)
                {
                    foreach (var rate in rates.Where(r => r.GroupId == minRateInfo.GroupId))
                    {
                        rate.IsVisible = true;
                        rate.IsDefault = true;
                    }
                }
                else
                {
                    foreach (var rate in rates.Where(r => r.ServiceIdentifier == minRateInfo.ServiceIdentifier
                                                    && r.GroupId == 0))
                                        //;.ServiceTypeUniversal == (int)minRateInfo.ServiceTypeUniversal
                                        //             && r.PackageTypeUniversal == (int)minRateInfo.PackageTypeUniversal
                                        //             && r.GroupId == 0))
                    {
                        rate.IsVisible = true;
                        rate.IsDefault = true;
                    }
                }
            }

            //NOTE: Can happend when shipping service undefined
            if (rates.All(r => !r.IsVisible))
            {
                rates.ForEach(r => { r.IsVisible = true; });
            }

            if (rates.Count == 1)
            {
                rates[0].IsDefault = true;
                rates[0].IsVisible = true;
            }
        }

        public static List<RateDTO> CalculateRatesForGroup(IList<PackageInfo> packages,
            Func<PackageInfo, IList<RateDTO>> getRates)
        {
            var results = new List<RateDTO>();
            foreach (var package in packages)
            {
                var retRates = getRates(package);

                if (retRates.Count == 0)
                {
                    throw new Exception("Do not get the expected service/package type, serviceType=" + package.ServiceTypeUniversal + ", packageType=" + package.PackageTypeUniversal + ", groupId=" + package.GroupId);
                }

                var partRateDto = new RateDTO
                {
                    ServiceTypeUniversal = retRates[0].ServiceTypeUniversal,
                    PackageTypeUniversal = retRates[0].PackageTypeUniversal,

                    PackageLength = retRates[0].PackageLength,
                    PackageWidth = retRates[0].PackageWidth,
                    PackageHeight = retRates[0].PackageHeight,

                    ItemOrderIds = package.Items.Select(i => new RateItemDTO()
                    {
                        OrderItemId = i.ItemOrderId,
                        Quantity = i.Quantity
                    }).ToList(),

                    ServiceIdentifier = retRates[0].ServiceIdentifier,
                    ProviderType = retRates[0].ProviderType,
                    Amount = retRates[0].Amount,
                    ShipDate = retRates[0].ShipDate,
                    DeliveryDate = retRates[0].DeliveryDate,
                    DeliveryDaysInfo = retRates[0].DeliveryDaysInfo,
                    EarliestDeliveryDate = retRates[0].EarliestDeliveryDate,
                    
                    IsVisible = true,

                    GroupId = package.GroupId,
                };

                results.Insert(0, partRateDto);
            }

            return results;
        }
        
        private static List<PackageInfo> DivideToPackagesByWeight(IWeightService weightService, 
            IList<OrderItemRateInfo> items,
            double maxWeight,
            ShippingTypeCode serviceType,
            PackageTypeCode packageType,
            int groupId)
        {
            var weightPerItemList = GetItemPerQty(items).OrderByDescending(i => i.Weight).ToList();

            var packages = new List<PackageInfo>();
            
            while (weightPerItemList.Any(i => !i.InPackage))
            {
                var package = new PackageInfo()
                {
                    ServiceTypeUniversal = serviceType,
                    PackageTypeUniversal = packageType,
                    Items = new List<OrderItemRateInfo>(),
                    GroupId = groupId,
                };

                var currentItemIndex = 0;

                while (currentItemIndex < weightPerItemList.Count)
                {
                    if (!weightPerItemList[currentItemIndex].InPackage)
                    {
                        if (weightService.AdjustWeight(
                            (package.Items.Sum(i => i.Weight) + weightPerItemList[currentItemIndex].Weight),
                            (package.Items.Count + 1)) <= maxWeight)
                        {
                            AddItemToPackage(package, weightPerItemList[currentItemIndex], false);

                            weightPerItemList[currentItemIndex].InPackage = true;
                        }
                    }

                    currentItemIndex++;
                }

                packages.Add(package);
            }

            if (packages.Count <= 1)
                return new List<PackageInfo>();

            return packages;
        }

        private static List<PackageInfo> DivideByFlatPackages(IList<OrderItemRateInfo> items, 
            ShippingTypeCode serviceType,
            PackageTypeCode packageType,
            Func<IList<OrderItemRateInfo>, bool> fitChecker,
            int groupId)
        {
            var packages = new List<PackageInfo>();
            var currentPackage = new PackageInfo()
            {
                ServiceTypeUniversal = serviceType,
                PackageTypeUniversal = packageType,
                Items = new List<OrderItemRateInfo>(),
                GroupId = groupId,
            };

            foreach (var item in items)
            {
                if (!fitChecker(new List<OrderItemRateInfo>() {item})) 
                    return new List<PackageInfo>(); //NOTE: when any individual item not supported flat package
            }

            foreach (var item in items)
            {
                var packageItems = currentPackage.Items.ToList();
                packageItems.Add(item);
                
                if (fitChecker(packageItems)) //ShippingServiceUtils.IsSupportFlatEnvelop
                    //|| currentPackage.Items.Count == 0) //NOTE: Required for XL it is not support flat envelop (but as exclusion we create for them separate package)
                {
                    currentPackage.Items.Add(item);
                }
                else
                {
                    if (currentPackage.Items.Any())
                    {
                        packages.Add(currentPackage);
                        currentPackage = new PackageInfo()
                        {
                            ServiceTypeUniversal = serviceType,
                            PackageTypeUniversal = packageType,
                            Items = new List<OrderItemRateInfo>() {item},
                            GroupId = groupId
                        };
                    }
                    else
                    {
                        return new List<PackageInfo>(); //NOTE: we can't put item into empty package, ex.: XL item, not support multi Flat
                    }
                }
            }

            if (currentPackage.Items.Any())
            {
                packages.Add(currentPackage);
            }

            if (packages.Count <= 1)
                return new List<PackageInfo>();

            return packages;
        }

        private static IEnumerable<PackageInfo> GetGroupedRatesByWeight(IWeightService weightService, 
            IList<OrderItemRateInfo> orderItems,
            double maxWeight,
            ShippingTypeCode serviceType,
            PackageTypeCode packageType,
            int groupId)
        {
            var weightPerItemList = JoinCombinedItems(GetItemPerQty(orderItems)).OrderByDescending(i => i.Weight).ToList();

            var packages = DivideToPackagesByWeight(weightService,
                weightPerItemList,
                maxWeight,
                serviceType,
                packageType,
                groupId);

            packages.ForEach(p => GroupPackageItems(SplitCombinedItems(p)));

            return packages;
        }

        public static IList<PackageInfo> GetGroupedFlatRates(ILogService logService,
            IList<OrderItemRateInfo> orderItems,
            ShippingTypeCode serviceType,
            PackageTypeCode packageType,
            Func<IList<OrderItemRateInfo>, bool> fitChecker,
            int groupId)
        {
            var weightPerItemList = JoinCombinedItems(GetItemPerQty(orderItems)).OrderByDescending(i => i.Weight).ToList();

            var packages = DivideByFlatPackages(weightPerItemList, serviceType, packageType, fitChecker, groupId);

            packages.ForEach(p => GroupPackageItems(SplitCombinedItems(p)));

            return packages;
        }

        public static IList<PackageInfo> GetGroupedStandardWithFlatRates(ILogService logService,
            IWeightService weightService,
            IList<OrderItemRateInfo> orderItems,
            int groupId)
        {
            var preparedItemList = JoinCombinedItems(GetItemPerQty(orderItems)).OrderByDescending(i => i.Weight).ToList(); // 
            
            //Part I. Divide into flat packages with overweight
            var packages = new List<PackageInfo>();
            var bigPackage = new PackageInfo()
            {
                Items = new List<OrderItemRateInfo>(),
                GroupId = groupId,
            };

            foreach (var orderItem in preparedItemList)
            {
                //Step 1. Add overweight items into Flat package
                if (orderItem.Weight > ShippingUtils.STANDART_MAX_WEIGTH) //TODO: m.b. needs ShippingUtils.AdjustWeight
                {
                    var package = new PackageInfo()
                    {
                        Items = new List<OrderItemRateInfo>() { orderItem },
                        ServiceTypeUniversal = ShippingTypeCode.Priority,
                        PackageTypeUniversal = PackageTypeCode.Flat,
                        GroupId = groupId,
                    };
                    packages.Add(package);
                }
                else
                {
                    //Step 1.1. Try to add other items into exist packages
                    var isAdded = false;
                    foreach (var package in packages)
                    {
                        var packageItems = package.Items.ToList();
                        packageItems.Add(orderItem);
                        if (ShippingServiceUtils.IsSupportFlatEnvelope(packageItems))
                        {
                            AddItemToPackage(package, orderItem, false);
                            
                            isAdded = true;
                            break;
                        }
                    }

                    if (!isAdded)
                        AddItemToPackage(bigPackage, orderItem, false);
                }
            }


            //Step 2. Divide big package to standards / flat
            if (bigPackage.Items.Any())
            {
                if (weightService.AdjustWeight(bigPackage.Items.Sum(i => i.Weight * i.Quantity),
                    bigPackage.Items.Sum(i => i.Quantity)) < ShippingUtils.STANDART_MAX_WEIGTH)
                {
                    bigPackage.ServiceTypeUniversal = ShippingTypeCode.Standard;
                    bigPackage.PackageTypeUniversal = PackageTypeCode.Regular;
                    packages.Add(bigPackage);
                }
                else
                {
                    //Main rule: 1 Flat + Standard always better then 3 Standard
                    var standardPackages = DivideToPackagesByWeight(weightService,
                        bigPackage.Items, 
                        ShippingUtils.STANDART_MAX_WEIGTH,
                        ShippingTypeCode.Standard, 
                        PackageTypeCode.Regular,
                        groupId);

                    if (standardPackages.Count == 3) //TODO: universal
                    {
                        //Try to use 1 Flat
                        var flatPackage = new PackageInfo()
                        {
                            Items = new List<OrderItemRateInfo>(),
                            ServiceTypeUniversal = ShippingTypeCode.Priority,
                            PackageTypeUniversal = PackageTypeCode.Flat,
                            GroupId = groupId,
                        };
                        var standardPackage = new PackageInfo()
                        {
                            Items = new List<OrderItemRateInfo>(),
                            ServiceTypeUniversal = ShippingTypeCode.Standard,
                            PackageTypeUniversal = PackageTypeCode.Regular,
                            GroupId = groupId
                        };
                        var sortedItems =
                            bigPackage.Items.OrderBy(i => ShippingServiceUtils.GetShippingSizeIndex(i.ShippingSize))
                                .ToList();
                        foreach (var item in sortedItems)
                        {
                            var packageItems = flatPackage.Items.ToList();
                            packageItems.Add(item);
                            if (ShippingServiceUtils.IsSupportFlatEnvelope(packageItems))
                            {
                                AddItemToPackage(flatPackage, item, false);
                            }
                            else
                            {
                                AddItemToPackage(standardPackage, item, false);
                            }
                        }

                        if (standardPackage.Items.Any()
                            && flatPackage.Items.Any()
                            && weightService.AdjustWeight(standardPackage.Items.Sum(i => i.Weight * i.Quantity),
                            standardPackage.Items.Sum(i => i.Quantity)) < ShippingUtils.STANDART_MAX_WEIGTH)
                        {
                            packages.Add(flatPackage);
                            packages.Add(standardPackage);
                        }
                        else
                        {
                            packages.AddRange(standardPackages);
                        }
                    }
                    else
                    {
                        packages.AddRange(standardPackages);
                    }
                }
            }

            packages.ForEach(p => GroupPackageItems(SplitCombinedItems(p)));

            if (packages.Count <= 1) //NOTE: if only one package we haven't package group
                return new List<PackageInfo>();

            return packages;
        }

        private static IList<OrderItemRateInfo> JoinCombinedItems(IList<OrderItemRateInfo> items)
        {
            var groupedList = new List<OrderItemRateInfo>();
            foreach (var item in items)
            {
                var itemWasAdded = false;
                if (item.ReplaceType == (int) ItemReplaceTypes.Combined)
                {
                    var sourceItemId = OrderHelper.GetSourceOrderItemId(item.ItemOrderId);
                    var groupedItem = groupedList.FirstOrDefault(g => OrderHelper.GetSourceOrderItemId(g.ItemOrderId) == sourceItemId);
                    if (groupedItem != null)
                    {
                        groupedItem.LinkedOrderItems.Add(item);
                        groupedItem.Weight += item.Weight;
                        groupedItem.ShippingSize = ShippingServiceUtils.MaxShippingSize(groupedItem.ShippingSize, item.ShippingSize);
                        itemWasAdded = true;
                    }
                }

                if (!itemWasAdded)
                {
                    var groupedItem = item.Clone();
                    groupedItem.LinkedOrderItems = new List<OrderItemRateInfo>()
                    {
                        item
                    };
                    groupedList.Add(groupedItem);
                }
            }

            return groupedList;
        }

        private static PackageInfo SplitCombinedItems(PackageInfo package)
        {
            var newItems = package.Items.SelectMany(i => i.LinkedOrderItems).ToList();
            package.Items = newItems;
            return package;
        }

        public static void GroupPackageItems(PackageInfo package)
        {
            var items = GetItemPerQty(package.Items);
            package.Items = new List<OrderItemRateInfo>();
            foreach (var item in items)
                AddItemToPackage(package, item, true);
        } 

        private static IList<OrderItemRateInfo> GetItemPerQty(IList<OrderItemRateInfo> items)
        {
            var weightPerItemList = new List<OrderItemRateInfo>();
            foreach (var item in items)
            {
                for (var j = 0; j < item.Quantity; j++) //QuantityOrdered
                    weightPerItemList.Add(new OrderItemRateInfo(item.ItemOrderId, 
                        item.ReplaceType, 
                        item.Weight, 
                        item.PackageWidth,
                        item.PackageHeight,
                        item.PackageLength,
                        1, 
                        item.ItemPrice, 
                        item.ShippingSize, 
                        item.ItemStyle, 
                        item.LinkedOrderItems)); //item.Weight ?? 0
            }
            weightPerItemList = weightPerItemList.OrderByDescending(i => i.Weight).ToList();
            return weightPerItemList;
        } 

        private static void AddItemToPackage(PackageInfo package, OrderItemRateInfo orderItem, bool grouped)
        {
            if (grouped)
            {
                var existItem = package.Items.FirstOrDefault(i => i.ItemOrderId == orderItem.ItemOrderId);
                if (existItem != null)
                    existItem.Quantity += orderItem.Quantity;
                else
                    package.Items.Add(orderItem);
            }
            else
            {
                package.Items.Add(orderItem);
            }
        }




        public static IDictionary<string, decimal?> GetRatesByStyleItemId(IUnitOfWork db,
            long styleItemId)
        {
            var styleItem = db.StyleItems.GetAllAsDto().FirstOrDefault(si => si.StyleItemId == styleItemId);
            var styleCache = db.StyleCaches.GetAll().FirstOrDefault(sc => sc.Id == styleItem.StyleId);

            double? weight = styleItem != null ? styleItem.Weight : null;
            string shippingSize = styleCache != null ? styleCache.ShippingSizeValue : null;
            string internationalPackage = styleCache != null ? styleCache.InternationalPackageValue : null;

            var item = new OrderItemRateInfo()
            {
                Quantity = 1,
                ShippingSize = shippingSize
            };
            var localPackageType = PackageTypeCode.Flat;
            var internationalPackageType = PackageTypeCode.Regular;
            if (!String.IsNullOrEmpty(shippingSize))
            {
                localPackageType = ShippingServiceUtils.IsSupportFlatEnvelope(new List<OrderItemRateInfo>() { item })
                    ? PackageTypeCode.Flat
                    : PackageTypeCode.Regular;
                //internationalPackageType = ShippingServiceUtils.IsSupportLargeEnvelope(new List<OrderItemRateInfo>() { item })
                //        ? PackageTypeCode.LargeEnvelopeOrFlat
                //        : PackageTypeCode.Regular;
            }
            if (internationalPackage == "Regular")
                internationalPackageType = PackageTypeCode.Regular;

            IList<RateByCountryDTO> rates = null;
            if (weight.HasValue && weight > 0)
                rates = db.RateByCountries.GetAllAsDto().Where(r => r.Weight == Math.Floor(weight.Value < 1 ? 1.0 : weight.Value)).ToList();


            decimal? usRateActualCost = null;
            decimal? caRateActualCost = null;
            decimal? ukRateActualCost = null;
            decimal? auRateActualCost = null;
            if (rates != null && rates.Any())
            {
                usRateActualCost = rates.FirstOrDefault(r => r.Country == "US" && r.PackageType == localPackageType.ToString())?.Cost;
                caRateActualCost = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == internationalPackageType.ToString())?.Cost;
                ukRateActualCost = rates.FirstOrDefault(r => r.Country == "GB" && r.PackageType == internationalPackageType.ToString())?.Cost;
                auRateActualCost = rates.FirstOrDefault(r => r.Country == "AU" && r.PackageType == internationalPackageType.ToString())?.Cost;
            }

            return new Dictionary<string, decimal?>()
            {
                { MarketplaceKeeper.AmazonComMarketplaceId, usRateActualCost },
                { MarketplaceKeeper.AmazonCaMarketplaceId, caRateActualCost },
                { MarketplaceKeeper.AmazonUkMarketplaceId, ukRateActualCost },
                { MarketplaceKeeper.AmazonAuMarketplaceId, auRateActualCost }
            };
        }

        public static decimal? CalculateForMarket(MarketType market,
            string marketplaceId,
            decimal initSalePrice,
            decimal? usActualRateCost,
            decimal? caActualRateCost,
            decimal? ukActualRateCost,
            decimal? auActualRateCost,
            decimal usShipping,
            decimal marketShippingAmount,
            decimal marketExtra)
        {
            //var caShipping = 9.49M;
            //var wmCAShipping = 4.99M;
            //var ukShipping = 9.49M;
            //var auShipping = 9.49M;
            //var caExtra = 1.00M;
            //var ukExtra = 1.00M;
            //var auExtra = 1.00M;

            var correction = PriceHelper.GetSalePriceCorrectionByMarket(market);

            if (marketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
            {
                return initSalePrice + correction;
            }

            if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId
                && usActualRateCost.HasValue && caActualRateCost.HasValue)
            {
                var usIncome = initSalePrice + usShipping - usActualRateCost.Value;
                return PriceHelper.RoundTo99(PriceHelper.Convert(usIncome + caActualRateCost.Value + marketExtra, PriceHelper.CADtoUSD, false) - marketShippingAmount) + correction;
            }

            if (market == MarketType.WalmartCA
                && usActualRateCost.HasValue && caActualRateCost.HasValue)
            {
                var usIncome = initSalePrice + usShipping - usActualRateCost.Value;
                return PriceHelper.RoundTo99(PriceHelper.Convert(usIncome + caActualRateCost.Value + marketExtra, PriceHelper.CADtoUSD, false) - marketShippingAmount) + correction;
            }

            if (market == MarketType.AmazonEU
                && usActualRateCost.HasValue && ukActualRateCost.HasValue)
            {
                var usIncome = initSalePrice + usShipping - usActualRateCost.Value;
                //NOTE: Min UK price is 3.99
                return PriceHelper.RoundTo99(Math.Max(3.99M, PriceHelper.Convert(usIncome + ukActualRateCost.Value + marketExtra, PriceHelper.GBPtoUSD, false) - marketShippingAmount));
            }

            if (market == MarketType.AmazonAU
                && usActualRateCost.HasValue && auActualRateCost.HasValue)
            {
                var usIncome = initSalePrice + usShipping - usActualRateCost.Value;
                //NOTE: Min UK price is 3.99
                return PriceHelper.RoundTo99(Math.Max(3.99M, PriceHelper.Convert(usIncome + auActualRateCost.Value + marketExtra, PriceHelper.AUDtoUSD, false) - marketShippingAmount));
            }

            if (market == MarketType.Walmart)
            {
                return initSalePrice + correction;
            }

            if (market == MarketType.Jet)
            {
                return initSalePrice + correction;
            }

            if (market == MarketType.eBay)
            {
                return initSalePrice + usActualRateCost //NOTE: include US shipping cost, for enable: "FREE SHIPPING"
                    + marketExtra + correction;
            }

            if (market == MarketType.Shopify
                || market == MarketType.Magento
                || market == MarketType.Groupon
                || market == MarketType.DropShipper
                || market == MarketType.CustomAshford
                || market == MarketType.OfflineOrders
                || market == MarketType.WooCommerce)
            {
                return initSalePrice + correction;
            }

            return null;
        }
    }
}
