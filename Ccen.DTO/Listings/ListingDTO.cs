using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class ListingDTO
    {
        public long Id { get; set; }
        public string ListingId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public bool IsFBA { get; set; }
        public bool IsPrime { get; set; }
        public string SKU { get; set; }
        public int ItemId { get; set; }
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string SourceMarketId { get; set; }

        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        
        public int Quantity { get; set; }
        public int SoldQuantity { get; set; }
        public int? DisplayQuantity { get; set; }
        public bool OnHold { get; set; }
        public DateTime? RestockDate { get; set; }


        public int? Rank { get; set; }

        public decimal Price { get; set; }

        public string ListingColor { get; set; }
        public string ListingSize { get; set; }

        //Additional
        public string ItemPicture { get; set; }
        public double? Weight { get; set; }

        public decimal? LowestPrice { get; set; }
        public DateTime? LowestPriceUpdateDate { get; set; }
    }
}
