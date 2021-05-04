using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Caches
{
    public class MarketplaceCacheInfo
    {
        public string MarketName { get; set; }
        public int ListingsCount { get; set; }
        public int BestListingStatus { get; set; }
    }
}
