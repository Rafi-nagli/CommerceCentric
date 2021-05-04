using Amazon.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.General.Markets
{
    public class MarketBaseHelper
    {

        public static string GetMarketCountry(MarketType market, string marketplaceId)
        {
            if (market == MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return "CA";
                return "US";
            }
            if (market == MarketType.AmazonAU)
            {
                return "AU";
            }
            if (market == MarketType.AmazonEU)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonDeMarketplaceId)
                    return "DE";
                if (marketplaceId == MarketplaceKeeper.AmazonEsMarketplaceId)
                    return "ES";
                if (marketplaceId == MarketplaceKeeper.AmazonFrMarketplaceId)
                    return "FR";
                if (marketplaceId == MarketplaceKeeper.AmazonItMarketplaceId)
                    return "IT";
                if (marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                    return "UK";
                return "UK";
            }

            if (market == MarketType.WalmartCA)
            {
                return "CA";
            }
            if (market == MarketType.AmazonIN)
            {
                return "IN";
            }

            return "US";
        }
    }
}
