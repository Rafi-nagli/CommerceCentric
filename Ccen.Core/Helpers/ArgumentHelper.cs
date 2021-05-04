using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Core.Helpers
{
    public static class ArgumentHelper
    {
        public static bool CheckMarket(MarketType? market)
        {
            if (!market.HasValue)
                return false;
            return market != MarketType.None;
        }

        public static bool CheckMarketplaceId(MarketType? market, string marketplaceId)
        {
            if (!market.HasValue)
                return false;

            if (market == MarketType.Magento
                || market == MarketType.Groupon
                || market == MarketType.CustomAshford
                || market == MarketType.Walmart 
                || market == MarketType.WalmartCA
                || market == MarketType.Jet
                || market == MarketType.eBay
                || market == MarketType.OverStock
                || market == MarketType.WooCommerce)
                return true;

            if (!String.IsNullOrEmpty(marketplaceId))
                return true;

            return false;
        }
    }
}
