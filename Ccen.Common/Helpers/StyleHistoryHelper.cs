using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Helpers
{
    public class StyleHistoryHelper
    {
        public static string StyleIdKey = "StyleId";
        public static string OnHoldKey = "OnHold";
        public static string OnSystemHoldKey = "OnSystemHold";
        public static string FillingStatusKey = "FillingStatus";
        public static string PictureKey = "Picture";
        public static string PictureStatusKey = "PictureStatus";
        public static string AttachListingKey = "AttachedListing";
        public static string DetachListingKey = "DetachListing";
        public static string CommentKey = "Comment";
        public static string DropShipperKey = "DropShipper";
        public static string AutoSelectDropShipperKey = "AutoSelectDropShipper";
        public static string DropShipperEffectiveDateKey = "DropShipperEffectiveDate";
        public static string LocationKey = "LocationKey";
        public static string TagsKey = "TagsKey";
        public static string OpenBoxKey = "OpenBox";
        public static string SealedBoxKey = "SealedBox";
        public static string AddOpenBoxKey = "AddOpenBox";
        public static string AddSealedBoxKey = "AddSealedBox";
        public static string RemoveOpenBoxKey = "RemoveOpenBox";
        public static string RemoveSealedBoxKey = "RemoveSealedBox";
        public static string UpdateOpenBoxKey = "UpdateOpenBox";
        public static string UpdateSealedBoxKey = "UpdateSealedBox";
        public static string CloseOpenBoxKey = "CloseOpenBox";
        public static string CloseSealedBoxKey = "CloseSealedBox";
        public static string RestockDateKey = "RestockDate";
        public static string StyleNameKey = "StyleName";

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
