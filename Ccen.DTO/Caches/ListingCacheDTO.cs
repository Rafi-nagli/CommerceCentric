using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Caches
{
    public class ListingCacheDTO : BaseCacheDto
    {
        public string MarketplaceId { get; set; }
        public int ItemId { get; set; }
        public int SoldQuantity { get; set; }

        public DateTime? MaxOrderDate { get; set; }
    }
}
