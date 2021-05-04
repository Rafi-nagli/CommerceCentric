using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Mailing;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderChangeViewModel
    {
        public OrderChangeTypes ChangeType { get; set; }

        public string Value { get; set; }
        public string ValueUrl { get; set; }

        //public Dictionary<string, string> Propeties { get; set; }
        

        public string Message { get; set; }

        public DateTime? ChangeDate { get; set; }
        public long? ChangedBy { get; set; }
        public string ChangedByName { get; set; }


        public string FormattedChangeType
        {
            get
            {
                switch (ChangeType)
                {
                    case OrderChangeTypes.Create:
                        return "Created";
                    case OrderChangeTypes.StatusChanged:
                        return "Status changed";

                    case OrderChangeTypes.Hold:
                        return "Put on hold";
                    case OrderChangeTypes.UnHold:
                        return "Unhold";

                    case OrderChangeTypes.DismissAddressWarn:
                        return "Address warn was dismissed";
                    case OrderChangeTypes.ChangeAddress:
                        return "Shipping address was changed";
                    case OrderChangeTypes.ChangeShippingMethod:
                        return "Shipping method was changed";
                    case OrderChangeTypes.RatesRecalculated:
                        return "Rates was recalculated";
                    case OrderChangeTypes.ChangeShippingProvider:
                        return "Shipping provider was changed";
                    case OrderChangeTypes.NewReturnRequest:
                        return "Received return request";


                    case OrderChangeTypes.AddToBatch:
                        return "Added to batch";
                    case OrderChangeTypes.RemoveFromBatch:
                        return "Removed from batch";
                    case OrderChangeTypes.PrintLabel:
                        return "Label was printed";
                    case OrderChangeTypes.FirstScan:
                        return "Package was scanned";
                    case OrderChangeTypes.Delivered:
                        return "Package was delivered";

                    case OrderChangeTypes.IncomeEmail:
                        return "Received incoming email";
                    case OrderChangeTypes.SendEmail:
                        return "Sent email";
                    case OrderChangeTypes.EmailStatusChanged:
                        return "Email status changed";

                    case OrderChangeTypes.NewComment:
                        return "New comment";
                }
                return "n/a";
            }
        }

        public OrderChangeViewModel()
        {
            
        }

        public OrderChangeViewModel(OrderChangeHistoryDTO change, IList<EmailOrderDTO> emails)
        {
            //Value = change.ToValue;
            ChangedBy = change.ChangedBy;
            ChangedByName = change.ChangedByName;
            ChangeDate = change.ChangeDate;

            if (change.FieldName == OrderHistoryHelper.OnHoldKey && change.ToValue == "True")
                ChangeType = OrderChangeTypes.Hold;
            if (change.FieldName == OrderHistoryHelper.OnHoldKey && change.ToValue == "False")
                ChangeType = OrderChangeTypes.UnHold;
            if (change.FieldName == OrderHistoryHelper.ShippingMethodKey)
            { 
                ChangeType = OrderChangeTypes.ChangeShippingMethod;
                Message = "";
                int shippingMethodId;
                if (!String.IsNullOrEmpty(change.FromValue) && change.FromValue != "0")
                {
                    var parts = change.FromValue.Split(';');
                    if (Int32.TryParse(parts[0], out shippingMethodId))
                        Message = ShippingUtils.GetPackageType(shippingMethodId).ToString() + " - " +
                                  ShippingUtils.GetShippingType(shippingMethodId).ToString();
                }
                else
                {
                    Message = "-";
                }
                Message += " -> ";
                if (!String.IsNullOrEmpty(change.ToValue) && change.ToValue != "0")
                {
                    var parts = change.ToValue.Split(';');
                    if (Int32.TryParse(parts[0], out shippingMethodId))
                        Message += ShippingUtils.GetPackageType(shippingMethodId).ToString() + " - " +
                                   ShippingUtils.GetShippingType(shippingMethodId).ToString();
                }
                else
                {
                    Message += "-";
                }
            }
            if (change.FieldName == OrderHistoryHelper.EmailStatusChangedKey)
            {
                var toValue = (EmailResponseStatusEnum) Enum.Parse(typeof (EmailResponseStatusEnum), change.ToValue);
                if (toValue == EmailResponseStatusEnum.NoResponseNeeded)
                {
                    var email = emails.FirstOrDefault(e => e.Id.ToString() == change.ExtendFromValue);
                    ChangeType = OrderChangeTypes.EmailStatusChanged;
                    Message = "Set No Response Needed";
                    if (email != null)
                    {
                        Value = email.Subject;
                        ValueUrl = UrlHelper.GetViewEmailUrl(email.Id, email.OrderIdString);
                    }
                }
            }

            if (change.FieldName == OrderHistoryHelper.RecalculateRatesKey)
            {
                ChangeType = OrderChangeTypes.RatesRecalculated;
            }
            if (change.FieldName == OrderHistoryHelper.ShipmentProviderTypeKey)
            {
                ChangeType = OrderChangeTypes.ChangeShippingProvider;
                Message = ""; //TODO:
            }

            if (change.FieldName == OrderHistoryHelper.AddToBatchKey && change.ToValue != null)
            {
                ChangeType = OrderChangeTypes.AddToBatch;
                Message = StringHelper.GetFirstNotEmpty(change.ExtendToValue, "Orders page");
            }
            if (change.FieldName == OrderHistoryHelper.AddToBatchKey && change.ToValue == null)
            {
                ChangeType = OrderChangeTypes.RemoveFromBatch;
                Message = "Name:" + (change.ExtendToValue ?? "-");
            }
            if (change.FieldName == OrderHistoryHelper.DismissAddressWarnKey)
                ChangeType = OrderChangeTypes.DismissAddressWarn;
            if (change.FieldName == OrderHistoryHelper.StatusChangedKey)
            {
                ChangeType = OrderChangeTypes.StatusChanged;
                Message = change.FromValue + " -> " + change.ToValue;
            }

            if (change.FieldName == OrderHistoryHelper.ManuallyPersonNameKey
                || change.FieldName == OrderHistoryHelper.ManuallyShippingAddress1Key
                || change.FieldName == OrderHistoryHelper.ManuallyShippingAddress2Key
                || change.FieldName == OrderHistoryHelper.ManuallyShippingCityKey
                || change.FieldName == OrderHistoryHelper.ManuallyShippingCountryKey
                || change.FieldName == OrderHistoryHelper.ManuallyShippingPhoneKey
                || change.FieldName == OrderHistoryHelper.ManuallyShippingZipKey
                || change.FieldName == OrderHistoryHelper.ManuallyShippingZipAddonKey)
            {
                ChangeType = OrderChangeTypes.ChangeAddress;
                Message = OrderHistoryHelper.PrepareFieldName(change.FieldName) + ": " + OrderHistoryHelper.PrepareFieldValue(change.FromValue) + " -> " + OrderHistoryHelper.PrepareFieldValue(change.ToValue);
            }
        }

        public OrderChangeViewModel(CommentDTO comment)
        {
            ChangeType = OrderChangeTypes.NewComment;

            Message = comment.Message + " (" + CommentHelper.TypeToString((CommentType)comment.Type) + ")";
            //Propeties = new string[] {comment.Id.ToString()};

            ChangedBy = comment.CreatedBy;
            ChangedByName = comment.CreatedByName;
            ChangeDate = comment.CreateDate;
        }

        public static IList<OrderChangeViewModel> BuildChanges(OrderShippingInfoDTO shippingInfo)
        {
            var results = new List<OrderChangeViewModel>();

            if (!String.IsNullOrEmpty(shippingInfo.TrackingNumber))
            {
                var printLabel = new OrderChangeViewModel()
                {
                    ChangeType = OrderChangeTypes.PrintLabel,
                    Value = shippingInfo.TrackingNumber,
                    ValueUrl = MarketUrlHelper.GetTrackingUrl(shippingInfo.TrackingNumber, shippingInfo.ShippingMethod.CarrierName, shippingInfo.TrackingStateSource),

                    Message = shippingInfo.ShippingMethod.CarrierName + " - " + shippingInfo.ShippingMethod.Name.ToString(),
                    ChangeDate = shippingInfo.LabelPurchaseDate,
                    ChangedBy = shippingInfo.LabelPurchaseBy,
                    ChangedByName = shippingInfo.LabelPurchaseByName,
                };
                results.Add(printLabel);
            }

            if (shippingInfo.ActualDeliveryDate.HasValue)
            {
                var deliveredEvent = new OrderChangeViewModel()
                {
                    ChangeType = OrderChangeTypes.Delivered,
                    Message = shippingInfo.TrackingStateEvent,

                    ChangeDate = shippingInfo.ActualDeliveryDate.Value,
                    ChangedBy = null
                };
                results.Add(deliveredEvent);
            }
            
            return results;
        }

        public OrderChangeViewModel(EmailOrderDTO email)
        {
            if (email.FolderType == (int)EmailFolders.Inbox)
                ChangeType = OrderChangeTypes.IncomeEmail;
            if (email.FolderType == (int)EmailFolders.Sent)
                ChangeType = OrderChangeTypes.SendEmail;

            Value = email.Subject;
            ValueUrl = UrlHelper.GetViewEmailUrl(email.Id, email.OrderIdString);
            ChangeDate = email.ReceiveDate;
        }

        public OrderChangeViewModel(ReturnRequestDTO returnRequest)
        {
            ChangeType = OrderChangeTypes.NewReturnRequest;

            Message = returnRequest.Reason;
            Message += ", Customer Comments: " + StringHelper.IfEmpty(returnRequest.CustomerComments, "-")
                    + ", Details: " + StringHelper.IfEmpty(returnRequest.Details, "-");

            ChangeDate = returnRequest.ReceiveDate;
        }

        public static OrderChangeViewModel BuildCreateOrderChange(DTOOrder order)
        {
            return new Orders.OrderChangeViewModel()
            {
                ChangeType = OrderChangeTypes.Create,

                ChangeDate = order.CreateDate
            };
        }

        public OrderChangeViewModel(RefundViewModel refund)
        {
            ChangeType = OrderChangeTypes.MadeRefund;

            Message = refund.Amount + " (" + refund.StatusName + ")";
            ChangeDate = refund.Date;
            ChangedBy = refund.By;
            //ChangedByName = refund.ByName;
        }
    }
}