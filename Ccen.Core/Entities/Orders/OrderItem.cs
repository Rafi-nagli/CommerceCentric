using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class OrderItem : BaseDateEntity
    {
        [Key]
        public long Id { get; set; }

        public long OrderId { get; set; }
        public long? LinkedFromOrderId { get; set; }

        public long? OverrideDropShipperId { get; set; }
        public DateTime? OverrideOrderDate { get; set; }

        public string ItemOrderIdentifier { get; set; }
        public string SourceItemOrderIdentifier { get; set; }

        public long ListingId { get; set; }
        
        //NOTE: Keep styleItemId which used when sold item
        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }

        public int ReplaceType { get; set; }
        public DateTime? ReplaceDate { get; set; }

        public long? SourceListingId { get; set; }

        //NOTE: Only for fast shown original info on Order Page
        public string SourceStyleString { get; set; }
        public long? SourceStyleItemId { get; set; }
        public string SourceStyleSize { get; set; }
        public string SourceStyleColor { get; set; }

        /// <summary>
        /// NOTE: Sub orders for eBay, need to review ??
        /// </summary>
        public string RecordNumber { get; set; }

        public long? DSItemId { get; set; }
        public decimal? DSCost { get; set; }

        /// <summary>
        /// NOTE:Each order item has own status on Walmart 
        /// </summary>
        public string ItemStatus { get; set; }
        public DateTime? ItemStatusDate { get; set; }

        public decimal? ItemPaid { get; set; }
        public decimal? ShippingPaid { get; set; }
        public decimal? ItemTotal { get; set; }

        public decimal ItemGrandPrice { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal? ItemTax { get; set; }
        public string ItemPriceCurrency { get; set; }
        public decimal? ItemPriceInUSD { get; set; }

        public decimal? RefundItemPrice { get; set; }
        public decimal? RefundItemPriceInUSD { get; set; }

        public decimal? PromotionDiscount { get; set; }
        public string PromotionDiscountCurrency { get; set; }
        public decimal? PromotionDiscountInUSD { get; set; }

        public decimal ShippingPrice { get; set; }
        public decimal? ShippingTax { get; set; }
        public string ShippingPriceCurrency { get; set; }
        public decimal? ShippingPriceInUSD { get; set; }

        public decimal? RefundShippingPrice { get; set; }
        public decimal? RefundShippingPriceInUSD { get; set; }

        public decimal? ShippingDiscount { get; set; }
        public string ShippingDiscountCurrency { get; set; }
        public decimal? ShippingDiscountInUSD { get; set; }
        
        public int QuantityOrdered { get; set; } 
        public int QuantityShipped { get; set; }
        public int QuantityRefunded { get; set; }
    }
}
