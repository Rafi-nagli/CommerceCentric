using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class OrderItemSource : BaseDateEntity
    {
        [Key]
        public long Id { get; set; }

        public long OrderId { get; set; }
        public string ItemOrderIdentifier { get; set; }

        public long ListingId { get; set; }

        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleString { get; set; }
        
        /// <summary>
        /// NOTE: Sub orders for eBay, need to review ??
        /// </summary>
        public string RecordNumber { get; set; } 

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

        //public decimal? ItemTax { get; set; }
        //public string ItemTaxCurrency { get; set; }
        //public decimal? ItemTaxInUSD { get; set; }

        public decimal? PromotionDiscount { get; set; }
        public string PromotionDiscountCurrency { get; set; }
        public decimal? PromotionDiscountInUSD { get; set; }

        public decimal ShippingPrice { get; set; }
        public decimal? ShippingTax { get; set; }
        public string ShippingPriceCurrency { get; set; }
        public decimal? ShippingPriceInUSD { get; set; }

        public decimal? ShippingDiscount { get; set; }
        public string ShippingDiscountCurrency { get; set; }
        public decimal? ShippingDiscountInUSD { get; set; }

        //public decimal? ShippingTax { get; set; }
        //public string ShippingTaxCurrency { get; set; }
        //public decimal? ShippingTaxInUSD { get; set; }


        //public decimal? GiftWrapPrice { get; set; }
        //public string GiftWrapPriceCurrency { get; set; }
        //public decimal? GiftWrapPriceInUSD { get; set; }

        //public decimal? GiftWrapTax { get; set; }
        //public string GiftWrapTaxCurrency { get; set; }
        //public decimal? GiftWrapTaxInUSD { get; set; }


        //public decimal? CODFee { get; set; }
        //public string CODFeeCurrency { get; set; }
        //public decimal? CODFeeInUSD { get; set; }

        //public decimal? CODFeeDiscount { get; set; }
        //public string CODFeeDiscountCurrency { get; set; }
        //public decimal? CODFeeDiscountInUSD { get; set; }


        //public decimal? TotalFee { get; set; }
        //public string TotalFeeCurrency { get; set; }
        //public decimal? TotalFeeInUSD { get; set; }

        public int QuantityOrdered { get; set; } //From Quantity

        public int QuantityShipped { get; set; }


    }
}
