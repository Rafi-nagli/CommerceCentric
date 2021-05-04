using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Views
{
    public class ViewItem
    {
        [Key, Column(Order = 0)]
        public int Id { get; set; }

        [Key, Column(Order = 1)]
        public long? ListingEntityId { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? DropShipperId { get; set; }

        public int ItemPublishedStatus { get; set; }

        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public int? ParentId { get; set; }

        public string SourceMarketId { get; set; }

        public string Barcode { get; set; }
        public string Size { get; set; }
        public string ColorVariation { get; set; }

        public string Color { get; set; }
        public string Title { get; set; }

        public string ItemPicture { get; set; }
        public string LargePicture { get; set; }

        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }
        public bool? StyleOnHold { get; set; }

        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public bool? StyleItemOnHold { get; set; }
        public bool UseStyleImage { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long CompanyId { get; set; }


        public string SKU { get; set; }
        public bool IsDefault { get; set; }
        public bool IsFBA { get; set; }
        public bool IsPrime { get; set; }
        public decimal? EstimatedOrderHandlingFeePerOrder { get; set; }
        public decimal? EstimatedPickPackFeePerUnit { get; set; }
        public decimal? EstimatedWeightHandlingFeePerUnit { get; set; }

        public string OnMarketTemplateName { get; set; }

        public string ListingId { get; set; }

        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }

        public bool OnHold { get; set; }
        public DateTime? AutoQuantityUpdateDate { get; set; }
        public DateTime? RestockDate { get; set; }

        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }


        public long? SaleId { get; set; }

        public bool IsManualPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? BusinessPrice { get; set; }
        public decimal? AutoAdjustedPrice { get; set; }

        public double Weight { get; set; }

        public bool? IsAmazonParentASIN { get; set; }
        public bool? IsExistOnAmazon { get; set; }
        public DateTime? OpenDate { get; set; }
        public int? AmazonRealQuantity { get; set; }
        public decimal? AmazonCurrentPrice { get; set; }
    }
}
