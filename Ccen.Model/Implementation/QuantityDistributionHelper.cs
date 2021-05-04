using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Models;
using Amazon.Model.General.Services;

namespace Amazon.Model.Implementation
{
    public class QuantityDistributionHelper
    {
        static public IList<MarketDistributeInfo> GetDistributionMarkets()
        {
            var marketInfoList = new List<MarketDistributeInfo>()
            {
                new MarketDistributeInfo() {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId,
                    Multiplier = 2,
                    MaxQuantity = 30,
                    LowQuantity = 10,
                    SupportRestockDate = true,
                },
                new MarketDistributeInfo() {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonCaMarketplaceId,
                    LinkedToMarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId,
                    Multiplier = 1,
                    MaxQuantity = 30,
                    LowQuantity = 10,
                    SupportRestockDate = true,
                },
                new MarketDistributeInfo() {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonMxMarketplaceId,
                    LinkedToMarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId,
                    Multiplier = 1,
                    MaxQuantity = 30,
                    LowQuantity = 10,
                    SupportRestockDate = true,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.AmazonEU,
                    MarketplaceId = MarketplaceKeeper.AmazonUkMarketplaceId,
                    Multiplier = 1,
                    MaxQuantity = 30,
                    LowQuantity = 10,
                    SupportRestockDate = true,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.AmazonAU,
                    MarketplaceId = MarketplaceKeeper.AmazonAuMarketplaceId,
                    Multiplier = 1,
                    MaxQuantity = 30,
                    LowQuantity = 10,
                    SupportRestockDate = true,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.AmazonIN,
                    MarketplaceId = MarketplaceKeeper.AmazonInMarketplaceId,
                    Multiplier = 1,
                    MaxQuantity = 30,
                    LowQuantity = 10,
                    SupportRestockDate = true,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.eBay,
                    MarketplaceId = MarketplaceKeeper.eBayAll4Kids,
                    Multiplier = 1,
                    MaxQuantity = 3,
                    LowQuantity = 3,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.eBay,
                    MarketplaceId = MarketplaceKeeper.eBayPA,
                    Multiplier = 1,
                    MaxQuantity = 3,
                    LowQuantity = 3,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.Magento,
                    MarketplaceId = String.Empty,
                    Multiplier = 1,
                    MaxQuantity = 5,
                    LowQuantity = 5,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.Walmart,
                    MarketplaceId = String.Empty,
                    Multiplier = 2,
                    MaxQuantity = 40,
                    LowQuantity = 10,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.WalmartCA,
                    MarketplaceId = String.Empty,
                    Multiplier = 1,
                    MaxQuantity = 40,
                    LowQuantity = 10,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.Jet,
                    MarketplaceId = String.Empty,
                    Multiplier = 1,
                    MaxQuantity = 40,
                    LowQuantity = 10,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.Groupon,
                    MarketplaceId = String.Empty,
                    Multiplier = 1,
                    MaxQuantity = 40,
                    LowQuantity = 10,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.DropShipper,
                    MarketplaceId = MarketplaceKeeper.DsToMBG,
                    Multiplier = 1,
                    MaxQuantity = 40,
                    LowQuantity = 10,
                },
                new MarketDistributeInfo()
                {
                    Market = MarketType.OverStock,
                    MarketplaceId = String.Empty,
                    Multiplier = 1,
                    MaxQuantity = 40,
                    LowQuantity = 10,
                },
            };

            return marketInfoList;
        }
    }
}
