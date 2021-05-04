using Amazon.DTO.Customers;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class DTOMarketOrder
    {
        public long Id { get; set; }
        public int Market { get; set; }
        public string SalesChannel { get; set; }
        public string FulfillmentChannel { get; set; }

        public string MarketplaceId { get; set; }

        public int OrderType { get; set; }


        public string OrderId { get; set; }
        public string MarketOrderId { get; set; }
        public string CustomerOrderId { get; set; }
        public string SalesRecordNumber { get; set; }

        public bool AutoDSSelection { get; set; }
        public long? DropShipperId { get; set; }
        public string DropShipperName { get; set; }
        public bool SentToDropShipper { get; set; }

        public int? SubOrderNumber { get; set; }
        public decimal? SubOrderAmountPercent { get; set; }

        public string OrderStatus { get; set; }
        public string SourceOrderStatus { get; set; }

        public int Quantity { get; set; }
        public long ShippingInfoId { get; set; }
        public int ShippingInfoType { get; set; }
        public int ShippingCalculationStatus { get; set; }

        public string ShippingService { get; set; }
        public string InitialServiceType { get; set; }
        public string SourceShippingService { get; set; }

        public int ShipmentProviderType { get; set; }


        public DateTime? SourceShippedDate { get; set; }
        public DateTime? PaidDate { get; set; }

        public int? UpgradeLevel { get; set; }

        public long? CustomerId { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string PersonName { get; set; }



        #region Shipping Address
        public int? ShippingAddressId { get; set; }

        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingZipAddon { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingPhoneNumeric { get; set; }

        public string ShippingAddressType { get; set; }

        public string MarketCustomerId { get; set; }
        #endregion

        public bool IsResidential { get; set; }

        public bool IsManuallyUpdated { get; set; }

        #region Manual Shipping Address
        public string ManuallyPersonName { get; set; }
        public string ManuallyShippingAddress1 { get; set; }
        public string ManuallyShippingAddress2 { get; set; }
        public string ManuallyShippingCity { get; set; }
        public string ManuallyShippingState { get; set; }
        public string ManuallyShippingCountry { get; set; }
        public string ManuallyShippingZip { get; set; }
        public string ManuallyShippingZipAddon { get; set; }
        public string ManuallyShippingPhone { get; set; }
        public string ManuallyShippingPhoneNumeric { get; set; }
        #endregion

        #region Billing Address

        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingState { get;set; }
        public string BillingZip { get; set; }
        public string BillingZipAddon { get; set; }

        public string BillingPersonName { get; set; }
        public string BillingPhone { get; set; }
        public string BillingEmail { get; set; }

        #endregion

        public decimal? InsuredValue { get; set; }
        public bool IsInsured { get; set; }

        public bool IsSignConfirmation { get; set; }

        public string LastCommentMessage { get; set; }
        public DateTime? LastCommentDate { get; set; }
        public long? LastCommentBy { get; set; }
        public string LastCommentByName { get; set; }
        public long? LastCommentNumber { get; set; }


        public int? SignifydStatus { get; set; }
        public string SignifydDesc { get; set; }

        public int AddressValidationStatus { get; set; }
        public bool IsDismissAddressValidation { get; set; }
        public DateTime? DismissAddressValidationDate { get; set; }
        public DateTime? AddressVerifyRequestDate { get; set; }

        public long? AttachedToOrderId { get; set; }
        public string AttachedToOrderString { get; set; }

        public DateTime? ShipDate { get; set; }
        public string AmazonEmail { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentInfo { get; set; }

        public string TotalPriceCurrency { get; set; }

        public decimal? TotalPaid { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal? TotalRefunded { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxRate { get; set; }


        public decimal? ShippingPaid { get; set; }

        public decimal? ShippingPrice { get; set; }
        public decimal? ShippingDiscountAmount { get; set; }
        public decimal? ShippingTaxAmount { get; set; } 
        public decimal? ShippingRefunded { get; set; }

        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountTax { get; set; }
        public string DiscountDesc { get; set; }


        public decimal? SourceTotalPaid { get; set; }

        public decimal SourceTotalPrice { get; set; }
        public decimal? SourceTaxAmount { get; set; }

        public decimal? SourceShippingPaid { get; set; }

        public decimal? SourceShippingPrice { get; set; }
        public decimal? SourceShippingDiscountAmount { get; set; }
        public decimal? SourceShippingTaxAmount { get; set; }

        public decimal? SourceDiscountAmount { get; set; }
        public decimal? SourceDiscountTax { get; set; }


        public DateTime? OrderDate { get; set; }
        public DateTime? MarketLastUpdatedDate { get; set; }

        public bool OnHold { get; set; }
        public DateTime? OnHoldUpdateDate { get; set; }
        public double WeightD { get; set; }


        public bool IsFeedbackRequested { get; set; }
        public DateTime? FeedbackRequestDate { get; set; }

        public bool NotDeliveredDismiss { get; set; }
        public bool NotDeliveredSubmittedClaim { get; set; }
        public bool NotDeliveredHighlight { get; set; }


        public string Timezone { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }

        public int? TrackingStateSource { get; set; }
        public string TrackingStateEvent { get; set; }
        public DateTime? TrackingStateDate { get; set; }

        public long? BatchId { get; set; }
        public string BatchName { get; set; }

        public bool? IsForceVisible { get; set; }
        public bool? IsRefundLocked { get; set; }

        public DateTime? EarliestShipDate { get; set; }
        public DateTime? LatestShipDate { get; set; }

        public DateTime? EarliestDeliveryDate { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }


        public IList<OrderNotifyDto> Notifies { get; set; }


        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        #region Final Shipping Address

        public string FinalPersonName
        {
            get
            {
                if (!String.IsNullOrEmpty(ManuallyPersonName)
                    && IsManuallyUpdated)
                    return ManuallyPersonName;
                return PersonName;
            }
        }

        public string FinalShippingAddress1
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingAddress1 : ShippingAddress1;
            }
        }

        public string FinalShippingAddress2
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingAddress2 : ShippingAddress2;
            }
        }

        public string FinalShippingCity
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingCity : ShippingCity;
            }
        }

        public string FinalShippingState
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingState : ShippingState;
            }
        }

        public string FinalShippingCountry
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingCountry : ShippingCountry;
            }
        }

        public string FinalShippingZip
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingZip : ShippingZip;
            }
        }

        public string FinalShippingZipAddon
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingZipAddon : ShippingZipAddon;
            }
        }

        public string FinalShippingPhone
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingPhone : ShippingPhone;
            }
        }
        #endregion
        
        public AddressDTO GetAddressDto()
        {
            return new AddressDTO()
            {
                FullName = FinalPersonName,
                Address1 = FinalShippingAddress1,
                Address2 = FinalShippingAddress2,
                City = FinalShippingCity,
                State = FinalShippingState,
                Country = FinalShippingCountry,
                Zip = FinalShippingZip,
                ZipAddon = FinalShippingZipAddon,
                Phone = FinalShippingPhone,

                BuyerEmail = BuyerEmail,
                IsResidential = IsResidential,
            };
        }

        public CustomerDTO GetCustomerInfo()
        {
            return new CustomerDTO()
            {
                Id = CustomerId,
                Name = BuyerName,
                Email = BuyerEmail,

                Address1 = FinalShippingAddress1,
                Address2 = FinalShippingAddress2,
                City = FinalShippingCity,
                State = FinalShippingState,
                Country = FinalShippingCountry,
                Zip = FinalShippingZip,
                ZipAddon = FinalShippingZipAddon,
                Phone = FinalShippingPhone,

                CreateDate = OrderDate,
            };
        }


        public MarketIdentifier GetMarketId()
        {
            return new MarketIdentifier(Market, MarketplaceId);
        }
    }
}
