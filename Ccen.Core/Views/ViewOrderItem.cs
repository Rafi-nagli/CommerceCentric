using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewOrderItem
    {
        [Key, Column(Order = 0)]
        public long Id { get; set; }

        public long OrderId { get; set; }
        public string ItemOrderIdentifier { get; set; }
        public string SourceItemOrderIdentifier { get; set; }

        public string RecordNumber { get; set; }

        public long ListingId { get; set; }
        public long? SourceListingId { get; set; }

        public decimal ItemGrandPrice { get; set; }
        public decimal ItemPaid { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemPrice { get; set; }
        public string ItemPriceCurrency { get; set; }
        public decimal? ItemPriceInUSD { get; set; }

        public decimal? RefundItemPrice { get; set; }
        public decimal? RefundItemPriceInUSD { get; set; }
        
        public decimal? PromotionDiscount { get; set; }
        public string PromotionDiscountCurrency { get; set; }
        public decimal? PromotionDiscountInUSD { get; set; }


        public decimal ShippingPrice { get; set; }
        public string ShippingPriceCurrency { get; set; }
        public decimal? ShippingPriceInUSD { get; set; }

        public decimal? ShippingPaid { get; set; }
        public decimal? ShippingTax { get; set; }

        public decimal? RefundShippingPrice { get; set; }
        public decimal? RefundShippingPriceInUSD { get; set; }
        
        public decimal? ShippingDiscount { get; set; }
        public string ShippingDiscountCurrency { get; set; }
        public decimal? ShippingDiscountInUSD { get; set; }

        
        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public string StyleImage { get; set; }
        public decimal? MSRP { get; set; }
        //public decimal? Cost { get; set; }
        public string OriginalStyleString { get; set; }

        public string SourceStyleString { get; set; }
        public long? SourceStyleItemId { get; set; }
        public string SourceStyleSize { get; set; }
        public string SourceStyleColor { get; set; }
        
        public float? Weight { get; set; }
        public string ShippingSize { get; set; }

        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public string InternationalPackage { get; set; }
        public string ExcessiveShipment { get; set; }
        public string ItemStyle { get; set; }        

        public DateTime? RestockDate { get; set; }

        public int ReplaceType { get; set; }

        public int QuantityOrdered { get; set; } //From Quantity
        public int QuantityShipped { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
