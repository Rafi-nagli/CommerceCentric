using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;

namespace Amazon.Common.Helpers
{
    public static class ShippingUtils
    {
        public static string FormattedToMarketShippingService(string shippingMethod, bool isIntl)
        {
            if (shippingMethod == "E-Packet")
            {
                if (isIntl)
                    return "International Regular";
                return "Regular";
            }

            return shippingMethod;
        }

        public static string FormattedToMarketCurrierName(string carrierName, bool isIntl, MarketType market)
        {
            //Walmart: The package shipment carrier. Valid entries are: UPS, USPS, FedEx, Airborne, OnTrac, DHL, LS, UDS, UPSMI, FDX, PILOT, ESTES, SAIA.
             
            if (StringHelper.IsEqualNoCase(carrierName, ShippingServiceUtils.SkyPostalCarrier))
            {
                if (market == MarketType.Walmart || market == MarketType.WalmartCA)
                    return "CPC";//return "CanadaPost";
                return "Canada Post";
            }
            if (StringHelper.IsEqualNoCase(carrierName, ShippingServiceUtils.IBCCarrier))
            {
                return "USPS";
            }

            if (StringHelper.IsEqualNoCase(carrierName, ShippingServiceUtils.DHLEComCarrier))
            {
                if (market == MarketType.Walmart)
                    return "USPS";
                if (market == MarketType.WalmartCA)
                    return "USPS";

                return "DHL eCommerce";

                //if (market == MarketType.eBay)
                //    return "DHLGlobalMail";

                //return "DHL Global Mail"; //NOTE: For Amazon
            }
            if (StringHelper.IsEqualNoCase(carrierName, ShippingServiceUtils.FedexCarrier))
            {
                return "FedEx";
            }

            return carrierName;
        }

        public static string CombineZip(string zip, string zipAddon)
        {
            if (String.IsNullOrEmpty(zipAddon))
                return zip;
            return zip + "-" + zipAddon;
        }

        public static int? ZipToInt5(string zip)
        {
            if (String.IsNullOrEmpty(zip))
                return null;

            var zip5 = zip.Replace(" ", "").Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            zip5 = zip5.Length > 5 ? zip5.Substring(0, 5) : zip5;
            int intZip;
            if (Int32.TryParse(zip5, out intZip))
                return intZip;
            return null;
        }

        public static string GetZip(string zip)
        {
            if (!string.IsNullOrEmpty(zip))
            {
                var l = zip.IndexOf('-');
                return l > 0
                    ? zip.Substring(0, l).Trim().Trim('-')
                    : zip.Trim().Trim('-');
            }
            return zip;
        }

        public static string GetZipAddon(string zip)
        {
            if (!string.IsNullOrEmpty(zip))
            {
                var i = zip.IndexOf('-');
                if (i > 0 && i + 1 <= zip.Length)
                {
                    return zip.Substring(i + 1).Trim().Trim('-');
                }
            }
            return String.Empty;
        }

        public static void FixUpItemPrices(IList<DTOOrderItem> items)
        {
            var totalPrice = items.Sum(i => i.ItemPrice * i.Quantity);
            var totalQuantity = items.Sum(i => i.Quantity);
            //TASK: max $18 + $6 x item (for each item more then 3)
            //TASK: #2 taxes when merchandise is over 20 CAD. Right now we make sure it’s less 20 USD.
            //We need to change it.Assume that 1US = 1.4, so max value on invoice should be 14.3 USD.

            var random = new Random(DateTime.Now.Millisecond);
            var maxBasePrice = PriceHelper.RoundToTwoPrecision(14.3M - (decimal)random.NextDouble() / 3M);
            var maxTotalPrice = maxBasePrice + Math.Max(0, totalQuantity - 3) * 6;
            if (totalPrice > maxTotalPrice)
            {
                decimal priceCorrection = maxTotalPrice / totalPrice;
                foreach (var item in items)
                {
                    item.ItemPrice = PriceHelper.RoundToTwoPrecision(priceCorrection * item.ItemPrice);
                }
            }
        }

        public static string GetService(string shipmentService, string country)
        {
            return
                country != null
                    ? !IsInternational(country) // country == "US"
                        ? shipmentService
                        : "i:" + shipmentService
                    : shipmentService;
        }

        public static string GetServiceToPrint(string service)
        {
            return IsServiceStandard(service)
                ? "Standard"
                : service;
        }

        public static string GetPackageTypeToPrint(string type)
        {
            return type.ToLower().Contains("flat")
                ? "Flat"
                : "Package";
        }


        public static bool IsServiceSameDay(string service)
        {
            if (String.IsNullOrEmpty(service))
                return false;
            return (service.ToLower().Contains("same") && service.ToLower().Contains("day"));
        }

        public static bool IsServiceTwoDays(string service)
        {
            if (String.IsNullOrEmpty(service))
                return false;
            //Second Day || Two-Day
            return (service.ToLower().Contains("two") || service.ToLower().Contains("second")) && service.ToLower().Contains("day");
        }

        public static bool IsServiceNextDay(string service)
        {
            //Next Day
            if (String.IsNullOrEmpty(service))
                return false;
            return service.ToLower().Contains("next") && service.ToLower().Contains("day");
        }

        public static bool IsServiceStandard(string service)
        {
            return (service ?? "").ToLower().Contains("standard") || (service ?? "").ToLower().Contains("first");
        }

        public static bool IsServiceExpedited(string service)
        {
            return (service ?? "").ToLower().Contains("expedited");
        }

        public static bool IsPackageType(string type)
        {
            return type.ToLower().Contains("package");
        }

        public static bool IsOrderToUpdate(string status)
        {
            return status.ToLower() != "canceled";
        }

        public static string SameDayServiceName = "SameDay";
        public static string NextDayServiceName = "NextDay";
        public static string SecondDayServiceName = "SecondDay";
        public static string ExpeditedServiceName = "Expedited";
        public static string StandardServiceName = "Standard";

        public static string FreeEconomyServiceType = "FreeEconomy";

        public static string CorrectInitialShippingService(string initialServiceType,
            string sourceServiceType,
            OrderTypeEnum orderType)
        {
            if (orderType == OrderTypeEnum.Prime)
            {
                //UPDATED: 04/21/2020 убирай
                ////NOTE: "If order has a prime item, like 106-5615239-3266649 it should definitely be considered as Priority, and by default send using Priority Flat or above (no first class)."
                //if (initialServiceType == StandardServiceName)
                //{
                //    //NOTE: FreeEconomy = ne, eto =Standard
                //    if (sourceServiceType != FreeEconomyServiceType)
                //        return ExpeditedServiceName;
                //}
                if (initialServiceType == SecondDayServiceName || initialServiceType == NextDayServiceName)
                    return ExpeditedServiceName;
            }
            return initialServiceType;
        }

        public static string InitialShippingServiceIncludeUpgrade(string initialServiceType, int? upgradeLevel)
        {
            /*
             * Source List:

                Expedited
                FreeEconomy
                NextDay
                SameDay
                SecondDay
                Scheduled
                Standard
             */

            //Upgrade only for Standard:
            //Standard->Expedited


            if (upgradeLevel == null || upgradeLevel == 0)
                return initialServiceType;

            if (upgradeLevel == 1)
            {
                if (initialServiceType.ToLower() == StandardServiceName.ToLower())
                {
                    return ExpeditedServiceName;
                }

                if (initialServiceType.ToLower() == ExpeditedServiceName.ToLower())
                {
                    return SecondDayServiceName;
                }
            }
            if (upgradeLevel == 2)
            {
                if (initialServiceType.ToLower() == StandardServiceName.ToLower()
                    || initialServiceType.ToLower() == ExpeditedServiceName.ToLower())
                    return SecondDayServiceName;
            }

            return initialServiceType;
        }

        public static bool IsOrderCanceled(string status)
        {
            return status?.ToLower() == "canceled";
        }

        public static bool IsOrderShipped(string status)
        {
            return status?.ToLower() == "shipped";
        }

        public static bool IsOrderUnshipped(string status)
        {
            return status?.ToLower() == "unshipped";
        }

        public static bool IsOrderPartial(string status)
        {
            return status?.ToLower() == "partiallyshipped";
        }

        public static bool IsOrderPending(string status)
        {
            return status?.ToLower() == "pending";
        }

        public static bool IsInternational(string country)
        {
            //http://en.wikipedia.org/wiki/List_of_U.S._state_abbreviations

            if (String.IsNullOrEmpty(country))
                return false;

            country = country.ToUpper();

            return country != "US"

                && country != "PR"
                && country != "GU"
                && country != "VI"
                && country != "UM"
                && country != "MP"
                && country != "AS"

                && country != "MH"
                && country != "PW"
                && country != "FM";
        }

        static public string CorrectingCountryCode(string countryCode)
        {
            if (countryCode == "PR"
                || countryCode == "GU"
                || countryCode == "VI"
                || countryCode == "UM"
                || countryCode == "MP"
                || countryCode == "AS"

                || countryCode == "MH"
                || countryCode == "PW"
                || countryCode == "FM")
                return "US";
            return countryCode;
        }

        static public string CorrectingStateCode(string state, string countryCode)
        {
            if (countryCode == "PR")
                return "PR";
            if (countryCode == "GU")
                return "GU";
            if (countryCode == "VI")
                return "VI";
            if (countryCode == "UM")
                return "UM";
            if (countryCode == "MP")
                return "MP";
            if (countryCode == "AS")
                return "AS";

            if (countryCode == "MH")
                return "MH";
            if (countryCode == "PW")
                return "PW";
            if (countryCode == "FM")
                return "FM";

            return state;
        }

        public static bool ShouldCorrectState(string state, string country)
        {
            if (string.IsNullOrEmpty(state))
                return false;

            return state.Length > 2 && !IsInternational(country);
        }

        public static bool IsMexico(string country)
        {
            if (String.IsNullOrEmpty(country))
                return false;

            return String.Compare(country, "MX", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "Mexico", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsSpain(string country)
        {
            if (String.IsNullOrEmpty(country))
                return false;

            return String.Compare(country, "ES", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "Spain", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsCanada(string country)
        {
            if (String.IsNullOrEmpty(country))
                return false;

            return String.Compare(country, "CA", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "Canada", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsSkyPostalSupportedCountry(string country)
        {
            if (String.IsNullOrEmpty(country))
                return false;

            return IsCanada(country) || country == "BR" || country == "CL" || country == "CO" || country == "MX";
        }

        public static bool IsLatvia(string country)
        {
            if (String.IsNullOrEmpty(country))
                return false;

            return String.Compare(country, "LV", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "Latvia", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsUK(string country)
        {
            if (String.IsNullOrEmpty(country))
                return false;

            return String.Compare(country, "UK", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "GB", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "United Kingdom", StringComparison.InvariantCultureIgnoreCase) == 0
                || String.Compare(country, "Great Britain", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsFloridaState(string state)
        {
            if (String.IsNullOrEmpty(state))
                return false;

            if (state.Length > 2)
                return String.Compare(state, "Florida", StringComparison.InvariantCultureIgnoreCase) == 0;
            return String.Compare(state, "FL", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsInternationalState(string state)
        {
            if (state == "PR"
                || state == "GU"
                || state == "MP"
                || state == "AS"
                || state == "VI"
                || state == "UM"

                || state == "AA"
                || state == "AE"
                || state == "AP"

                || state == "MH"
                || state == "FM"
                || state == "PW")
                return true;
            return false;
        }

        public static decimal GetItemPrice(ListingOrderDTO listing)
        {
            var price = listing.ItemPrice;
            if (listing.QuantityOrdered != 0)
                price += listing.ShippingPrice / listing.QuantityOrdered;
            return price;
        }



        public static double STANDART_MAX_WEIGTH = 16;// 15.999f;
        public static double ISTANDART_MAX_WEIGTH = 64;// 4lb;


        public static decimal? GetRougePaidUSShippingAmount(string shippingService, int quantityOrdered)
        {
            //NOTE: using NY state
            if (shippingService == StandardServiceName)
            {
                return 4.49M + (quantityOrdered - 1) * 1M; //NOTE: $1 per each additional item
            }
            if (shippingService == ExpeditedServiceName)
            {
                return 7.49M + (quantityOrdered - 1) * 1M; //NOTE: $1 per each additional item
            }
            if (shippingService == SecondDayServiceName || shippingService == NextDayServiceName)
            {
                return 24.49M + (quantityOrdered - 1) * 1M; //NOTE: $1 per each additional item
            }
            return null;
        }

        /*
         First Class
Priority Flat
Priority Regular
Priority Express Flat
Priority Express Regular
International Flat
International Regular
         */

        public static string MethodNameToAmazonService(string name)
        {
            var isInternational = (name ?? "").IndexOf("international", StringComparison.OrdinalIgnoreCase) >= 0
                || (name ?? "").IndexOf("DHL", StringComparison.OrdinalIgnoreCase) >= 0;
            var iPrefix = isInternational ? "i:" : "";

            if (string.IsNullOrEmpty(name))
            {
                return iPrefix + StandardServiceName;
            }

            name = name.ToLower();
            if (name.Contains("same") && name.Contains("day")) //Amazon: SameDay
            {
                return iPrefix + SameDayServiceName;
            }
            if ((name.Contains("express") && name.Contains("worldwide"))
                || name.IndexOf("dhl", StringComparison.OrdinalIgnoreCase) >= 0) //Dhl: Express WorldWide OR DHL MX
            {
                return iPrefix + ExpeditedServiceName;
            }
            if (name.Contains("express"))
            {
                return iPrefix + SecondDayServiceName; //the "NextDay" also suitable;
            }
            if ((name.Contains("next") && name.Contains("day"))
                || name.Contains("overnight"))
            {
                return iPrefix + NextDayServiceName; 
            }

            if (name.Contains("ups")
                && name.Contains("ground")
                && name.Contains("Regional Rate Box")) //Regional Rate Box A
            {
                return iPrefix + ExpeditedServiceName;
            }

            if (name.Contains("priority"))
            {
                return iPrefix + ExpeditedServiceName;
            }

            return iPrefix + StandardServiceName;
        }

        public static string PrepareMethodNameToDisplay(string methodName, string deliveryDaysInfo)
        {
            if (String.IsNullOrEmpty(methodName))
                return methodName;

            var name = methodName.Replace("International ", "Intl ");//.Replace("Priority ", "");
            if (name == "Flat" && (deliveryDaysInfo ?? "").Contains("3"))
                name += "(" + deliveryDaysInfo + " days)";

            return name;
        }

        public static string PrepareStampsMethodNameToDisplay(string methodName, string deliveryDaysInfo)
        {
            if (String.IsNullOrEmpty(methodName))
                return methodName;

            var name = methodName.Replace("International ", "Intl ");//.Replace("Priority ", "");
            if (name == "Flat" && (deliveryDaysInfo ?? "").Contains("3"))
                name += "(" + deliveryDaysInfo + " days)";

            if (StringHelper.ContainsNoCase(name, "2 days"))
                return "Priority";

            if (name == "Priority Regular")
                return "Priority";


            return name;
        }


        public const int FirstClassShippingMethodId = 1;
        public const int PriorityFlatShippingMethodId = 2;
        public const int PriorityReqularShippingMethodId = 3;
        public const int ExpressFlatShippingMethodId = 4;
        public const int ExpressReqularShippingMethodId = 5;
        public const int InternationalFlatShippingMethodId = 6;
        public const int InternationalRegularShippingMethodId = 7;
        public const int InternationalPriorityFlatShippingMethodId = 8;
        public const int InternationalPriorityRegularShippingMethodId = 11; //NOTE: Everything correct, gap between Ids
        public const int InternationalExpressFlatShippingMethodId = 13;
        public const int InternationalExpressRegularShippingMethodId = 14;

        public const int AmazonFirstClassShippingMethodId = 15;
        public const int AmazonPriorityFlatShippingMethodId = 16;
        public const int AmazonPriorityRegularShippingMethodId = 17;
        public const int AmazonExpressFlatShippingMethodId = 18;

        public const int DhlExpressWorldWideShippingMethodId = 19;

        public const int AmazonExpressRegularShippingMethodId = 20;

        public const int DynamexPTPSameShippingMethodId = 21;

        public const int AmazonDhlExpressMXShippingMethodId = 22;

        public const int AmazonUPSGroundShippingMethodId = 23;

        public const int DhlEComSMParcelGroundShippingMethodId = 24;
        public const int DhlEComSMParcelExpeditedShippingMethodId = 25;
        public const int DhlEComSMParcelPlusGroundShippingMethodId = 26;
        public const int DhlEComParcelInternationalDirectShippingMethodId = 27;
        public const int DhlEComGMPacketPlusShippingMethodId = 28;

        public const int AmazonRegionalRateBoxAMethodId = 29;
        public const int RegionalRateBoxAMethodId = 30;

        public const int IBCCEPePocketMethodId = 31;
        public const int IBCIPAMethodId = 32;

        public const int FedexOneRate2DayEnvelope = 41;
        public const int FedexOneRate2DayPak = 42;

        public const int FedexPriorityOvernightEnvelope = 44;
        public const int FedexPriorityOvernightPak = 45;

        public const int AmazonFedExHomeDeliveryShippingMethodId = 33;
        public const int AmazonFedExExpressSaverShippingMethodId = 34;

        public const int AmazonFedExStandardOvernight = 38;
        public const int AmazonFedExPriorityOvernight = 39;

        public const int FedexSmartPost = 40;

        public const int AmazonFedEx2DayShppingMethodId = 46;
        public const int AmazonFedEx2DayAMShppingMethodId = 47;

        public const int Fedex2DayShppingMethodId = 50;
        public const int Fedex2DayAMShppingMethodId = 51;

        public const int FedexIntlPriority = 53;
        public const int FedexIntlEconomy = 54;

        public const int AmazonFedEx2DayOneRateEnvelopeShippingMethodId = 56;
        public const int AmazonFedEx2DayOneRatePakShippingMethodId = 57;

        public const int FedexGroundShippingMethodId = 48;
        public const int FedexHomeDeliveryShippingMethodId = 52;

        public const int AmazonUPS2ndDayAirShippingMethodId = 35;

        public static int ConvertToMainProviderType(int providerType)
        {
            if (providerType == (int)ShipmentProviderType.StampsPriority)
                return (int)ShipmentProviderType.Stamps;
            return providerType;
        }

        public static int? ConvertAmazonToStampsShippingMethod(int shippingMethodId)
        {
            if (shippingMethodId == AmazonFirstClassShippingMethodId)
                return FirstClassShippingMethodId;
            if (shippingMethodId == AmazonPriorityFlatShippingMethodId)
                return PriorityFlatShippingMethodId;
            if (shippingMethodId == AmazonPriorityRegularShippingMethodId)
                return PriorityReqularShippingMethodId;
            if (shippingMethodId == AmazonExpressFlatShippingMethodId)
                return ExpressFlatShippingMethodId;
            if (shippingMethodId == AmazonExpressRegularShippingMethodId)
                return ExpressReqularShippingMethodId;

            return null;
        }

        public static int[] GetShippingMethodAnalogs(int shippingMethodId)
        {
            if (shippingMethodId == FirstClassShippingMethodId
                || shippingMethodId == AmazonFirstClassShippingMethodId
                || shippingMethodId == DhlEComSMParcelGroundShippingMethodId
                || shippingMethodId == DhlEComSMParcelExpeditedShippingMethodId)
                return new[] { FirstClassShippingMethodId, AmazonFirstClassShippingMethodId, DhlEComSMParcelGroundShippingMethodId, DhlEComSMParcelExpeditedShippingMethodId };
            if (shippingMethodId == PriorityFlatShippingMethodId
                || shippingMethodId == AmazonPriorityFlatShippingMethodId)
                return new[] { PriorityFlatShippingMethodId, AmazonPriorityFlatShippingMethodId };
            if (shippingMethodId == PriorityReqularShippingMethodId
                || shippingMethodId == AmazonPriorityRegularShippingMethodId
                || shippingMethodId == DhlEComGMPacketPlusShippingMethodId)
                return new[] { PriorityReqularShippingMethodId, AmazonPriorityRegularShippingMethodId, DhlEComGMPacketPlusShippingMethodId };
            if (shippingMethodId == ExpressFlatShippingMethodId
                || shippingMethodId == AmazonExpressFlatShippingMethodId)
                return new[] { ExpressFlatShippingMethodId, AmazonExpressFlatShippingMethodId };
            if (shippingMethodId == ExpressReqularShippingMethodId
                || shippingMethodId == AmazonExpressRegularShippingMethodId)
                return new[] { ExpressReqularShippingMethodId, AmazonExpressRegularShippingMethodId };
            if (shippingMethodId == DhlExpressWorldWideShippingMethodId
                || shippingMethodId == AmazonDhlExpressMXShippingMethodId)
                return new[] { DhlExpressWorldWideShippingMethodId, AmazonDhlExpressMXShippingMethodId };
            return new[] { shippingMethodId };
        }

        //public static bool HasPickup(int? shippingMethodId)
        //{
        //    if (!shippingMethodId.HasValue)
        //        return false;

        //    if (shippingMethodId == DhlExpressWorldWideShippingMethodId
        //        || shippingMethodId == AmazonDhlExpressMXShippingMethodId

        //        || shippingMethodId == DhlEComGMPacketPlusShippingMethodId
        //        || shippingMethodId == DhlEComParcelInternationalDirectShippingMethodId
        //        || shippingMethodId == DhlEComSMParcelExpeditedShippingMethodId
        //        || shippingMethodId == DhlEComSMParcelGroundShippingMethodId
        //        || shippingMethodId == DhlEComSMParcelPlusGroundShippingMethodId

        //        || shippingMethodId == DynamexPTPSameShippingMethodId

        //        || shippingMethodId == AmazonUPSGroundShippingMethodId
        //        || shippingMethodId == AmazonUPS2ndDayAirShippingMethodId

        //        || shippingMethodId == IBCCEPePocketMethodId
        //        || shippingMethodId == IBCIPAMethodId)
        //        return true;

        //    return false;
        //}



        public static bool IsMethodNonStandard(int? shippingMethodId)
        {
            if (shippingMethodId == null)
            {
                return false;
            }
            return !IsMethodStandard(shippingMethodId);
        }

        public static bool IsMethodStandard(int? shippingMethodId)
        {
            if (shippingMethodId == null)
            {
                return false;
            }
            return shippingMethodId == FirstClassShippingMethodId //First Class
                   || shippingMethodId == AmazonFirstClassShippingMethodId
                   || shippingMethodId == DhlEComSMParcelGroundShippingMethodId
                   || shippingMethodId == DhlEComSMParcelExpeditedShippingMethodId
                   || shippingMethodId == InternationalFlatShippingMethodId //International Flat
                   || shippingMethodId == InternationalRegularShippingMethodId; //International Regular
        }

        public static bool IsFlat(int? shippingMethodId)
        {
            if (shippingMethodId == null)
            {
                return false;
            }

            if (shippingMethodId == PriorityFlatShippingMethodId
                || shippingMethodId == ExpressFlatShippingMethodId

                || shippingMethodId == InternationalFlatShippingMethodId
                || shippingMethodId == InternationalPriorityFlatShippingMethodId
                || shippingMethodId == InternationalExpressFlatShippingMethodId

                || shippingMethodId == AmazonPriorityFlatShippingMethodId
                || shippingMethodId == AmazonExpressFlatShippingMethodId)
                return true;
            return false;
        }

        public static string GetPackageName(PackageTypeCode packageType)
        {
            switch (packageType)
            {
                case PackageTypeCode.Regular:
                    return "Regular";
                case PackageTypeCode.LargeEnvelopeOrFlat:
                case PackageTypeCode.Flat:
                    return "Flat";
                case PackageTypeCode.RegionalRateBoxA:
                    return "RateBoxA";
            }
            return String.Empty;
        }

        public static PackageTypeCode GetPackageType(int shippingMethodId)
        {
            switch (shippingMethodId)
            {
                case PriorityFlatShippingMethodId:
                case ExpressFlatShippingMethodId:
                case InternationalPriorityFlatShippingMethodId:
                case InternationalExpressFlatShippingMethodId: //??? Not 100% shure
                case AmazonPriorityFlatShippingMethodId:
                case AmazonExpressFlatShippingMethodId:
                    return PackageTypeCode.Flat;

                case InternationalFlatShippingMethodId:
                    return PackageTypeCode.LargeEnvelopeOrFlat;

                case AmazonRegionalRateBoxAMethodId:
                case RegionalRateBoxAMethodId:
                    return PackageTypeCode.RegionalRateBoxA;

                default:
                    return PackageTypeCode.Regular;
            }
        }


        public static ShippingTypeCode GetShippingType(int shippingMethodId)
        {
            switch (shippingMethodId)
            {
                case FirstClassShippingMethodId:
                case AmazonFirstClassShippingMethodId:
                case DhlEComSMParcelGroundShippingMethodId:
                case DhlEComSMParcelExpeditedShippingMethodId:
                    return ShippingTypeCode.Standard;

                case InternationalFlatShippingMethodId:
                case InternationalRegularShippingMethodId:
                    return ShippingTypeCode.IStandard;

                case PriorityFlatShippingMethodId:
                case PriorityReqularShippingMethodId:
                case AmazonPriorityFlatShippingMethodId:
                case AmazonPriorityRegularShippingMethodId:
                case AmazonUPSGroundShippingMethodId:
                case DhlEComSMParcelPlusGroundShippingMethodId:
                case AmazonRegionalRateBoxAMethodId:
                case RegionalRateBoxAMethodId:
                case FedexOneRate2DayEnvelope:
                case FedexOneRate2DayPak:
                    return ShippingTypeCode.Priority;

                case ExpressFlatShippingMethodId:
                case ExpressReqularShippingMethodId:
                case AmazonExpressFlatShippingMethodId:
                case AmazonExpressRegularShippingMethodId:
                    return ShippingTypeCode.PriorityExpress;

                case DynamexPTPSameShippingMethodId:
                    return ShippingTypeCode.SameDay;

                case InternationalPriorityFlatShippingMethodId:
                case InternationalPriorityRegularShippingMethodId:
                case DhlExpressWorldWideShippingMethodId:
                case AmazonDhlExpressMXShippingMethodId:
                case DhlEComParcelInternationalDirectShippingMethodId:
                case DhlEComGMPacketPlusShippingMethodId:
                    return ShippingTypeCode.IPriority;

                case InternationalExpressFlatShippingMethodId:
                case InternationalExpressRegularShippingMethodId:
                    return ShippingTypeCode.IPriorityExpress;

                default:
                    return ShippingTypeCode.Standard;
            }
        }

        public static bool IsInternationalShippingType(int shippingMethodId)
        {
            switch (shippingMethodId)
            {
                case InternationalFlatShippingMethodId:
                case InternationalRegularShippingMethodId:
                case InternationalPriorityFlatShippingMethodId:
                case InternationalPriorityRegularShippingMethodId:
                case InternationalExpressFlatShippingMethodId:
                case InternationalExpressRegularShippingMethodId:
                case DhlExpressWorldWideShippingMethodId:
                case AmazonDhlExpressMXShippingMethodId:
                    return true;
            }
            return false;
        }

        public static string GetInsuranceInfo(decimal insuranceCost, string currency)
        {
            return (insuranceCost > 0)
                ? " - ins " + currency + insuranceCost
                : "";
        }

        public static string GetSignInfo(decimal signCost, string currency)
        {
            return (signCost > 0)
                ? " - sign " + currency + signCost
                : "";
        }

        public static DateTime? AlignMarketDateByEstDayEnd(DateTime? date, MarketType market)
        {
            if (!date.HasValue)
                return date;

            if (market == MarketType.Amazon)
            {
                return date.Value.AddHours(-3); //Amazon US/CA return data in PST, after convert to EST we have +3 hours (+1 day)
            }
            if (market == MarketType.AmazonEU)
            {
                return date.Value.AddHours(5); //Amazon UK return date in GMT, after convert to EST we have -5 hours
            }
            if (market == MarketType.AmazonAU)
            {
                return date.Value.AddHours(-10); //Amazon AU return date in GMT, after convert to EST we have +10 hours
            }
            if (market == MarketType.Walmart)
            {
                return date.Value.AddHours(-2).AddMinutes(-1); //Malmart date show as 2:00 insted of 11:59
            }
            if (market == MarketType.WalmartCA)
            {
                return date.Value.AddHours(-2).AddMinutes(-1); //Malmart date show as 2:00 insted of 11:59
            }
            if (market == MarketType.Jet)
            {
                return date.Value;
            }
            return date;
        }

        public static string BuildLabelNotes(OrderShippingInfoDTO shipping)
        {
            var notes = OrderHelper.FormatOrderNumber(shipping.OrderAmazonId, (MarketType)shipping.Market) + " > " +
                        (shipping.TotalQuantity > 1 ? shipping.TotalQuantity + " items" : shipping.TotalQuantity + " item") +
                        (ShippingUtils.IsFlat(shipping.ShippingMethod.Id) ? " Flat" : "");

            if (shipping.NumberInList > 0)
                notes = shipping.NumberInList + " > " + notes;

            if (shipping.BatchId.HasValue && shipping.BatchId > 0)
                notes = "B" + shipping.BatchId + " > " + notes;

            return notes;
        }

        public static int GetShippingMethodSortIndex(int shippingMethodId, string shippingService)
        {
            var formattedShippingService = (shippingService ?? "").Replace("i:", "");

            if (formattedShippingService == ShippingUtils.FreeEconomyServiceType)
                return 4;
            if (formattedShippingService == ShippingUtils.StandardServiceName)
                return 5;
            if (formattedShippingService == ShippingUtils.ExpeditedServiceName)
                return 6;
            if (formattedShippingService == ShippingUtils.SecondDayServiceName)
                return 7;
            if (formattedShippingService == ShippingUtils.NextDayServiceName)
                return 8;
            if (formattedShippingService == ShippingUtils.SameDayServiceName)
                return 9;
            return 5;
        }

        public static int GetShippingMethodIndex(int shippingMethodId)
        {
            if (shippingMethodId == ShippingUtils.DynamexPTPSameShippingMethodId)
                return 25;
            if (shippingMethodId == ShippingUtils.DhlExpressWorldWideShippingMethodId
                || shippingMethodId == ShippingUtils.AmazonDhlExpressMXShippingMethodId)
                return 24;
            if (shippingMethodId == ShippingUtils.AmazonUPSGroundShippingMethodId
                || shippingMethodId == ShippingUtils.AmazonUPS2ndDayAirShippingMethodId)
                return 23;
            if (shippingMethodId == ShippingUtils.AmazonFedExExpressSaverShippingMethodId
                || shippingMethodId == ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId)
                return 22;

            return 1;
        }

        public static bool RequireAmazonProvider(int orderType,
            int market,
            string country,
            string sourceShippingService)
        {
            return orderType == (int)OrderTypeEnum.Prime
                        || ((ShippingUtils.IsServiceNextDay(sourceShippingService)
                            || ShippingUtils.IsServiceSameDay(sourceShippingService)
                            || ShippingUtils.IsServiceTwoDays(sourceShippingService))
                        && market == (int)MarketType.Amazon
                        && !ShippingUtils.IsInternational(country));
        }
    }
}
