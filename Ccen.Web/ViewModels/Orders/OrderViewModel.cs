using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Sorting;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Orders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using UrlHelper = Amazon.Web.Models.UrlHelper;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets;
using Amazon.Utils;
using Amazon.Web.ViewModels.Html;
using Amazon.Core.Helpers;
using Amazon.DTO.Orders;
using Amazon.DAL.Repositories;
using Amazon.Core.Models.Calls;
using Amazon.Core.Views;

namespace Amazon.Web.ViewModels
{
    public class OrderViewModel
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string FulfillmentChannel { get; set; }

        public string OrderId { get; set; }
        public string CustomerOrderId { get; set; }
        public string MarketOrderId { get; set; }

        public long? DropShipperId { get; set; }
        public string DropShipperName { get; set; }
        public int? SubOrderNumber { get; set; }

        public long? BatchId { get; set; }
        public string OrderStatus { get; set; }
        public string Quantity { get; set; }
        public int QuantityShipped { get; set; }
        public int QuantityOrdered { get; set; }

        public int? ShippingGroupId { get; set; }
        public int ShippingMethodId { get; set; }
        public string ShippingMethodName { get; set; }
        public string ShippingPackageName { get; set; }
        public string InitialServiceType { get; set; }
        public string SourceServiceType { get; set; }
        public int? UpgradeLevel { get; set; }
        public int ShippingCalculationStatus { get; set; }
        public int ShippingMethodIndex
        {
            get { return ShippingUtils.GetShippingMethodSortIndex(ShippingMethodId, InitialServiceType); }
        }

        public string FormattedShippingMethodName { get; set; }

        public string BuyerName { get; set; }
        public string PersonName { get; set; }
        public bool HasPhoneNumber { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? AlignedExpectedShipDate
        {
            get
            {
                return ShippingUtils.AlignMarketDateByEstDayEnd(ExpectedShipDate, (MarketType)Market);
            }
        }

        public DateTime? EstDeliveryDate { get; set; }
        public DateTime? ExpDeliveryDate { get; set; }

        public DateTime? AlignedExpDeliveryDate
        {
            get
            {
                return ShippingUtils.AlignMarketDateByEstDayEnd(ExpDeliveryDate, (MarketType)Market);
            }
        }

        public double Weight { get; set; }

        public bool AllItemIsWeight
        {
            get { return Items != null ? Items.All(i => i.Weight > 0) : false; }
        }

        public string WeightString
        {
            get
            {
                if (!AllItemIsWeight)
                    return "";

                return WeightHelper.FormatWeight(Weight);
            }
        }

        public IList<LabelViewModel> Labels { get; set; }

        public bool HasLabel
        {
            get
            {
                return Labels != null && Labels.Any(l => l.IsPrinted);
            }
        }

        public bool HasAllLabels
        {
            get
            {
                return Labels != null && Labels.All(l => l.IsPrinted);
            }
        }

        public bool HasMailLabel
        {
            get { return Labels != null && Labels.Any(l => l.FromType == LabelFromType.Mail); }
        }

        public int? MainNumberInBatch
        {
            get { return HasLabel ? Labels[0].NumberInBatch : null; }
        }

        public string MainTrackingNumber
        {
            get
            {
                return HasLabel ? Labels[0].TrackingNumber : "";
            }
        }

        public bool IsShippingPriority
        {
            get { return ShippingUtils.IsMethodNonStandard(ShippingMethodId); }
        }

        public bool IsInternational
        {
            get { return ShippingUtils.IsInternational(ShippingCountry); }
        }


        public int TotalItemQuantity
        {
            get { return Items != null ? Items.Sum(i => i.QuantityOrdered) : 0; }
        }
        public decimal ItemPrice
        {
            get { return Items != null ? Items.Sum(i => i.QuantityOrdered != 0 ? i.ItemPrice : 0) : 0; }
        }

        public decimal ItemDiscount
        {
            get { return Items != null ? Items.Sum(i => i.ItemDiscount ?? 0) : 0; }
        }

        public decimal RefundItemPrice
        {
            get { return Items != null ? Items.Sum(i => i.RefundItemPrice ?? 0) : 0; }
        }

        public decimal ShippingPrice
        {
            get { return Items != null ? Items.Sum(i => i.QuantityOrdered != 0 ? i.ShippingPrice : 0) : 0; }
        }

        public decimal? ShippingDiscount
        {
            get { return Items != null ? Items.Sum(i => i.QuantityOrdered != 0 ? i.ShippingDiscount : 0) : (decimal?)null; }
        }

        public decimal? RefundShippingPrice
        {
            get { return Items != null ? Items.Sum(i => i.RefundShippingPrice ?? 0) : (decimal?)null; }
        }

        public decimal ShippingPriceInUSD
        {
            get { return PriceHelper.RougeConvertToUSD(Currency, ShippingPrice); }
        }

        public decimal ItemPriceInUSD
        {
            get { return PriceHelper.RougeConvertToUSD(Currency, ItemPrice); }
        }

        public bool HasItemRefund
        {
            get { return RefundItemPrice > 0; }
        }

        public bool HasShippingRefund
        {
            get { return RefundShippingPrice > 0; }
        }



        public decimal? TotalExcessiveShipmentThreshold
        {
            get
            {
                var total = Items != null ? Items.Sum(i => (i.ExcessiveShipmentThreshold ?? 0) * i.QuantityOrdered) : 0;
                if (total == 0)
                    total = 1;
                return total;
            }
        }



        public bool HasBatchLabel { get; set; }

        public string ShipmentCarrier { get; set; }
        public int ShipmentProviderType { get; set; }

        public string ShipmentProviderName
        {
            get { return ShipmentProviderHelper.GetName((ShipmentProviderType)ShipmentProviderType); }
        }

        public decimal StampsShippingCost { get; set; }


        public bool IsInsured { get; set; }
        public decimal InsuranceCost { get; set; }


        public bool IsSignConfirmation { get; set; }
        public decimal SignConfirmationCost { get; set; }

        public int ShippingTypeCode { get; set; }
        public string ActualAmazonService { get; set; }

        public bool OnHold { get; set; }
        public DateTime? OnHoldUpdateDate { get; set; }

        public bool IsRefundLocked { get; set; }

        public bool IsDisabled
        {
            get
            {
                return false; //IsPrime; 
            }
        }

        public string LastCommentMessage { get; set; }
        public DateTime? LastCommentDate { get; set; }
        public long? LastCommentBy { get; set; }
        public string LastCommentByName { get; set; }
        public long? LastCommentNumber { get; set; }


        //Notification
        public bool IsPrime { get; set; }
        public bool IsSameDay { get; set; }
        public bool IsOversold { get; set; }
        public bool IsPossibleDuplicate { get; set; }
        public string PossibleDuplicateReason { get; set; }
        public bool InBlackList { get; set; }
        public string InBlackListReason { get; set; }
        public int? GetRateResult { get; set; }
        public string GetRateMessage { get; set; }
        public AddressValidationStatus AddressValidationStatus { get; set; }
        public string AddressStampsValidationMessage { get; set; }
        public string AddressGoogleValidationMessage { get; set; }
        public bool IsDismissAddressValidation { get; set; }

        public string AttachedToOrderString { get; set; }

        public DateTime? FutureShippingDate { get; set; }
        public bool IsOverchargedShipping { get; set; }
        public bool HasOversoldItems { get; set; }
        public bool IsInternationalExpress { get; set; }


        public bool IsPOBox
        {
            get { return AddressHelper.IsPOBox(ShippingAddress1, ShippingAddress2); }
        }

        public bool IsAPO
        {
            get { return AddressHelper.IsAPO(ShippingCountry, ShippingState, ShippingCity); }
        }

        public bool IsUSIsland
        {
            get { return AddressHelper.IsUSIsland(ShippingCountry, ShippingState); }
        }

        public bool IsPostalService
        {
            get { return IsPOBox || IsAPO || IsUSIsland; }
        }


        public bool HasCancelationRequest { get; set; }
        public int CancelationRequestCount { get; set; }

        public int Selected { get; set; }

        public int ItemCount { get; set; }


        public string GetRateMessageToUI
        {
            get
            {
                return ExceptionHelper.FormatErrorMessageToUI(GetRateMessage);
            }
        }


        public string SellerUrl
        {
            get { return UrlHelper.GetSellerCentralOrderUrl((MarketType)Market, MarketplaceId, OrderId); }
        }

        public string NewEmailUrl
        {
            get { return UrlHelper.GetNewEmailUrl(OrderId); }
        }

        public string OrderEmailsUrl
        {
            get { return UrlHelper.GetOrderEmailsUrl(OrderId); }
        }

        public string Currency
        {
            get { return PriceHelper.GetCurrencySymbol((MarketType)Market, MarketplaceId); }
        }

        public bool HasShippingOptions
        {
            get { return ShippingOptions != null && ShippingOptions.Count > 1; }
        }

        public string BatchString { get; set; }

        public string BatchUrl
        {
            get
            {
                return UrlHelper.GetBatchUrl(BatchId);
            }
        }

        public bool IsOrder { get { return Id > 0; } }

        public int NumberByLocation { get; set; }
        public bool HasAddressLengthIssue { get; set; }
        public bool HasAddressInvalidSymbols { get; set; }

        public bool UIChecked { get; set; }
        public bool UIDisabled { get; set; }

        public DateTime? ShippingDate { get; set; }


        private List<SelectListShippingOption> shippingOptions;
        public List<SelectListShippingOption> ShippingOptions
        {
            get { return shippingOptions; }
            set { shippingOptions = value; }
        }

        public bool IsWeight
        {
            get
            {
                return !String.IsNullOrEmpty(WeightString);
            }
        }

        public string FormattedOrderId
        {
            get { return OrderHelper.FormatOrderNumber(OrderId, (MarketType)Market); }
        }

        public string FormattedCustomerOrderId
        {
            get { return OrderHelper.FormatOrderNumber(CustomerOrderId, (MarketType)Market); }
        }

        public string MarketName
        {
            get
            {
                return MarketHelper.GetMarketName(Market, MarketplaceId);
            }
        }

        public string BayNumber
        {
            get { return Items != null ? Items.FirstOrDefault()?.BayNumber : ""; }
        }

        public long LocationIndex
        {
            get { return Items != null ? (Items.FirstOrDefault()?.LocationIndex ?? 0) : 0; }
        }

        public IList<OrderItemViewModel> Items { get; set; }

        public IList<OrderNotifyViewModel> Notifies { get; set; }


        public OrderViewModel()
        {

        }

        public OrderViewModel(DTOOrder item, bool isFulfilmentUser)
        {
            #region General

            Id = item.Id;
            EntityId = item.Id;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;
            FulfillmentChannel = item.FulfillmentChannel;

            BatchId = item.BatchId;
            BatchString = item.BatchName;

            DropShipperId = item.DropShipperId;
            DropShipperName = item.DropShipperName;
            SubOrderNumber = item.SubOrderNumber;

            OrderStatus = item.OrderStatus;
            InitialServiceType = item.InitialServiceType;
            SourceServiceType = item.SourceShippingService;
            UpgradeLevel = item.UpgradeLevel;
            ShippingCalculationStatus = item.ShippingCalculationStatus;

            OrderId = item.OrderId;
            MarketOrderId = item.MarketOrderId;
            CustomerOrderId = item.CustomerOrderId;

            Quantity = item.Quantity.ToString("G");
            PersonName = item.FinalPersonName;
            BuyerName = item.BuyerName;

            ShippingCountry = item.FinalShippingCountry;
            ShippingState = item.FinalShippingState;
            ShippingCity = item.FinalShippingCity;
            ShippingAddress1 = item.FinalShippingAddress1;
            ShippingAddress2 = item.FinalShippingAddress2;

            OnHold = item.OnHold;
            OnHoldUpdateDate = item.OnHoldUpdateDate;
            IsRefundLocked = item.IsRefundLocked ?? false;

            ItemCount = item.Items != null ? item.Items.Count : 0;


            HasAddressLengthIssue = !AddressHelper.ValidateLenghts(item.GetAddressDto(),
                (ShipmentProviderType)item.ShipmentProviderType,
                item.DropShipperId);
            HasAddressInvalidSymbols = AddressHelper.HasInvalidSymbols(item.GetAddressDto());
            AttachedToOrderString = item.AttachedToOrderString;

            OrderDate = item.OrderDate;
            ExpDeliveryDate = item.LatestDeliveryDate;
            ExpectedShipDate = item.LatestShipDate; //.EarliestShipDate;
            Weight = item.WeightD;

            #endregion


            #region Additional/Flags

            HasPhoneNumber = item.IsManuallyUpdated ? !String.IsNullOrEmpty(item.ManuallyShippingPhone) : !String.IsNullOrEmpty(item.ShippingPhone);

            IsDismissAddressValidation = item.IsDismissAddressValidation;
            //AddressValidationStatus = (AddressValidationStatus)item.AddressValidationStatus;

            AttachedToOrderString = item.AttachedToOrderString;

            LastCommentMessage = item.LastCommentMessage;
            LastCommentDate = item.LastCommentDate;
            LastCommentBy = item.LastCommentBy;
            LastCommentByName = item.LastCommentByName;
            LastCommentNumber = item.LastCommentNumber;

            IsPrime = item.OrderType == (int)OrderTypeEnum.Prime;

            #endregion


            #region Notifies

            if (item.Notifies != null)
            {
                var duplicateNotify = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.Duplicate);
                if (duplicateNotify != null)
                {
                    IsPossibleDuplicate = true;
                    PossibleDuplicateReason = duplicateNotify.Message;
                }
                var oversoldNotify = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.OversoldItem
                    || n.Type == (int)OrderNotifyType.OversoldOnHoldItem);
                if (oversoldNotify != null)
                {
                    IsOversold = true;
                }
                var blackListNotify = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.BlackList);
                if (blackListNotify != null)
                {
                    InBlackList = true;
                    InBlackListReason = blackListNotify.Message;
                }
                var getRateNotify = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.CalcRate);
                if (getRateNotify != null && getRateNotify.Status == (int)GetRateResultType.Error)
                {
                    GetRateResult = getRateNotify.Status;
                    GetRateMessage = getRateNotify.Message;
                }

                #region Address Validation Status
                var addressStampsValidationMessage =
                    item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.AddressCheckStamps);
                if (addressStampsValidationMessage != null)
                {
                    AddressStampsValidationMessage = addressStampsValidationMessage.Message;
                }
                var addressGoogleValidationMessage =
                    item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.AddressCheckGoogleGeocode);
                if (addressGoogleValidationMessage != null)
                {
                    AddressGoogleValidationMessage = addressGoogleValidationMessage.Message;
                }
                //Exclude Fedex address validation
                var isFedexShipping = item.ShippingInfos.Any(sh => sh.IsActive && sh.ShippingMethod?.CarrierName == ShippingServiceUtils.FedexCarrier);
                var isStampsShipping = item.ShippingInfos.Any(sh => sh.IsActive && sh.ShipmentProviderType == (int)Amazon.Core.Models.Settings.ShipmentProviderType.Stamps);
                IList<OrderNotifyDto> addressNotifications = item.Notifies.OrderByDescending(i => i.CreateDate).ToList();
                if (isFedexShipping)
                    addressNotifications = ArrayHelper.ToArray(addressNotifications.FirstOrDefault(i => i.Type == (int)OrderNotifyType.AddressCheckFedex));
                if (isStampsShipping)
                    addressNotifications = ArrayHelper.ToArray(addressNotifications.FirstOrDefault(i => i.Type == (int)OrderNotifyType.AddressCheckStamps));

                var fedexAddressCheckStatus = addressNotifications.FirstOrDefault(i => i.Type == (int)OrderNotifyType.AddressCheckFedex)?.Status ?? (int)AddressValidationStatus.None;
                var stampsAddressCheckStatus = addressNotifications.FirstOrDefault(i => i.Type == (int)OrderNotifyType.AddressCheckStamps)?.Status ?? (int)AddressValidationStatus.None;

                AddressValidationStatus = (AddressValidationStatus)Math.Max(fedexAddressCheckStatus, stampsAddressCheckStatus);

                #endregion

                var furureShipping = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.FutureShipping);
                if (furureShipping != null)
                {
                    FutureShippingDate = DateHelper.FromDateString(furureShipping.Message);
                }
                var overchargedShipping =
                    item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.OverchargedShpppingCost);
                if (overchargedShipping != null)
                {
                    IsOverchargedShipping = true;
                }
                var oversoldItems = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.OversoldItem
                    || n.Type == (int)OrderNotifyType.OversoldOnHoldItem);
                if (oversoldItems != null)
                {
                    HasOversoldItems = true;
                }
                var internationalExpressItems =
                    item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.InternationalExpress);
                if (internationalExpressItems != null)
                {
                    IsInternationalExpress = true;
                }
                var sameDayItems = item.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.SameDay);
                if (sameDayItems != null)
                {
                    IsSameDay = true;
                }
                var cancelationRequestCount =
                    item.Notifies.Count(n => n.Type == (int)OrderNotifyType.CancellationRequest);
                if (cancelationRequestCount > 0)
                {
                    HasCancelationRequest = true;
                    CancelationRequestCount = cancelationRequestCount;
                }

                Notifies = item.Notifies.Select(n => new OrderNotifyViewModel(n)).ToList();
            }
            else
            {
                Notifies = new List<OrderNotifyViewModel>();
            }

            #endregion


            #region Shippings

            HasBatchLabel = item.ShippingInfos.Any(sh => !String.IsNullOrEmpty(sh.TrackingNumber));

            var mainActiveShipping = item.ShippingInfos.FirstOrDefault(i => i.IsActive);
            var mainShipping = mainActiveShipping ?? item.ShippingInfos.FirstOrDefault(sh => sh.ShipmentProviderType == item.ShipmentProviderType);

            var activeShippings = item.ShippingInfos.Where(i => i.IsActive).ToList();

            EstDeliveryDate = activeShippings.Max(sh => sh.EstimatedDeliveryDate);

            if (item.MailInfos.Any(m => !m.LabelCanceled)
                && item.ShippingInfos.All(sh => String.IsNullOrEmpty(sh.LabelPath) || sh.LabelCanceled))
            {
                var mailShipping = item.MailInfos.OrderBy(m => m.LabelPurchaseDate).FirstOrDefault(l => !l.LabelCanceled);

                mainShipping = mailShipping;
                mainActiveShipping = mailShipping;
                activeShippings = new List<OrderShippingInfoDTO>() { mailShipping };

                IsInsured = mailShipping.IsInsured;
                IsSignConfirmation = mailShipping.IsSignConfirmation;
            }
            else
            {
                IsInsured = item.IsInsured;
                IsSignConfirmation = item.IsSignConfirmation;
            }

            StampsShippingCost = activeShippings.Sum(i => (i.StampsShippingCost ?? 0) + (!isFulfilmentUser ? (i.UpChargeCost ?? 0) : 0));
            InsuranceCost = activeShippings.Sum(i => i.InsuranceCost ?? 0);
            SignConfirmationCost = activeShippings.Sum(i => i.SignConfirmationCost ?? 0);

            ShippingGroupId = mainActiveShipping != null ? (int?)mainActiveShipping.ShippingGroupId : null;
            ShippingMethodId = mainActiveShipping != null && mainActiveShipping.ShippingMethod != null
                ? mainActiveShipping.ShippingMethod.Id : 0;
            ShippingMethodName = mainActiveShipping != null && mainActiveShipping.ShippingMethod != null
                ? ShippingUtils.PrepareMethodNameToDisplay(mainActiveShipping.ShippingMethod.ShortName, mainActiveShipping.DeliveryDaysInfo) : string.Empty;
            ShippingPackageName = mainActiveShipping != null && mainActiveShipping.ShippingMethod != null
                ? ShippingUtils.GetPackageName(ShippingUtils.GetPackageType(mainActiveShipping.ShippingMethodId)) : string.Empty;
            Selected = mainActiveShipping != null ? mainActiveShipping.ShippingGroupId : 0;
            FormattedShippingMethodName = GetShippingMethodName(activeShippings);

            ShipmentProviderType = mainShipping != null ? mainShipping.ShipmentProviderType : (int)Core.Models.Settings.ShipmentProviderType.Stamps;

            ShippingDate = activeShippings.Where(i => i.IsActive).Max(i => i.ShippingDate);

            //var currentAmazonServiceName = mainActiveShipping != null ?
            //    ShippingUtils.MethodNameToAmazonService(mainActiveShipping.ShippingMethod != null ? mainActiveShipping.ShippingMethod.Name : string.Empty)
            //    : ShippingUtils.InitialShippingServiceIncludeUpgrade(InitialServiceType, UpgradeLevel);
            var shipmentCarrier = mainShipping != null && mainShipping.ShippingMethod != null
                 ? mainShipping.ShippingMethod.CarrierName : String.Empty;

            if (StampsShippingCost == 0) //When no rates, default service name always "Standard", replace to initial
            {
                //currentAmazonServiceName = InitialServiceType;
                shipmentCarrier = String.Empty;
            }

            if (mainShipping != null && !String.IsNullOrEmpty(mainShipping.CustomCarrier))
            {
                shipmentCarrier = mainShipping.CustomCarrier;
                FormattedShippingMethodName = StringHelper.GetFirstNotEmpty(mainShipping.CustomShippingMethodName, "Standard");
                ShipmentProviderType = (int)Amazon.Core.Models.Settings.ShipmentProviderType.None;
            }

            ActualAmazonService = InitialServiceType;// currentAmazonServiceName;
            ShipmentCarrier = shipmentCarrier;
            ShippingTypeCode = (int)ShippingServiceUtils.AmazonServiceToUniversalServiceType(InitialServiceType);// currentAmazonServiceName);

            if (item.ShippingInfos.Count > 1
                && (mainActiveShipping == null || mainActiveShipping.LabelFromType == (int)LabelFromType.Batch))
            {

                var shippings = item.ShippingInfos.Select(sh => new OrderShippingViewModel(sh)).ToList();

                var packages = shippings
                    .Where(sh => sh.IsActive)
                    .OrderBy(sh => sh.ShippingInfoId)
                    .Select(p => new PackageViewModel()
                    {
                        ShippingId = p.ShippingInfoId,
                        PackageLength = p.PackageLength,
                        PackageWidth = p.PackageWidth,
                        PackageHeight = p.PackageHeight,
                    })
                    .ToList();

                ShippingOptions = GetShippingOptions(item.ShippingInfos,
                    (MarketType)Market,
                    item.IsSignConfirmation,
                    item.IsInsured,
                    isFulfilmentUser,
                    showOptionsPrices: true,
                    showProviderName: true);
            }

            #endregion


            #region Labels

            Labels = item.ShippingInfos.Where(i => (!String.IsNullOrEmpty(i.LabelPath)
                || i.IsActive //NOTE: in case when bag system shows user issue (keeped twice shippings with labels)
                ))
                .Select(sh => new LabelViewModel()
                {
                    Id = sh.Id,
                    Carrier = sh.ShippingMethod.CarrierName,
                    CustomCarrier = sh.CustomCarrier,
                    TrackingNumber = sh.TrackingNumber,
                    DeliveredStatus = sh.DeliveredStatus,
                    ActualDeliveryDate = sh.ActualDeliveryDate,
                    TrackingStateDate = sh.TrackingStateDate,
                    TrackingStatusSource = sh.TrackingStateSource,

                    ShippingCountry = item.FinalShippingCountry,
                    EstDeliveryDate = item.LatestDeliveryDate,

                    NumberInBatch = sh.NumberInBatch,
                    Path = sh.LabelPath,
                    PurchaseMessage = sh.LabelPurchaseMessage,
                    PurchaseResult = sh.LabelPurchaseResult,
                    PurchaseDate = sh.LabelPurchaseDate,
                    FromType = LabelFromType.Batch,
                    IsCanceled = sh.LabelCanceled
                })
                .ToList();

            Labels.AddRange(item.MailInfos
                .Where(l => !l.LabelCanceled)
                .Select(mi => new LabelViewModel()
                {
                    Id = mi.Id,
                    Carrier = mi.ShippingMethod.CarrierName,
                    TrackingNumber = mi.TrackingNumber,
                    DeliveredStatus = mi.DeliveredStatus,
                    ActualDeliveryDate = mi.ActualDeliveryDate,
                    TrackingStateDate = mi.TrackingStateDate,
                    TrackingStatusSource = mi.TrackingStateSource,

                    ShippingCountry = item.FinalShippingCountry,
                    EstDeliveryDate = item.LatestDeliveryDate,

                    Path = mi.LabelPath,
                    PurchaseMessage = mi.LabelPurchaseMessage,
                    PurchaseResult = mi.LabelPurchaseResult,
                    PurchaseDate = mi.LabelPurchaseDate,
                    FromType = LabelFromType.Mail,
                    MailReasonId = mi.MailReasonId,
                    IsCanceled = mi.LabelCanceled
                }));

            Labels = Labels.OrderBy(l => l.PurchaseDate).ToList();

            #endregion
        }

        #region Shipping Options

        public static List<DropDownListItem> GetShippingProviders(IList<ShipmentProviderDTO> shipmentProviders,
            MarketType market,
            string marketplaceId,
            string country,
            string sourceShippingService,
            int orderType)
        {
            var providerList = shipmentProviders;

            if (market == MarketType.Groupon)
            {
                providerList = providerList.Where(p => p.Type == (int)Core.Models.Settings.ShipmentProviderType.FedexOneRate
                    || p.Type == (int)Core.Models.Settings.ShipmentProviderType.Stamps).ToList();
            }
            else
            {
                var requiredAmazon = ShippingUtils.RequireAmazonProvider(orderType,
                           (int)market,
                           country,
                           sourceShippingService);

                if (requiredAmazon)
                {
                    providerList = providerList.Where(p => p.Type == (int)Core.Models.Settings.ShipmentProviderType.Amazon).ToList();
                }

                if (market == MarketType.Amazon
                    && !ShippingUtils.IsInternational(country))
                {
                    providerList = providerList.Where(p => p.Type != (int)Core.Models.Settings.ShipmentProviderType.FedexGeneral).ToList();
                }

                if (ShippingUtils.IsInternational(country))
                {
                    providerList = providerList.Where(p => p.Type != (int)Core.Models.Settings.ShipmentProviderType.FedexOneRate
                        && p.Type != (int)Core.Models.Settings.ShipmentProviderType.FedexSmartPost).ToList();
                }

                if ((market != MarketType.Amazon
                    && market != MarketType.AmazonEU
                    && market != MarketType.AmazonAU))
                {
                    providerList = providerList.Where(p => p.Type != (int)Core.Models.Settings.ShipmentProviderType.Amazon).ToList();
                }

                if (ShippingUtils.IsInternational(country)
                    && !(ShippingUtils.IsMexico(country))
                    && !(ShippingUtils.IsCanada(country) && marketplaceId == MarketplaceKeeper.AmazonComMarketplaceId))
                {
                    providerList = providerList.Where(p => p.Type != (int)Core.Models.Settings.ShipmentProviderType.Amazon).ToList();
                }

                if (ShippingUtils.IsMexico(country)
                    && market == MarketType.Amazon)
                {
                    providerList = providerList.Where(p => p.Type == (int)Core.Models.Settings.ShipmentProviderType.Amazon
                    //TASK: add Fedex to Amazon Mexico
                        || p.Type == (int)Core.Models.Settings.ShipmentProviderType.FedexGeneral).ToList();
                }

                if (!ShippingUtils.IsInternational(country))
                {
                    providerList = providerList.Where(p => p.Type != (int)Core.Models.Settings.ShipmentProviderType.Dhl
                        && p.Type != (int)Core.Models.Settings.ShipmentProviderType.IBC
                        && p.Type != (int)Core.Models.Settings.ShipmentProviderType.FIMS).ToList();
                }
            }

            return providerList.Where(p => p.Type != (int)Core.Models.Settings.ShipmentProviderType.StampsPriority)
                .Select(p => new DropDownListItem()
                {
                    Selected = false,
                    Text = StringHelper.GetFirstNotEmpty(p.ShortName, p.Name),
                    Value = p.Type.ToString()
                }).ToList();
        }


        public static string GetShippingMethodName(IList<OrderShippingInfoDTO> groupShippings)
        {
            var methods = groupShippings.GroupBy(sh => sh.ShippingMethod.ShortName).Select(s => new
            {
                Name = s.Key,
                DeliveryDays = s.Max(sh => sh.DeliveryDays ?? -1),
                DeliveryDaysInfo = s.Max(sh => sh.DeliveryDaysInfo),
                Value = s.Count()
            }).ToList();

            var text = "";
            if (groupShippings.Sum(o => o.StampsShippingCost) > 0
                || groupShippings.Any(sh => sh.ShipmentProviderType == (int)Core.Models.Settings.ShipmentProviderType.DhlECom))
            {
                if (groupShippings.Count() == 1)
                {
                    var methodName = ShippingUtils.PrepareMethodNameToDisplay(methods.First().Name, methods.First().DeliveryDaysInfo);
                    text = StringHelper.JoinTwo(", ",
                        text,
                        methodName);
                }
                else
                {
                    foreach (var method in methods)
                    {
                        text = StringHelper.JoinTwo(", ",
                            text,
                            method.Value + " x " + ShippingUtils.PrepareMethodNameToDisplay(method.Name, method.DeliveryDaysInfo));
                    }
                }
            }
            else
            {
                text = "[none]";
            }

            return text;
        }


        public static DTOOrder AttachOrderTo(IUnitOfWork db,
            long orderId,
            string toOrderString,
            DateTime when,
            long? by)
        {
            var dbOrder = db.Orders.Get(orderId);
            var dbToOrder = db.Orders.GetByCustomerOrderNumber(toOrderString);
            if (dbOrder != null)
            {
                if (dbToOrder != null)
                {
                    dbOrder.AttachedToOrderId = dbToOrder.Id;
                    dbOrder.AttachedToOrderString = dbToOrder.AmazonIdentifier;
                    dbOrder.AttachedToOrderDate = when;
                    dbOrder.AttachedToOrderBy = by;

                    db.Commit();

                    return new DTOOrder()
                    {
                        Id = dbOrder.Id,
                        AttachedToOrderId = dbToOrder.Id,
                        AttachedToOrderString = dbToOrder.AmazonIdentifier,
                    };
                }
                else
                {
                    dbOrder.AttachedToOrderId = null;
                    dbOrder.AttachedToOrderString = null;
                    dbOrder.AttachedToOrderDate = when;
                    dbOrder.AttachedToOrderBy = by;

                    db.Commit();
                }
            }

            return null;
        }

        public static List<SelectListShippingOption> GetShippingOptions(IList<OrderShippingInfoDTO> shippings, 
            MarketType market,
            bool isSignConfirmation,
            bool isInsured,
            bool isFulfilmentUser,
            bool showOptionsPrices = true,
            bool showProviderName = false)
        {
            var options = new List<SelectListShippingOption>();
            foreach (var group in shippings.GroupBy(i => i.ShippingGroupId).OrderBy(i => i.Sum(s => s.StampsShippingCost)))
            {
                var groupShippings = group.ToList();

                var text = GetShippingMethodName(groupShippings);
                var estDeliveryInfo = !String.IsNullOrEmpty(groupShippings.FirstOrDefault()?.DeliveryDaysInfo) ? (groupShippings.FirstOrDefault()?.DeliveryDaysInfo + " days") : "";

                if (group.Key == RateHelper.CustomPartialGroupId)
                    text = "C: " + text;

                text += " - " + "$" +
                    (groupShippings.All(o => o.StampsShippingCost.HasValue) ?
                        (groupShippings.Sum(o => o.StampsShippingCost + (!isFulfilmentUser ? (o.UpChargeCost ?? 0) : 0))
                            + (showOptionsPrices ? ShippingUtils.GetInsuranceInfo(groupShippings.Sum(o => isInsured ? (o.InsuranceCost ?? 0) : 0), "$") +
                                  ShippingUtils.GetSignInfo(groupShippings.Sum(o => isSignConfirmation ? (o.SignConfirmationCost ?? 0) : 0), "$")
                                : "")) : 
                            "n/a");

                text = StringHelper.Join(" - ", text, estDeliveryInfo);

                var providerName = ShipmentProviderHelper.GetShortName((ShipmentProviderType)groupShippings.FirstOrDefault().ShipmentProviderType);

                options.Add(new SelectListShippingOption()
                {
                    Selected = groupShippings.Max(o => o.IsActive),
                    Text = (showProviderName ? (providerName + ": ") : "") + text,
                    Value = group.Key.ToString(),
                    PackageCount = groupShippings.Count(),
                    RequiredPackageSize = groupShippings.Any(x => x.ShippingMethod.RequiredPackageSize),
                    Tag = groupShippings.Count().ToString()
                });
            }

            return options;
        }

        #endregion

        public static int GetFilteredForDisplayCount(IUnitOfWork db, OrderSearchFilterViewModel search, bool resetTime)
        {
            if (search == null)
            {
                search = OrderSearchFilterViewModel.Empty;
            }

            search = SetSearchModelProperties(search, resetTime);
            var filter = search.GetModel();
            return db.ItemOrderMappings
                .GetFilteredOrdersWithItems(null, filter)
                .Count();
        }

        public static GridResponse<OrderViewModel> GetFilteredForDisplay(IUnitOfWork db,
            ILogService log, 
            IWeightService weightService,
            OrderSearchFilterViewModel search, 
            bool isFulfilmentUser,
            SortMode sortOrder = SortMode.None,
            bool resetTime = true)
        {
            if (search == null)
            {
                search = OrderSearchFilterViewModel.Empty;
            }

            search = SetSearchModelProperties(search, resetTime);
            var filter = search.GetModel();
            filter.IncludeNotify = true;
            filter.IncludeMailInfos = true;
            filter.UnmaskReferenceStyles = true;
            filter.IncludeAllItems = true;
            filter.IncludeAllShippings = true;

            log.Info("Before GetDisplayOrdersWithItems");
            var rep = (ItemOrderMappingRepository)db.ItemOrderMappings;
            GridResponse<DTOOrder> searchResults = rep
                .GetDisplayOrdersWithItems(log, weightService, filter);
            log.Info("End GetDisplayOrdersWithItems");

            var items = new List<OrderViewModel>();
            
            if (search.FulfillmentChannel == FulfillmentChannelTypeEx.AFN)
            {
                UpdateItemsFBAAvailableQty(db, searchResults.Items);
                UpdateItemsFBAFee(db, searchResults.Items);
                SetSimilarSkuInfo(db, searchResults.Items);
            }

            searchResults.Items = SortHelper.Sort(searchResults.Items, sortOrder);
                
            foreach (var order in searchResults.Items)
            {
                var orderVm = new OrderViewModel(order, isFulfilmentUser);
                orderVm.Items = order.Items.Select(i => 
                    new OrderItemViewModel(i,
                        order.OnHold,
                        ShippingUtils.IsOrderPartial(order.OrderStatus))).ToList();
                //Update in case that the order mapped to listings from other inventory (ex.: ES, DE, FR, IT to UK inventory)
                orderVm.Items.ToList().ForEach(i =>
                {
                    i.Market = order.Market;
                    i.MarketplaceId = order.MarketplaceId;
                });
                items.Add(orderVm);
            }

            return new GridResponse<OrderViewModel>(items, searchResults.TotalCount);
        }

        public static GridResponse<OrderViewModel> GetFilteredForDisplayByPage(IUnitOfWork db,
            ILogService logger,
            IWeightService weightService,
            OrderSearchFilterViewModel search,
            bool isFulfilmentUser,
            SortMode sortOrder = SortMode.None,
            bool resetTime = true)
        {
            if (search == null)
            {
                search = OrderSearchFilterViewModel.Empty;
            }

            search = SetSearchModelProperties(search, resetTime);
            var filter = search.GetModel();
            filter.IncludeNotify = true;
            filter.IncludeMailInfos = true;
            filter.UnmaskReferenceStyles = true;
            filter.IncludeAllItems = true;

            GridResponse<DTOOrder> searchResult = db.ItemOrderMappings
                .GetDisplayOrdersWithItems(weightService, filter);

            var items = new List<OrderViewModel>();

            if (search.FulfillmentChannel == FulfillmentChannelTypeEx.AFN)
            {
                UpdateItemsFBAAvailableQty(db, searchResult.Items);
                UpdateItemsFBAFee(db, searchResult.Items);
                SetSimilarSkuInfo(db, searchResult.Items);
            }

            searchResult.Items = SortHelper.Sort(searchResult.Items, sortOrder);

            foreach (var order in searchResult.Items)
            {
                var orderVm = new OrderViewModel(order, isFulfilmentUser);
                orderVm.Items = order.Items.Select(i =>
                    new OrderItemViewModel(i,
                        order.OnHold,
                        ShippingUtils.IsOrderPartial(order.OrderStatus))).ToList();
                //Update in case that the order mapped to listings from other inventory (ex.: ES, DE, FR, IT to UK inventory)
                orderVm.Items.ToList().ForEach(i =>
                {
                    i.Market = order.Market;
                    i.MarketplaceId = order.MarketplaceId;
                });
                items.Add(orderVm);
            }

            return new GridResponse<OrderViewModel>(items, searchResult.TotalCount);
        }

        private static void UpdateItemsFBAAvailableQty(IUnitOfWork db, IList<DTOOrder> orders)
        {
            var invListings = db.ListingFBAInvs.GetAllActual();
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    var invListing = invListings.FirstOrDefault(l => String.Compare(l.SellerSKU, item.SKU, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (invListing != null)
                    {
                        item.AvailableQuantity = invListing.QuantityAvailable;
                    }
                }
            }
        }

        private static void UpdateItemsFBAFee(IUnitOfWork db, IList<DTOOrder> orders)
        {
            var estimatedFees = db.ListingFBAEstFees.GetAllActual();
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    if (!item.HasSettlementUpdate)
                    {
                        var estimatedFee = estimatedFees.FirstOrDefault(f => String.Compare(f.SKU, item.SKU, StringComparison.InvariantCultureIgnoreCase) == 0);
                        if (estimatedFee != null)
                        {
                            item.FBAPerOrderFulfillmentFee = estimatedFee.EstimatedOrderHandlingFeePerOrder;
                            item.FBAPerUnitFulfillmentFee = estimatedFee.EstimatedPickPackFeePerUnit;
                            item.FBAWeightBasedFee = estimatedFee.EstimatedWeightHandlingFeePerUnit;
                        }
                    }
                }
            }
        }

        private static void SetSimilarSkuInfo(IUnitOfWork db, IList<DTOOrder> orders)
        {
            var listings = db.Listings.GetFiltered(l => !l.IsRemoved).Select(l => new ItemDTO()
            {
                SKU = l.SKU,
                CurrentPrice = l.CurrentPrice
            }).ToList();
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    var similarNonFBASku = item.SKU.Replace("-FBA", "");
                    if (similarNonFBASku != item.SKU)
                    {
                        var nonFbaListing = listings.FirstOrDefault(l => l.SKU == similarNonFBASku);
                        if (nonFbaListing != null)
                        {
                            item.SimilarNonFBASKU = nonFbaListing.SKU;
                            item.SimilarNonFBAPrice = nonFbaListing.CurrentPrice;
                        }
                    }
                }
            }
        }

        private static OrderSearchFilterViewModel SetSearchModelProperties(OrderSearchFilterViewModel search, bool resetTime)
        {
            search = (OrderSearchFilterViewModel) GeneralUtils.StripNullStrings(search);

            if (resetTime)
            {
                search.DateFrom = DateHelper.SetBeginOfDay(search.DateFrom);
                search.DateTo = DateHelper.SetEndOfDay(search.DateTo);
            }
            return search;
        }

        public static CallMessagesResult<OrderViewModel> UpdateShippingInfo(IUnitOfWork db, 
            IOrderHistoryService orderHistoryService, IWeightService weightService,
            long id, 
            string orderId, 
            int groupId,
            long? by)
        {
            var messages = new List<MessageString>();
            int previousIsActiveGroupId = 0;
            try
            {
                var dbShippings = db.OrderShippingInfos.GetByOrderId(id).ToList();
                previousIsActiveGroupId = dbShippings.Where(sh => sh.IsActive).Select(sh => sh.ShippingGroupId).FirstOrDefault();
                var previousNumberInBatch = dbShippings.FirstOrDefault(sh => sh.IsActive)?.NumberInBatch;
                CheckPackageRequired(db, weightService, id, dbShippings, groupId);
                foreach (var shipping in dbShippings)
                {
                    shipping.IsActive = shipping.ShippingGroupId == groupId;
                    shipping.IsVisible = shipping.ShippingGroupId == groupId;

                    if (shipping.IsActive && previousNumberInBatch.HasValue)
                    {
                        shipping.NumberInBatch = previousNumberInBatch;
                        previousNumberInBatch = null;
                    }
                }
                db.Commit();

                var activeShipmentProviderId = dbShippings.FirstOrDefault(sh => sh.IsActive)?.ShipmentProviderType;
                var dbOrder = db.Orders.GetAll().FirstOrDefault(o => o.Id == id);
                if (dbOrder != null && activeShipmentProviderId.HasValue)
                    dbOrder.ShipmentProviderType = activeShipmentProviderId.Value;
                db.Commit();
            }
            catch (ApplicationException er)
            {
                messages.Add(new MessageString() { Message = er.Message, Status = MessageStatus.Error });                
            }
            var model = new OrderViewModel();
            var dtoShippings = db.OrderShippingInfos.GetByOrderIdAsDto(id);

            var activeService = dtoShippings.OrderBy(sh => sh.ShippingNumber).FirstOrDefault(i => i.IsActive);
            if (activeService != null && activeService.ShippingMethodId > 0)
            {
                var method = db.ShippingMethods.Get(activeService.ShippingMethodId);
                model.StampsShippingCost = dtoShippings.Where(s => s.IsActive).Sum(s => s.StampsShippingCost ?? 0);
                model.InsuranceCost = dtoShippings.Where(s => s.IsActive).Sum(s => s.InsuranceCost ?? 0);
                model.ShipmentProviderType = method.ShipmentProviderType;
                model.ShipmentCarrier = method.CarrierName;
                model.ShippingGroupId = messages.Any() ? previousIsActiveGroupId : groupId;
                model.ShippingMethodId = activeService.ShippingMethodId;
                model.ShippingMethodName = method != null ? ShippingUtils.PrepareMethodNameToDisplay(method.Name, activeService.DeliveryDaysInfo) : String.Empty;
                model.Selected = messages.Any() ? previousIsActiveGroupId : groupId;
                model.FormattedShippingMethodName = GetShippingMethodName(dtoShippings.Where(s => s.IsActive).ToList());                
            }

            if (!messages.Any())
            {
                orderHistoryService.AddRecord(id, "ShippingMethodId", previousIsActiveGroupId, null, groupId, model.ShippingMethodName, by);
            }

            return new CallMessagesResult<OrderViewModel>()
            {
                Data = model,
                Messages = messages.Any() ? messages : new List<MessageString>()
                {
                    new MessageString() { Message = "Success", Status = MessageStatus.Success }
                },
                Status = !messages.Any() ? CallStatus.Success : CallStatus.Fail
            };
        }

        private static void CheckPackageRequired(IUnitOfWork db, IWeightService ws, long orderId, List<OrderShippingInfo> allShippings, int groupId)
        {
            var groupShippings = allShippings.Where(x => x.ShippingGroupId == groupId).ToList();
            var methodIds = groupShippings.Select(x => x.ShippingMethodId).ToList();
            var methods = db.ShippingMethods.GetFiltered(x => methodIds.Contains(x.Id));
            bool requiredPackageSize = methods.Any(x => x.RequiredPackageSize);
            if (!requiredPackageSize)
            {
                return;
            }
            var orderItems = db.OrderItems.GetOrderPackageSizes(orderId).ToList();


            bool hasAllSizes = db.OrderShippingInfos.GetByOrderId(orderId).All(x => x.PackageWidth.HasValue && x.PackageWidth.Value != 0
            && x.PackageLength.HasValue && x.PackageLength.Value != 0
            && x.PackageHeight.HasValue && x.PackageHeight.Value != 0);

            if (!hasAllSizes)
            {
                hasAllSizes = orderItems.All(x => x.PackageWidth.HasValue && x.PackageWidth.Value != 0
                && x.PackageLength.HasValue && x.PackageLength.Value != 0
                && x.PackageHeight.HasValue && x.PackageHeight.Value != 0);
            }
            
            if (!hasAllSizes)
            {
                throw new ApplicationException("You cannot use this shipping method. Please set the Package Size first");
            }
        } 

        public static IList<string> UpgradeShippingService(IUnitOfWork db, 
            IWeightService weightService,
            IList<IShipmentApi> rateProviders,
            IList<long> orderIdList)
        {
            var failedUpdate = new List<string>();

            IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(weightService, orderIdList.ToArray(), includeSourceItems: true).ToList();
            IList<OrderShippingInfo> shippingInfoes = db.OrderShippingInfos.GetByOrderId(orderIdList).ToList();
            IList<ShippingMethodDTO> shippingMethodList = db.ShippingMethods.GetAllAsDto().ToList();

            foreach (var dtoOrder in dtoOrders)
            {
                var wasUpgraded = false;

                //Ignore shipped orders
                if (ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus)
                    || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath)))
                {
                    failedUpdate.Add(dtoOrder.OrderId);
                    continue;
                }

                if (dtoOrder.InitialServiceType == ShippingUtils.StandardServiceName)
                {
                    //var order = db.Orders.GetById(dtoOrder.Id);
                    //order.InitialServiceType = ShippingUtils.ExpeditedServiceName;
                    //db.Commit();

                    var shippingList = shippingInfoes
                            .Where(sh => sh.OrderId == dtoOrder.Id)
                            .ToList();

                   
                    //var fedexEnvelopeMethodId = dtoOrder.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId ? ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                    //    : ShippingUtils.FedexOneRate2DayEnvelope;
                    //var fedexPakMethodId = dtoOrder.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId ? ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId
                    //    : ShippingUtils.FedexOneRate2DayPak;
                    //var fedexEnvelope = shippingInfoes.FirstOrDefault(sh => sh.ShippingMethodId == fedexEnvelopeMethodId);
                    //var fedexPak = shippingInfoes.FirstOrDefault(sh => sh.ShipmentMethodId = fedexPakMethodId);

                    var activeShippings = shippingList.Where(sh => sh.IsActive).ToList();
                    if (activeShippings.All(sh => sh.ShippingMethodId == ShippingUtils.FirstClassShippingMethodId))
                    {
                        if (ShippingServiceUtils.IsSupportFedexEnvelope(OrderHelper.BuildAndGroupOrderItems(dtoOrder.Items))
                             && shippingList.Any(sh => sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope))
                        {
                            //activeShippings.ForEach(sh => sh.IsActive = false);
                            shippingList.ForEach(sh => sh.IsActive = sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope);
                            var dbOrder = db.Orders.Get(dtoOrder.Id);
                            dbOrder.ShipmentProviderType = (int)Amazon.Core.Models.Settings.ShipmentProviderType.FedexOneRate;
                            db.Commit();
                            dtoOrder.ShipmentProviderType = dbOrder.ShipmentProviderType;
                        }
                        else
                        {
                            if (ShippingServiceUtils.IsSupportFedexPak(OrderHelper.BuildAndGroupOrderItems(dtoOrder.Items))
                                 && shippingList.Any(sh => sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak))
                            {
                                //activeShippings.ForEach(sh => sh.IsActive = false);
                                shippingList.ForEach(sh => sh.IsActive = sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak);
                                var dbOrder = db.Orders.Get(dtoOrder.Id);
                                dbOrder.ShipmentProviderType = (int)Amazon.Core.Models.Settings.ShipmentProviderType.FedexOneRate;
                                db.Commit();
                                dtoOrder.ShipmentProviderType = dbOrder.ShipmentProviderType;
                            }
                        }
                        db.Commit();
                    }

                    if (activeShippings.All(sh => sh.ShippingMethodId == ShippingUtils.AmazonFirstClassShippingMethodId))
                    {
                        if (ShippingServiceUtils.IsSupportFedexEnvelope(OrderHelper.BuildAndGroupOrderItems(dtoOrder.Items))
                            && shippingList.Any(sh => sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId))
                        {
                            //activeShippings.ForEach(sh => sh.IsActive = false);
                            shippingList.ForEach(sh => sh.IsActive = sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId);
                        }
                        else
                        {
                            if (ShippingServiceUtils.IsSupportFedexPak(OrderHelper.BuildAndGroupOrderItems(dtoOrder.Items))
                                && shippingList.Any(sh => sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId))
                            {
                                //activeShippings.ForEach(sh => sh.IsActive = false);
                                shippingList.ForEach(sh => sh.IsActive = sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId);
                            }
                        }
                        db.Commit();
                    }
                }


                //var rateProvider = rateProviders.FirstOrDefault(r => (int)r.Type == dtoOrder.ShipmentProviderType);
                //if (rateProvider != null
                //    && rateProvider.CanUpgrade(dtoOrder.InitialServiceType,
                //    dtoOrder.UpgradeLevel ?? 0,
                //    dtoOrder.ShippingCountry))
                //{
                //    var rateOrderItems = dtoOrder.Items.Select(i => new OrderItemRateInfo()
                //    {
                //        Quantity = i.QuantityOrdered,
                //        ShippingSize = i.ShippingSize,
                //        ItemStyle = ItemStyleHelper.GetFromItemStyleOrTitle(i.ItemStyle, i.Title)
                //    }).ToList();
                //    var isSupportFlat = ShippingServiceUtils.IsSupportFlatEnvelop(rateOrderItems);

                //    var newActiveRate = rateProvider.UpgradeService(dtoOrder.InitialServiceType, isSupportFlat);
                //    var newActiveShippingMethod = shippingMethodList.FirstOrDefault(m => m.ServiceIdentifier == newActiveRate.ServiceIdentifier);

                //    if (newActiveShippingMethod != null)
                //    {
                //        var newActiveShippingList = shippingInfoes
                //            .Where(sh => sh.OrderId == dtoOrder.Id && sh.ShippingMethodId == newActiveShippingMethod.Id)
                //            .ToList();
                //        var oldActiveShippingList = shippingInfoes
                //            .Where(sh => sh.OrderId == dtoOrder.Id && sh.IsActive)
                //            .ToList();
                //        if (newActiveShippingList.Any())
                //        {
                //            //Apply changes
                //            foreach (var shipping in oldActiveShippingList)
                //            {
                //                shipping.IsActive = false;
                //                shipping.IsVisible = false;
                //            }
                //            foreach (var shipping in newActiveShippingList)
                //            {
                //                shipping.IsActive = true;
                //                shipping.IsVisible = true;
                //            }

                //            //Update into DB, after success update
                //            var order = db.Orders.GetById(dtoOrder.Id);
                //            order.UpgradeLevel = (dtoOrder.UpgradeLevel ?? 0) + 1;

                //            db.Commit();

                //            wasUpgraded = true;
                //        }
                //    }
                //}
                //if (!wasUpgraded)
                //{
                //    failedUpdate.Add(dtoOrder.OrderId);
                //}
            }

            return failedUpdate;
        }

        public static IList<string> DowngradeShippingService(IUnitOfWork db,
            IWeightService weightService,
            IList<IShipmentApi> rateProviders,
            IList<long> orderIdList)
        {
            var failedUpdate = new List<string>();

            IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(weightService, orderIdList.ToArray(), includeSourceItems: true).ToList();
            IList<OrderShippingInfo> shippingInfoes = db.OrderShippingInfos.GetByOrderId(orderIdList).ToList();
            IList<ShippingMethodDTO> shippingMethodList = db.ShippingMethods.GetAllAsDto().ToList();

            foreach (var dtoOrder in dtoOrders)
            {
                var wasUpgraded = false;

                //Ignore shipped orders
                if (ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus)
                    || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath)))
                {
                    failedUpdate.Add(dtoOrder.OrderId);
                    continue;
                }


                var rateProvider = rateProviders.FirstOrDefault(r => (int)r.Type == dtoOrder.ShipmentProviderType);
                if (rateProvider != null
                    && rateProvider.CanDowngrade(dtoOrder.InitialServiceType,
                    dtoOrder.UpgradeLevel ?? 0,
                    dtoOrder.ShippingCountry))
                {
                    var rateOrderItems = dtoOrder.Items.Select(i => new OrderItemRateInfo()
                    {
                        Quantity = i.QuantityOrdered,
                        ShippingSize = i.ShippingSize,
                        ItemStyle = ItemStyleHelper.GetFromItemStyleOrTitle(i.ItemStyle, i.Title)
                    }).ToList();
                    var isSupportFlat = ShippingServiceUtils.IsSupportFlatEnvelope(rateOrderItems);

                    var newActiveRate = rateProvider.DowngradeService(dtoOrder.InitialServiceType, isSupportFlat);
                    var newActiveShippingMethod = shippingMethodList.FirstOrDefault(m => m.ServiceIdentifier == newActiveRate.ServiceIdentifier);

                    if (newActiveShippingMethod != null)
                    {
                        var newActiveShippingList = shippingInfoes
                            .Where(sh => sh.OrderId == dtoOrder.Id && sh.ShippingMethodId == newActiveShippingMethod.Id)
                            .ToList();
                        var oldActiveShippingList = shippingInfoes
                            .Where(sh => sh.OrderId == dtoOrder.Id && sh.IsActive)
                            .ToList();
                        if (newActiveShippingList.Any())
                        {
                            //Apply changes
                            foreach (var shipping in oldActiveShippingList)
                            {
                                shipping.IsActive = false;
                                shipping.IsVisible = false;
                            }
                            foreach (var shipping in newActiveShippingList)
                            {
                                shipping.IsActive = true;
                                shipping.IsVisible = true;
                            }

                            //Update into DB, after success update
                            var order = db.Orders.GetById(dtoOrder.Id);
                            order.UpgradeLevel = (dtoOrder.UpgradeLevel ?? 0) - 1;

                            db.Commit();

                            wasUpgraded = true;
                        }
                    }
                }
                if (!wasUpgraded)
                {
                    failedUpdate.Add(dtoOrder.OrderId);
                }
            }

            return failedUpdate;
        }


        ////TODO: Make logic as in Upgrade
        //public static IList<string> DownGradeShippingService(IUnitOfWork db, 
        //    CompanyDTO company,
        //    ILogService log,
        //    ISyncInformer syncInfo,
        //    IList<IShipmentApi> rateProviders,
        //    ITime time,
        //    long[] orderIdList)
        //{
        //    var failedUpdate = new List<string>();
        //    IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(orderIdList, includeSourceItems: true).ToList();
        //    foreach (var dtoOrder in dtoOrders)
        //    {
        //        //Ignore shipped orders
        //        if (ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus)
        //            || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath)))
        //        {
        //            failedUpdate.Add(dtoOrder.OrderId);
        //            continue;
        //        }

        //        if (dtoOrder.UpgradeLevel.HasValue && dtoOrder.UpgradeLevel.Value > 0)
        //        {
        //            dtoOrder.UpgradeLevel = dtoOrder.UpgradeLevel - 1;

        //            var synchronizer = new AmazonOrdersSynchronizer(log,
        //                AccessManager.Company,
        //                syncInfo,
        //                rateProviders,
        //                company.GetReturnAddressDto(),
        //                company.GetPickupAddressDto(),
        //                time);

        //            if (!synchronizer.UIUpdate(db, dtoOrder, false, false, keepCustomShipping: false))
        //            {
        //                failedUpdate.Add(dtoOrder.OrderId);
        //            }
        //            else
        //            {
        //                //Update into DB, after success update
        //                var order = db.Orders.GetById(dtoOrder.Id);
        //                order.UpgradeLevel = dtoOrder.UpgradeLevel;
        //                db.Commit();
        //            }
        //        }
        //        else
        //        {
        //            failedUpdate.Add(dtoOrder.OrderId);
        //        }
        //    }

        //    return failedUpdate;
        //}


        public static void DismissAddressWarn(IUnitOfWork db, 
            IOrderHistoryService orderHistoryService,
            long id,
            DateTime when,
            long? by)
        {
            var dbOrder = db.Orders.Get(id);
            if (dbOrder != null)
            {
                orderHistoryService.AddRecord(id, OrderHistoryHelper.DismissAddressWarnKey, dbOrder.IsDismissAddressValidation, true, by);

                dbOrder.IsDismissAddressValidation = true;
                dbOrder.DismissAddressValidationBy = by;
                dbOrder.DismissAddressValidationDate = when;
                db.Commit();
            }
        }
    }
}