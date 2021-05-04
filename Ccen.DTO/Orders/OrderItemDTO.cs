using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class OrderItemDTO
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public string ItemOrderIdentifier { get; set; }
        public string SourceItemOrderIdentifier { get; set; }

        public long ListingId { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleString { get; set; }
        public int ReplaceType { get; set; }

        public string SKU { get; set; }

        public long? SourceListingId { get; set; }
        public string RecordNumber { get; set; } //NOTE: Sub orders for eBay, need to review ??

        public decimal ItemPrice { get; set; }
        public string ItemPriceCurrency { get; set; }
        public decimal? ItemPriceInUSD { get; set; }

        public decimal? ItemPaid { get; set; }

        public double? Weight { get; set; }

        public decimal? PromotionDiscount { get; set; }
        public string PromotionDiscountCurrency { get; set; }
        public decimal? PromotionDiscountInUSD { get; set; }

        public decimal ShippingPrice { get; set; }
        public string ShippingPriceCurrency { get; set; }
        public decimal? ShippingPriceInUSD { get; set; }

        public decimal? ShippingDiscount { get; set; }
        public string ShippingDiscountCurrency { get; set; }
        public decimal? ShippingDiscountInUSD { get; set; }
        

        public int QuantityOrdered { get; set; } //From Quantity

        public int QuantityShipped { get; set; }
    }
}
