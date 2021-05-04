using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ItemMarketInfoDTO
    {
        public string SKU { get; set; }
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
    }
}
