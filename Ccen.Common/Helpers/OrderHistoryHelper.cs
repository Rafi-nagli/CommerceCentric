using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Helpers
{
    public class OrderHistoryHelper
    {
        public static string AddToBatchKey = "AddToBatch";
        public static string DSChangedKey = "DSChanged";
        public static string StatusChangedKey = "StatusChanged";
        public static string OnHoldKey = "OnHold";
        public static string OnRefundLockedKey = "OnRefundLocked";
        public static string ShippingMethodKey = "ShippingMethodId";
        public static string DismissAddressWarnKey = "IsDismissAddressValidation";

        public static string ManuallyPersonNameKey = "ManuallyPersonName";
        public static string ManuallyShippingAddress1Key = "ManuallyShippingAddress1";
        public static string ManuallyShippingAddress2Key = "ManuallyShippingAddress2";
        public static string ManuallyShippingCityKey = "ManuallyShippingCity";
        public static string ManuallyShippingCountryKey = "ManuallyShippingCountry";
        public static string ManuallyShippingZipKey = "ManuallyShippingZip";
        public static string ManuallyShippingZipAddonKey = "ManuallyShippingZipAddon";
        public static string ManuallyShippingPhoneKey = "ManuallyShippingPhone";

        public static string IsInsuredKey = "IsInsured";
        public static string IsSignConfirmationKey = "IsSignConfirmation";
        public static string ShipmentProviderTypeKey = "ShipmentProviderType";
        public static string ReplaceItemKey = "ReplaceItem";
        public static string RecalculateRatesKey = "RecalculateRates";

        public static string CancellationRequestKey = "CancellationRequest";

        public static string EmailStatusChangedKey = "EmailStatusChanged";
        public static string EmailReviewedStatusChangedKey = "EmailReviewedStatusChanged";

        public static string PrepareFieldName(string fieldName)
        {
            if (String.IsNullOrEmpty(fieldName))
                return "";
            return fieldName.Replace("Manually", "");
        }

        public static string PrepareFieldValue(string val)
        {
            if (String.IsNullOrEmpty(val))
                return "";
            return val.Replace("False", "no").Replace("True", "yes");
        }
    }
}
