using System;

namespace Amazon.DTO.Caches
{
    public class ParentItemCacheDTO : BaseCacheDto
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public int DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }

        public bool HasListings { get; set; }
        public bool? HasChildWithFakeParentASIN { get; set; }
        public bool? HasQtyDifferences { get; set; }
        public bool? HasPriceDifferences { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? LastSoldDate { get; set; }

        public DateTime? LastOpenDate { get; set; }

        public string PositionsInfo { get; set; }
    }
}
