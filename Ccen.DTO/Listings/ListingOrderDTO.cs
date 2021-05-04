
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.DTO
{
    public class ListingOrderDTO
    {
        public long Id { get; set; }

        public string ListingId { get; set; }
        
        public int ItemId { get; set; }
        
        public string Barcode { get; set; }

        public string ASIN { get; set; }
        public int Market { get; set; } 
        public string MarketplaceId { get; set; }

        public long? DropShipperId { get; set; }

        public int? Rank { get; set; }


        public string StyleID { get; set; }
        public long? StyleId { get; set; }
        public string OriginalStyleString { get; set; }

        public int ReplaceType { get; set; }

        public string ItemStatus { get; set; }
        public DateTime? ItemStatusDate { get; set; }

        public string ParentSourceMarketId { get; set; }
        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public string OrderNumber { get; set; }
        public long? OrderId { get; set; }
        public string ItemOrderId { get; set; }
        public string SourceItemOrderIdentifier { get; set; }

        public long OrderItemEntityId { get; set; }
        
        public string RecordNumber { get; set; }

        public long? SourceListingId { get; set; }
        
        public decimal ItemGrandPrice { get; set; } //TODO: Replace to ItemPaid

        public decimal? ItemTotal { get; set; }
        public decimal? ItemPaid { get; set; }
        public decimal? ShippingPaid { get; set; }

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

        
        /// <summary>
        /// Amazon = FBA Available Quantity; eBay = 
        /// </summary>
        public int? AvailableQuantity { get; set; }

        //FBA Fee
        public decimal? FBAPerOrderFulfillmentFee { get; set; }
        public decimal? FBAPerUnitFulfillmentFee { get; set; }
        public decimal? FBAWeightBasedFee { get; set; }
        public decimal? Commission { get; set; }
        public bool HasSettlementUpdate { get; set; }


        //Related Non FBA Listing
        public string SimilarNonFBASKU { get; set; }
        public decimal? SimilarNonFBAPrice { get; set; }


        
        public string Picture { get; set; }
        public string ItemPicture { get; set; }

        public int QuantityOrdered { get; set; }
        public int QuantityShipped { get; set; }
        public int QuantityRefunded { get; set; }
        public string SKU { get; set; }
        public double? Weight { get; set; }

        public string Size { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleImage { get; set; }
        public decimal? MSRP { get; set; }

        public bool UseStyleImage { get; set; }

        //public decimal? Cost { get; set; }
        //public string OriginalStyleString { get; set; }
        public long? DSItemId { get; set; }
        public decimal? DSCost { get; set; }
        public string DSSKU { get; set; }
        public string DSModel { get; set; }


        public string SourceStyleString { get; set; }
        public long? SourceStyleItemId { get; set; }
        public string SourceStyleSize { get; set; }
        public string SourceStyleColor { get; set; }

        public int? FeedIndex { get; set; }

        public string Color { get; set; }
        public string ParentASIN { get; set; }
        public string Title { get; set; }
        public DateTime? ListingCreateDate { get; set; }

        public int? RealQuantity { get; set; }

        public string ItemStyle { get; set; }
        public string ShippingSize { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
        public string InternationalPackage { get; set; }
        public string ExcessiveShipmentThreshold { get; set; }
        public DateTime? RestockDate { get; set; }
        public DateTime? FulfillDate { get; set; }
        public DateTime? PreOrderExpReceiptDate { get; set; }

        public bool IsPrime { get; set; }

        public long? LocationIndex { get; set; }

        public string OnMarketTemplateName { get; set; }

        public IList<StyleLocationDTO> Locations { get; set; }

        public StyleLocationDTO DefaultLocation
        {
            get
            {
                return Locations != null && Locations.Any() ? Locations.OrderByDescending(l => l.IsDefault).First() : null;
            }
        }

        public int SortIsle { get { return DefaultLocation != null ? DefaultLocation.SortIsle : int.MaxValue; } }
        public int SortSection { get { return DefaultLocation != null ? DefaultLocation.SortSection : int.MaxValue; } }
        public int SortShelf { get { return DefaultLocation != null ? DefaultLocation.SortShelf : int.MaxValue; } }

        public long? SortLocationIndex { get { return LocationIndex; } }

        //Navitation
        public long ShippingInfoId { get; set; }

        //Additional
        public decimal NetItemPaid { get; set; }
    }
}
