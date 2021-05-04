using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.Cache;
using Amazon.DTO.Listings;

namespace Amazon.Core.Models.Caches
{
    public class StyleItemInfoCacheEntry : IDbCacheableEntry
    {
        public long Id { get; set; }
        public string ParentASIN { get; set; }

        public string ASIN { get; set; }

        public string SKU { get; set; }

        public long? StyleId { get; set; }
        public string StyleString { get; set; }


        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public bool StyleOnHold { get; set; }
        public long? StyleItemId { get; set; }

        public bool IsFBA { get; set; }
        public bool OnHold { get; set; }

        public string StyleUrl { get; set; }

        public decimal? CurrentPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }

        public decimal? AmazonCurrentPrice { get; set; }
        public int? AmazonRealQuantity { get; set; }


        public int? RemainingQuantity { get; set; }

        public string Image { get; set; }

        public string MarketplacesInfo { get; set; }

        public bool IsRemoved { get; set; }

        public IList<ListingDefectDTO> ListingDefects { get; set; }

        public long Key
        {
            get { return Id; }
        }

        public bool Deleted
        {
            get { return IsRemoved; }
        }
    }
}
