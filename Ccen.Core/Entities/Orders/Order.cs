using System;
using System.ComponentModel.DataAnnotations.Schema;
using Amazon.Core.Contracts;

namespace Amazon.Core.Entities
{
    public class Order : BaseDateAndByEntity, IAuditable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int Market { get; set; }
        public string SalesChannel { get; set; }
        public string FulfillmentChannel { get; set; }
        public string MarketplaceId { get; set; }
        public string AmazonIdentifier { get; set; }
        public string MarketOrderId { get; set; }
        public string SalesRecordNumber { get; set; }
        public string CustomerOrderId { get; set; }

        public bool AutoDSSelection { get; set; }
        public long? DropShipperId { get; set; }
        public bool SentToDropShipper { get; set; }
        public DateTime? SentToDropShipperDate { get; set; }
        public string DSOrderNumber { get; set; }

        public int? SubOrderNumber { get; set; }
        public decimal? SubOrderAmountPercent { get; set; }

        public int OrderType { get; set; }
        public bool IsAcknowledged { get; set; }


        public string OrderStatus { get; set; }
        public string SourceOrderStatus { get; set; }

        public int Quantity { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentInfo { get; set; }


        public decimal? TotalPaid { get; set; }
        public decimal? ShippingPaid { get; set; }

        public decimal TotalPrice { get; set; }
        public string TotalPriceCurrency { get; set; }

        public decimal? TaxAmount { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? ShippingPrice { get; set; }
        public decimal? ShippingDiscountAmount { get; set; }
        public decimal? ShippingTaxAmount { get; set; }

        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountTax { get; set; }
        public string DiscountDesc { get; set; }

        public DateTime? OverrideOrderDate { get; set; }
        public DateTime? OrderDate { get; set; }

        public long? CustomerId { get; set; }

        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }

        public string ShippingAddressSourceHash { get; set; }
        public bool? ShippingAddressIsResidential { get; set; }
        public string PersonName { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingZipAddon { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingPhoneNumeric { get; set; }

        public bool IsManuallyUpdated { get; set; }
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


        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string BillingZipAddon { get; set; }

        public string BillingPersonName { get; set; }
        public string BillingPhone { get; set; }
        public string BillingEmail { get; set; }


        public decimal? InsuredValue { get; set; }
        public bool IsInsured { get; set; }

        public bool IsSignConfirmation { get; set; }

        public bool IsDismissAddressValidation { get; set; }
        public long? DismissAddressValidationBy { get; set; }
        public DateTime? DismissAddressValidationDate { get; set; }
        public int AddressValidationStatus { get; set; }

        public long? AttachedToOrderId { get; set; }
        public string AttachedToOrderString { get; set; }
        public DateTime? AttachedToOrderDate { get; set; }
        public long? AttachedToOrderBy { get; set; }



        public string AmazonEmail { get; set; }

        public string InitialServiceType { get; set; }
        public string SourceShippingService { get; set; }
        public int? UpgradeLevel { get; set; }
        public int ShipmentProviderType { get; set; }

        public DateTime? EarliestShipDate { get; set; }
        public DateTime? LatestShipDate { get; set; }

        public DateTime? EstDeliveryDate { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }


        public DateTime? SourceShippedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? MarketLastUpdatedDate { get; set; }

        public bool IsFeedbackRequested { get; set; }
        public DateTime? FeedbackRequestDate { get; set; }
        public DateTime? AddressVerifyRequestDate { get; set; }

        public int CheckedTimes { get; set; }
        public DateTime? CheckedDate { get; set; }

        public int ShippingCalculationStatus { get; set; }
        public bool OnHold { get; set; }
        public DateTime? OnHoldUpdateDate { get; set; }
        public long? BatchId { get; set; }
        public bool? IsForceVisible { get; set; }
        public bool? IsRefundLocked { get; set; }
        public string RefundDesc { get; set; }

        public int? SignifydStatus { get; set; }
        public string SignifydDesc { get; set; }
    }
}
