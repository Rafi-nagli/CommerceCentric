using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class MarketIdentifier
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public MarketIdentifier(int market, string marketplaceId)
        {
            Market = market;
            MarketplaceId = marketplaceId;
        }

        public static MarketIdentifier Empty()
        {
            return new MarketIdentifier((int)0, null);
        }

        public override string ToString()
        {
            return Market.ToString() + "_" + MarketplaceId;
        }
    }
}
