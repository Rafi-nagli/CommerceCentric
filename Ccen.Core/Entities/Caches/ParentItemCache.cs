using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Caches
{
    public class ParentItemCache : BaseCacheEntity
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

        public string PositionsInfo { get; set; }

        public DateTime? LastOpenDate { get; set; }
    }
}
