using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class MarketplaceInfo
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public int? ListingQtyAlert { get; set; }
        public int? ListingPriceAlert { get; set; }
        public int? OrderCountOnMarket { get; set; }
        public int? OrderCountInDb { get; set; }
        public DateTime? OrderSyncDate { get; set; }
        public DateTime? OrdersFulfillmentDate { get; set; }

        public DateTime? ListingsSyncDate { get; set; }
        public bool? ListingsSyncInProgress { get; set; }
        public bool? ListingsManualSyncRequest { get; set; }
            
        public DateTime? ListingsQuantityToAmazonSyncDate { get; set; }
        public DateTime? ListingsPriceSyncDate { get; set; }

        public DateTime? RankSyncDate { get; set; }
    }
}
