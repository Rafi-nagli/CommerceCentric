using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Orders;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleChangeViewModel
    {
        public string ChangeName { get; set; }

        public string Value { get; set; }
        public string ValueUrl { get; set; }


        public string Message { get; set; }

        public DateTime? ChangeDate { get; set; }
        public long? ChangedBy { get; set; }
        public string ChangedByName { get; set; }


        public string FormattedChangeType
        {
            get
            {
                if (String.IsNullOrEmpty(ChangeName))
                    return "n/a";
                return ChangeName;
            }
        }

        public StyleChangeViewModel()
        {

        }

        public StyleChangeViewModel(StyleChangeHistoryDTO change)
        {
            ChangedBy = change.ChangedBy;
            ChangedByName = change.ChangedByName;
            ChangeDate = change.ChangeDate;

            ChangeName = StringHelper.AddSpacesBeforeUpperCaseLetters(change.FieldName);

            if (change.FieldName == StyleHistoryHelper.LocationKey)
            {
                ChangeName = "Location " + change.FromValue + " => " + change.ToValue;
            }

            if (change.FieldName == StyleHistoryHelper.OnHoldKey && change.ToValue == "True")
                ChangeName = "Put on hold";
            if (change.FieldName == StyleHistoryHelper.OnHoldKey && change.ToValue == "False")
                ChangeName = "UnHold";

            if (change.FieldName == StyleHistoryHelper.OnHoldKey && change.ToValue == "True")
                ChangeName = "Put on system hold";
            if (change.FieldName == StyleHistoryHelper.OnHoldKey && change.ToValue == "False")
                ChangeName = "System unhold";

            if (change.FieldName == StyleHistoryHelper.AttachListingKey)
            {
                Message = "Marketplace: " + GetMarketName(change.ExtendFromValue)
                    + ", ASIN:" + (change.ExtendToValue ?? "").Split(':').LastOrDefault();
            }

            if (change.FieldName == StyleHistoryHelper.DetachListingKey)
            {
                Message = "Marketplace: " + GetMarketName(change.ExtendFromValue)
                    + ", ASIN:" + (change.ExtendToValue ?? "").Split(':').LastOrDefault();
                ChangeName = "Listing detached";
            }

            if (change.FieldName == StyleHistoryHelper.CommentKey)
            {
                Message = change.ToValue;
                ChangeName = "Comment changed";
            }

            if (change.FieldName == StyleHistoryHelper.PictureKey)
            {
                Value = change.ToValue;
                ValueUrl = change.ToValue;
                ChangeName = "Picture changed";
            }

            if (change.FieldName == StyleHistoryHelper.PictureStatusKey)
            {
                Message = Enum.Parse(typeof(StylePictureStatuses), change.FromValue).ToString() + " => " + Enum.Parse(typeof(StylePictureStatuses), change.ToValue).ToString();
                ChangeName = "Picture status changed";
            }
        }

        private string GetMarketName(string marketValue)
        {
            try
            {
                var parts = marketValue.Split(":".ToCharArray());
                var market = Int32.Parse(parts[0]);
                var marketplace = parts[1];
                return MarketHelper.GetMarketName(market, marketplace);
            }
            catch
            {
                return "n/a";
            }
        }

        public  StyleChangeViewModel(StyleItemQuantityHistoryDTO change)
        {
            //Value = change.ToValue;
            ChangedBy = change.CreatedBy;
            ChangedByName = change.CreatedByName;
            ChangeDate = change.CreateDate;

            ChangeName = "Qty changed";

            if (ChangedByName == "Admin")
                ChangedByName = null;

            var html = "n/a";
            if (change.Type == (int)QuantityChangeSourceType.AddNewBox
                || (change.Type == (int)QuantityChangeSourceType.AddSpecialCase)
                    && change.Quantity < 0)
            { //NOTE: SP minus = return, back to inventory
                html = "<span class='green'>+" + Math.Abs(change.Quantity ?? 0) + "</span>";
            }
            if (change.Type == (int)QuantityChangeSourceType.Initial)
            {
                html = "<span class='green'>=" + change.Quantity + "</span>";
            }
            if (change.Type == (int)QuantityChangeSourceType.EnterNewQuantity)
            {
                if (change.Quantity == null)
                {
                    html = "use box qty";
                }
                else
                {
                    var delta = change.Tag != null ? change.Tag : (change.Quantity - change.FromQuantity).ToString();
                    html = "<span class='green'>+" + delta + "</span>";
                }
            }
            if (change.Type == (int)QuantityChangeSourceType.UseBoxQuantity)
            {
                html = "<span class='green'>" + change.FromQuantity + "-> box qty</span>";
            }

            if (change.Type == (int)QuantityChangeSourceType.AddSpecialCase
                && change.Quantity > 0)
            {
                html = "<span class='red'>-" + Math.Abs(change.Quantity ?? 0) + "</span>";
            }

            if (change.Type == (int)QuantityChangeSourceType.OrderCancelled)
            {
                html = "<span class='red'>+" + Math.Abs(change.Quantity ?? 0) + "</span>";
            }
            if (change.Type == (int)QuantityChangeSourceType.NewOrder
                || change.Type == (int)QuantityChangeSourceType.SentToFBA
                || change.Type == (int)QuantityChangeSourceType.SentToStore)
            {
                html = "<span class='green'>-" + Math.Abs(change.Quantity ?? 0) + "</span>";
            }

            if (change.Type == (int)QuantityChangeSourceType.Removed
                || change.Type == (int)QuantityChangeSourceType.RemoveBox
                || change.Type == (int)QuantityChangeSourceType.RemoveSpecialCase)
            {
                html = "<span class='red'>-" + change.FromQuantity + "</span>";
            }

            if (change.Type == (int)QuantityChangeSourceType.RemainingChanged)
            {
                if (change.FromQuantity != change.Quantity)
                    html = "<span class='gray'>=" + change.FromQuantity + " -> =" + change.Quantity + "</span>";
                else
                    html = "<span class='gray'>=" + change.Quantity + "</span>";
            }

            var formattedType = QuantityChangeSourceTypeHelper.GetName((QuantityChangeSourceType)change.Type);
            if (change.Type == (int)QuantityChangeSourceType.NewOrder)
            {
                formattedType = "<a href='" + UrlHelper.GetOrderUrl(change.Tag) + "' target='blank'>" + change.Tag + "</a>";
            }
            
            if (change.Type == (int)QuantityChangeSourceType.AddSpecialCase)
            {
                int intTag;
                if (Int32.TryParse(change.Tag, out intTag))
                {
                    formattedType = ((QuantityOperationType) intTag).ToString();
                }
            }

            if (change.Type == (int) QuantityChangeSourceType.OnHold)
            {
                bool boolTag;
                if (bool.TryParse(change.Tag, out boolTag))
                {
                    formattedType = boolTag ? "Yes" : "No";
                }
            }

            Message = "Size: " + change.Size + ", " + formattedType + ": " + html;
        }

        public static StyleChangeViewModel BuildCreateChange(StyleEntireDto style)
        {
            return new StyleChangeViewModel()
            {
                ChangeName = "Created",

                ChangeDate = style.CreateDate,
                ChangedBy = style.CreatedBy
            };
        }
    }
}