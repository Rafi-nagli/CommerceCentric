using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Caches
{
    public class MarketplaceCacheInfo
    {
        public string MarketName { get; set; }
        public string ASIN { get; set; }
        public string SourceMarketId { get; set; }
        public int ListingsCount { get; set; }
    }
}
