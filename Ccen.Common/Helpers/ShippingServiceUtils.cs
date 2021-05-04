using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using System;
using Amazon.DTO.Listings;

namespace Amazon.Common.Helpers
{
    public class ShippingServiceUtils
    {
        public const string USPSCarrier = "USPS";
        public const string DHLCarrier = "DHL";
        public const string DHLMXCarrier = "DHLMX";
        public const string DynamexCarrier = "Dynamex";
        public const string UPSCarrier = "UPS";
        public const string FedexCarrier = "FEDEX";
        public const string DHLEComCarrier = "DHL eCom";
        public const string IBCCarrier = "IBC";
        public const string FIMSCarrier = "FIMS";
        public const string SkyPostalCarrier = "SKYPOSTAL";


        public static List<ShippingTypeCode> LocalTypesUniversal = new List<ShippingTypeCode>
        {
            ShippingTypeCode.Standard, 
            ShippingTypeCode.Priority, 
            ShippingTypeCode.PriorityExpress,
            ShippingTypeCode.SameDay
        };

        public static string[] GetRelatedCarrierNames(CarrierGroupType carrier)
        {
            switch (carrier)
            {
                case CarrierGroupType.USPS:
                    return new [] { USPSCarrier, IBCCarrier };
                case CarrierGroupType.DHL:
                    return new [] { DHLCarrier, DHLMXCarrier };
                case CarrierGroupType.Dynamex:
                    return new [] { DynamexCarrier };
                case CarrierGroupType.DHLECom:
                    return new [] { DHLEComCarrier };
                case CarrierGroupType.Fedex:
                    return new[] { FedexCarrier };
            }
            return null;
        }

        public static ShippingTypeCode AmazonServiceToUniversalServiceType(string serviceType)
        {
            /* Available List
             Expedited
            FreeEconomy
            NextDay
            SameDay
            SecondDay
            Scheduled
            Standard*/

            switch ((serviceType ?? "").ToLower())
            {
                case "standard":
                    return ShippingTypeCode.Standard;
                case "expedited":
                    return ShippingTypeCode.Priority;
                case "sameday":
                    return ShippingTypeCode.SameDay;
                case "nextday": //NOTE: both valid
                case "secondday":
                    return ShippingTypeCode.PriorityExpress;
                case "i:standard":
                    return ShippingTypeCode.IStandard;
                case "i:expedited":
                    return ShippingTypeCode.IPriority;
                default:
                    return ShippingTypeCode.None;
            }
        }


        public static PackageTypeCode GetDefaultInternationalPackage(IList<ListingOrderDTO> items)
        {
            /*
             * надо было сделать наверно 3 значенита, типа unknown, flat, regular
             * если в заказе несколько пижам и у одной из них стоит reqular, то выставляем shipping type = reqular
            */

            var itemCount = items.Sum(i => i.QuantityOrdered);
            var hasRegular = items.Any(i => i.InternationalPackage == "Regular");
            if (hasRegular)
                return PackageTypeCode.Regular;

            if (items.Count == 1 && items[0].InternationalPackage == "Flat")
                return PackageTypeCode.Flat;

            var xLargeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XL")).Sum(i => i.QuantityOrdered);
            var largeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "L")).Sum(i => i.QuantityOrdered);
            var mediumItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "M")).Sum(i => i.QuantityOrdered);
            var smallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "S")).Sum(i => i.QuantityOrdered);
            var xSmallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XS")).Sum(i => i.QuantityOrdered);
            var unknownItemCount = items.Where(i => string.IsNullOrEmpty(i.ShippingSize)).Sum(i => i.QuantityOrdered);

            /* Up to 3 shipping size= small, when all marked as Int. flat can go to Int. flat automatically.
            Obviously includes cases when one of them (or more) is XS..
            */
            if (items.All(i => i.InternationalPackage == "Flat")
                && (unknownItemCount == 0
                    && xLargeItemCount == 0
                    && largeItemCount == 0
                    && mediumItemCount == 0
                    && (smallItemCount + xSmallItemCount <= 3)))
            {
                return PackageTypeCode.Flat;
            }

            if (itemCount > 1)
                return PackageTypeCode.None;

            return PackageTypeCode.None;
        }

        public const string ShippingSizeXL = "XL";
        public const string ShippingSizeL = "L";
        public const string ShippingSizeM = "M";
        public const string ShippingSizeS = "S";
        public const string ShippingSizeXS = "XS";

        public static string MaxShippingSize(string shippingSize1, string shippingSize2)
        {
            if (GetShippingSizeIndex(shippingSize1) > GetShippingSizeIndex(shippingSize2))
                return shippingSize1;
            return shippingSize2;
        }

        public static int GetShippingSizeIndex(string shippingSize)
        {
            switch (shippingSize)
            {
                case ShippingSizeXS:
                    return 1;
                case ShippingSizeS:
                    return 2;
                case ShippingSizeM:
                    return 3;
                case ShippingSizeL:
                    return 4;
                case ShippingSizeXL:
                    return 5;
            }
            return 6;
        }

        /// <summary>
        /// International Flat
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool IsSupportLargeEnvelope(IList<OrderItemRateInfo> items)
        {
            /*We should break to multiple international packages, if user ordered more then one pajama and:
            1.	One of the pajama’s, International Package <> Flat.
            2.	If 2 pajamas, and Shipping size M+S or above (i.e. M+M, M+L)
            3.	If 3 or more pajamas, try to break to 2-3 packages 
            */

            var xLargeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XL")).Sum(i => i.Quantity);
            var largeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "L")).Sum(i => i.Quantity);
            var mediumItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "M")).Sum(i => i.Quantity);
            var smallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "S")).Sum(i => i.Quantity);
            var xSmallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XS")).Sum(i => i.Quantity);

            var unknownItemCount = items.Where(i => string.IsNullOrEmpty(i.ShippingSize)).Sum(i => i.Quantity);

            if (unknownItemCount > 1
                || xLargeItemCount > 0
                || largeItemCount > 1
                || unknownItemCount * 6 + largeItemCount * 6 + mediumItemCount * 4.5 + smallItemCount * 2 + xSmallItemCount * 1.5 > 6

                //NOTE: for double check
                || mediumItemCount > 1
                || smallItemCount > 3
                || xSmallItemCount > 4)
                return false;

            return true;
        }

        public static bool IsSupportFedexEnvelope(IList<OrderItemRateInfo> items)
        {
            var isSupportFlatEnvelope = IsSupportFlatEnvelope(items);
            if (isSupportFlatEnvelope)
                return true;

            var xLargeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XL")).Sum(i => i.Quantity);
            var largeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "L")).Sum(i => i.Quantity);
            var mediumItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "M")).Sum(i => i.Quantity);
            var smallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "S")).Sum(i => i.Quantity);
            var xSmallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XS")).Sum(i => i.Quantity);
            var unknownItemCount = items.Where(i => string.IsNullOrEmpty(i.ShippingSize)).Sum(i => i.Quantity);

            #region Maximum: 1.For envelope L+S
            if (unknownItemCount > 0 || xLargeItemCount > 0)
                return false;
            if ((largeItemCount + mediumItemCount) == 1
                && (smallItemCount + xSmallItemCount) <= 1)
                return true;

            if ((largeItemCount + mediumItemCount) == 0
                && (smallItemCount + xSmallItemCount) <= 2)
                return true;

            if (items.Sum(i => i.Quantity) > 1)
                return false;
            #endregion

            return false;
        }

        public static bool IsSupportFedexMediumBox(IList<OrderItemRateInfo> items)
        {
            var xLargeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XL")).Sum(i => i.Quantity);
            var largeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "L")).Sum(i => i.Quantity);
            var mediumItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "M")).Sum(i => i.Quantity);
            var smallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "S")).Sum(i => i.Quantity);
            var xSmallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XS")).Sum(i => i.Quantity);
            var unknownItemCount = items.Where(i => string.IsNullOrEmpty(i.ShippingSize)).Sum(i => i.Quantity);

            #region Maximum: 3.	Medium Box: L+L
            if (unknownItemCount > 0)
                return false;

            if ((largeItemCount) == 2
                && (smallItemCount + xSmallItemCount + mediumItemCount) == 0)
                return true;

            if (IsSupportFedexPak(items))
                return true;
            #endregion

            return false;
        }

        public static bool IsSupportFedexPak(IList<OrderItemRateInfo> items)
        {
            var xLargeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XL")).Sum(i => i.Quantity);
            var largeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "L")).Sum(i => i.Quantity);
            var mediumItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "M")).Sum(i => i.Quantity);
            var smallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "S")).Sum(i => i.Quantity);
            var xSmallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XS")).Sum(i => i.Quantity);
            var unknownItemCount = items.Where(i => string.IsNullOrEmpty(i.ShippingSize)).Sum(i => i.Quantity);

            /*3.	Why ccen puts 3+ Large products into Fedex Pak?
            Fedex Pak can hold:
            a.	XL + XS
            b.	L+L+S 
            c.	M+M+M+XS
            d.	M+S+S+S+XS

            //XS - skazhem 10
            */

            #region Maximum: 2.	For Pack: L+M (or L+S+S) or XL
            if (unknownItemCount > 0 || xLargeItemCount > 1)
                return false;

            if (xLargeItemCount * 9 + xSmallItemCount * 1 + smallItemCount * 2 + mediumItemCount * 3 + largeItemCount * 4 <= 10)
            {
                return true;
            }

            //if ((xLargeItemCount) == 1
            //    && smallItemCount <= 1
            //    && (xSmallItemCount + mediumItemCount + largeItemCount) == 0)
            //    return true;

            //if (xLargeItemCount == 0
            //    && largeItemCount <= 1
            //    && (smallItemCount + xSmallItemCount + mediumItemCount + largeItemCount) <= 1)
            //    return true;

            //if (xLargeItemCount == 0
            //    && largeItemCount <= 2
            //    && smallItemCount <= 1)
            //    return true;

            //if (xLargeItemCount + largeItemCount == 0
            //    && mediumItemCount <= 2
            //    && (smallItemCount + xSmallItemCount + mediumItemCount) <= 1)
            //    return true;


            //if ((largeItemCount) == 0
            //    && (smallItemCount + xSmallItemCount + mediumItemCount) <= 2)
            //    return true;
            #endregion

            return false;
        }

        public static bool IsSupportFlatEnvelope(IList<OrderItemRateInfo> items)
        {
            if (items.All(i => i.ItemStyle == ItemStyleType.Nightgown
                || i.ItemStyle == ItemStyleType.NightgownSaras))
            {
                if (items.Sum(i => i.Quantity * (i.ItemStyle == ItemStyleType.Nightgown ? 1 : 2)) <= 5)
                    return true;
                return false;
            }
            
            var xLargeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XL")).Sum(i => i.Quantity);
            var largeItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "L")).Sum(i => i.Quantity);
            var mediumItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "M")).Sum(i => i.Quantity);
            var smallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "S")).Sum(i => i.Quantity);
            var xSmallItemCount = items.Where(i => !string.IsNullOrEmpty(i.ShippingSize) && (i.ShippingSize == "XS")).Sum(i => i.Quantity);

            var unknownItemCount = items.Where(i => string.IsNullOrEmpty(i.ShippingSize)).Sum(i => i.Quantity);

            //We have a big problem when system selects Flat priority shipping, when user ordered many pajamas.
            //Priority Flat should never be selected if user bough more than 2 Medium size pajamas, or more than one Large/XL pajama.
            //and 1M + 2S or 4S or 6XS or 2S+2XS
            //In this cases we should see dropdown.


            //6XS
            //2S + (2XS || 2S) //duplicate 1M || 1L || 1XL
            //1M + (2S || 3XS) //duplicate 1L
            //1L + any (obsolet)
            //1L + M (и меньше) (new)
            //1XL

            //2M < sum
            //6XS <= sum

            //1S = 1.5XS
            //1M = 2S
            //1L = 1.5M
            //1XL = 1.5L

            //Update 02/13/2016
            //esli vtoraya XS or S to vne zavisimosti kakaya pervaya ono pomestitsya vo Flat
            //eщё правило XS+XS+ Unknown -> flat
            if (unknownItemCount > 0)
            {
                if (unknownItemCount > 1)
                    return false;

                //Sum of other items > 2
                if (xLargeItemCount + largeItemCount + mediumItemCount + smallItemCount + xSmallItemCount > 2)
                    return false;

                //Sum of other items = 2
                if (xLargeItemCount + largeItemCount + mediumItemCount + smallItemCount + xSmallItemCount == 2)
                {
                    if (xSmallItemCount == 2)
                        return true;
                    return false;
                }

                //Sum of other items = 1
                if (xLargeItemCount + largeItemCount + mediumItemCount + smallItemCount + xSmallItemCount == 1)
                {
                    if (xSmallItemCount > 0
                        || smallItemCount > 0)
                        return true;

                    return false;
                }

                //Sum of other items = 0
                if (xLargeItemCount + largeItemCount + mediumItemCount + smallItemCount + xSmallItemCount == 0)
                    return true;

                return false;
            }


            if (xLargeItemCount > 0
                || largeItemCount > 1
                || mediumItemCount > 2
                || smallItemCount >= 4
                || xSmallItemCount >= 6
                || (largeItemCount + mediumItemCount + smallItemCount + xSmallItemCount >= 6) //if more that 6 any size
                || (largeItemCount == 1 && (mediumItemCount + smallItemCount + xSmallItemCount > 1)) //1L + M (и меньше)
                || (mediumItemCount == 2 && (smallItemCount > 0)) //2M + any
                || (mediumItemCount == 2 && (xSmallItemCount > 1)) //>2M + 1XS
                || (mediumItemCount == 1 && (1.5 * smallItemCount + xSmallItemCount > 3)) //+2s or 3xs
                || (smallItemCount >= 2 && xSmallItemCount >= 2)) //2S+2XS
                return false;

            return true;
        }

        public static bool IsEqualPackages(ItemPackageDTO package1, ItemPackageDTO package2)
        {
            if (package1 == null 
                || package2 == null)
            {
                return false;
            }

            if (package1.PackageWidth == package1.PackageWidth
                && package1.PackageHeight == package2.PackageHeight
                && package1.PackageLength == package2.PackageLength)
            {
                return true;
            }

            return false;
        }

        public static bool FitInPackage(ItemPackageDTO boxPackage, ItemPackageDTO testPackage)
        {
            //NOTE: if one of them is empty, skip validation
            if (boxPackage.IsEmpty || testPackage.IsEmpty)
                return true;

            if (boxPackage.MaxVolume.HasValue)
            {
                var testVolume = testPackage.PackageWidth * testPackage.PackageHeight * testPackage.PackageLength;
                if (testVolume > boxPackage.MaxVolume)
                    return false;
            }

            var boxSizes = new decimal[] { boxPackage.PackageWidth.Value, boxPackage.PackageHeight.Value, boxPackage.PackageLength.Value };
            var testSizes = new decimal[] { testPackage.PackageWidth.Value, testPackage.PackageHeight.Value, testPackage.PackageLength.Value };
            boxSizes = boxSizes.OrderByDescending(s => s).ToArray();
            testSizes = testSizes.OrderByDescending(s => s).ToArray();
            for (var i = 0; i < 3; i++)
            {
                if (testSizes[i] > boxSizes[i])
                    return false;
            }

            return true;
        }
    }
}
