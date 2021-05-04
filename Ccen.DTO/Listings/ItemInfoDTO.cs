using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Listings;

namespace Amazon.DTO
{
    public class ItemInfoDTO
    {
        public string Size { get; set; }
        public string ColorVariation { get; set; }

        public string Color { get; set; }

        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string SKU { get; set; }

        public long? StyleId { get; set; }
        public string StyleString { get; set; }

        public long? StyleItemId { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }

        public bool IsFBA { get; set; }
        public string StyleUrl { get; set; }

        public string ItemPicture { get; set; }

        public decimal? CurrentPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        
        public decimal? FBAFee { get; set; }

        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }

        public decimal? AmazonCurrentPrice { get; set; }
        public int? AmazonRealQuantity { get; set; }


        public int? ListingSoldQuantity { get; set; }

        public int? MarketSoldQuantity { get; set; }
        public int? ScannedSoldQuantity { get; set; }
        public int? SentToFBAQuantity { get; set; }
        public int? SpecialCaseQuantity { get; set; }

        public int? TotalMarketSoldQuantity { get; set; }
        public int? TotalScannedSoldQuantity { get; set; }
        public int? TotalSentToFBAQuantity { get; set; }
        public int? TotalSpecialCaseQuantity { get; set; }
        public int? TotalSaleEventQuantity { get; set; }

        public int? InventoryQuantity { get; set; }
        public int? RemainingQuantity { get; set; }

        public int LinkedListingCount { get; set; }

        public DateTime? LastOrder { get; set; }

        public DateTime? OpenDate { get; set; }
        public DateTime? CreateDate { get; set; }


        public IList<ListingDefectDTO> ListingDefects { get; set; }
    }
}
