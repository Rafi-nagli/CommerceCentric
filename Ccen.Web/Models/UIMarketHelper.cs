using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.Models
{
    public class UIMarketHelper
    {
        public static IList<MarketplaceName> GetSalesMarketplaces()
        {
            var marketplaces = new List<MarketplaceName>()
            {
                new MarketplaceName() { Market = MarketType.Amazon, MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId },
                new MarketplaceName() { Market = MarketType.Walmart, MarketplaceId = "" },
                new MarketplaceName() { Market = MarketType.WalmartCA, MarketplaceId = "" },
                new MarketplaceName() { Market = MarketType.Amazon, MarketplaceId = MarketplaceKeeper.AmazonCaMarketplaceId },
                new MarketplaceName() { Market = MarketType.AmazonEU, MarketplaceId = "" },
                new MarketplaceName() { Market = MarketType.AmazonAU, MarketplaceId = "" },
                new MarketplaceName() { Market = MarketType.eBay, MarketplaceId = "" },
                new MarketplaceName() { Market = MarketType.Jet, MarketplaceId = "" }
            };
            foreach (var m in marketplaces)
            {
                m.ShortName = MarketHelper.GetShortName((int)m.Market, m.MarketplaceId);
                m.DotName = MarketHelper.GetDotShortName((int)m.Market, m.MarketplaceId);
            }
            return marketplaces;
        }
    }
}