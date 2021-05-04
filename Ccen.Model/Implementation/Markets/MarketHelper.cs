using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Models;
using Amazon.DAL;

namespace Amazon.Model.Implementation.Markets
{
    public class MarketHelper
    {
        public static MarketType Default
        {
            get { return MarketType.Amazon; }
        }

        public static MarketType DefaultUIMarket
        {
            get { return MarketType.None; }
        }

        public static string DefaultUIMarketplaceId
        {
            get { return ""; }
        }

        public static string GetMarketLogo(int market, string marketplaceId)
        {
            if (marketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
            {
                return "/Images/amazon-com-logo-ps.png";
            }
            if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
            {
                return "/Images/amazon-ca-logo-ps.png";
            }
            if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
            {
                return "/Images/amazon-mx-logo-ps.png";
            }
            if (market == (int)MarketType.AmazonEU)
            {
                return "/Images/amazon-eu-logo-ps.png";
            }
            if (market == (int)MarketType.AmazonAU)
            {
                return "/Images/amazon-au-logo-ps.png";
            }
            if (market == (int)MarketType.eBay)
            {
                return "/Images/ebay-logo-ps.png";
            }
            if (market == (int)MarketType.Walmart)
            {
                return "/Images/walmart-logo-ps.png";
            }
            if (market == (int)MarketType.WalmartCA)
            {
                return "/Images/walmart-ca-logo-ps.png";
            }
            if (market == (int)MarketType.Jet)
            {
                return "/Images/jet-logo-ps.png";
            }
            if (market == (int)MarketType.Shopify)
            {
                return "/Images/shopify-logo-ps.png";
            }
            if (market == (int)MarketType.WooCommerce)
            {
                return "/Images/woocommerce-logo-ps.png";
            }
            if (market == (int)MarketType.Groupon)
            {
                return "/Images/groupon-logo-ps.png";
            }
            if (market == (int)MarketType.OverStock)
            {
                return "/Images/overstock-logo-ps.png";
            }
            if (market == (int)MarketType.Magento)
            {
                return "/Images/magento-logo-ps.png";
            }
            return "";
        }

        public static IList<MarketplaceName> GetOrderPageMarketplaces()
        {
            List<MarketplaceName> marketplaces;
            using (var db = new UnitOfWork(null))
            {
                var dtoMarketplaces = db.Marketplaces.GetAllAsDto()
                    .Where(m => m.IsActive
                        && !m.IsHidden)
                    .OrderBy(m => m.SortOrder)
                    .ToList();
                marketplaces = dtoMarketplaces.Select(m => new MarketplaceName()
                {
                    Market = (MarketType)m.Market,
                    MarketplaceId = m.MarketplaceId,
                }).ToList();
            }
            foreach (var m in marketplaces)
            {
                m.Name = GetMarketName((int) m.Market, m.MarketplaceId);
                m.ShortName = GetShortName((int)m.Market, m.MarketplaceId);
                m.DotName = GetDotShortName((int)m.Market, m.MarketplaceId);
            }
            return marketplaces;
        }

        public static IList<MarketplaceName> GetSalesMarketplaces()
        {
            List<MarketplaceName> marketplaces;
            using (var db = new UnitOfWork(null))
            {
                var dtoMarketplaces = db.Marketplaces.GetAllAsDto()
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.SortOrder)
                    .ToList();
                marketplaces = dtoMarketplaces.Select(m => new MarketplaceName()
                {
                    Market = (MarketType)m.Market,
                    MarketplaceId = m.MarketplaceId,
                }).ToList();
            }
            foreach (var m in marketplaces)
            {
                m.ShortName = GetShortName((int)m.Market, m.MarketplaceId);
                m.DotName = GetDotShortName((int)m.Market, m.MarketplaceId);
            }
            return marketplaces;
        }

        public static MarketType GetMarketTypeByName(string name)
        {
            switch (name)
            {
                case "US":
                case "CA":
                case "MX":
                    return MarketType.Amazon;
                case "UK":
                    return MarketType.AmazonEU;
                case "AU":
                    return MarketType.AmazonAU;
                case "eBay":
                    return MarketType.eBay;
                case "MG":
                    return MarketType.Magento;
                case "WM":
                case "Walmart":
                    return MarketType.Walmart;
                case "WM-CA":
                case "WalmartCA":
                    return MarketType.WalmartCA;
                case "Jet":
                    return MarketType.Jet;
                case "SH":
                case "Shopify":
                    return MarketType.Shopify;
                case "WOO":
                case "WooCommerce":
                    return MarketType.WooCommerce;
                case "GP":
                    return MarketType.Groupon;
                case "OS":
                case "OStock":
                case "OverStock":
                    return MarketType.OverStock;
            }
            return MarketType.None;
        }

        public static MarketplaceName GetMarketNameByEmailAddress(string emailAddress)
        {
            var market = MarketType.None;
            string marketplaceId = null;
            if (String.IsNullOrEmpty(emailAddress))
            {
                return new MarketplaceName()
                {
                    Market = market
                };
            }

            emailAddress = emailAddress.ToLower();
            if (emailAddress.Contains("amazon"))
            {
                if (emailAddress.Contains(".com"))
                {
                    market = MarketType.Amazon;
                    marketplaceId = MarketplaceKeeper.AmazonComMarketplaceId;
                }
                if (emailAddress.Contains(".ca"))
                {
                    market = MarketType.Amazon;
                    marketplaceId = MarketplaceKeeper.AmazonCaMarketplaceId;
                }
                if (emailAddress.Contains(".mx"))
                {
                    market = MarketType.Amazon;
                    marketplaceId = MarketplaceKeeper.AmazonMxMarketplaceId;
                }

                if (emailAddress.Contains(".uk"))
                {
                    market = MarketType.AmazonEU;
                    marketplaceId = MarketplaceKeeper.AmazonUkMarketplaceId;
                }
                if (emailAddress.Contains(".de"))
                {
                    market = MarketType.AmazonEU;
                    marketplaceId = MarketplaceKeeper.AmazonDeMarketplaceId;
                }
                if (emailAddress.Contains(".es"))
                {
                    market = MarketType.AmazonEU;
                    marketplaceId = MarketplaceKeeper.AmazonEsMarketplaceId;
                }
                if (emailAddress.Contains(".fr"))
                {
                    market = MarketType.AmazonEU;
                    marketplaceId = MarketplaceKeeper.AmazonFrMarketplaceId;
                }
                if (emailAddress.Contains(".it"))
                {
                    market = MarketType.AmazonEU;
                    marketplaceId = MarketplaceKeeper.AmazonItMarketplaceId;
                }

                if (emailAddress.Contains(".au"))
                {
                    market = MarketType.AmazonAU;
                    marketplaceId = MarketplaceKeeper.AmazonAuMarketplaceId;
                }
            }
            else if (emailAddress.Contains("walmart") && emailAddress.EndsWith("ca"))
            {
                market = MarketType.WalmartCA;
            }
            else if (emailAddress.Contains("walmart"))
            {
                market = MarketType.Walmart;
            }
            else if (emailAddress.Contains("ebay"))
            {
                market = MarketType.eBay;
            }
            else if (emailAddress.Contains("jet"))
            {
                market = MarketType.Jet;
            }

            return new MarketplaceName()
            {
                Market = market,
                MarketplaceId = marketplaceId
            };
        }

        public static string GetMarketplaceIdTypeByName(string name)
        {
            switch (name)
            {
                case "US":
                    return MarketplaceKeeper.AmazonComMarketplaceId;
                case "CA":
                    return MarketplaceKeeper.AmazonCaMarketplaceId;
                case "MX":
                    return MarketplaceKeeper.AmazonMxMarketplaceId;
                case "UK":
                    return MarketplaceKeeper.AmazonUkMarketplaceId;
                case "AU":
                    return MarketplaceKeeper.AmazonAuMarketplaceId;
                    //case "eBay":
                    //    return MarketplaceKeeper.eBayMarketplaceId;
                    //case "MG":
                    //    return MarketplaceKeeper.MagentoMarketplaceId;
            }
            return "";
        }

        public static string GetMarketName(int market, string marketplaceId)
        {
            if (market == (int) MarketType.AmazonAU)
            {
                return "amazon.com.au";
            }
            if (market == (int) MarketType.AmazonIN)
            {
                return "amazon.in";
            }
            if (market == (int)MarketType.AmazonEU)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonDeMarketplaceId)
                    return "amazon.de";
                if (marketplaceId == MarketplaceKeeper.AmazonEsMarketplaceId)
                    return "amazon.es";
                if (marketplaceId == MarketplaceKeeper.AmazonFrMarketplaceId)
                    return "amazon.fr";
                if (marketplaceId == MarketplaceKeeper.AmazonItMarketplaceId)
                    return "amazon.it";
                if (marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                    return "amazon.co.uk";
                return "amazon.co.uk";
            }
            if (market == (int)MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return "amazon.ca";
                if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    return "amazon.com.mx";
                return "amazon.com";
            }
            if (market == (int)MarketType.eBay)
            {
                return "eBay(" + marketplaceId + ")";
            }
            if (market == (int)MarketType.Magento)
            {
                return "pa.com";
            }
            if (market == (int)MarketType.Walmart)
            {
                return "walmart.com";
            }
            if (market == (int)MarketType.WalmartCA)
            {
                return "walmart.ca";
            }
            if (market == (int)MarketType.Jet)
            {
                return "jet.com";
            }
            if (market == (int)MarketType.Shopify)
            {
                return "shopify.com";
            }
            if (market == (int)MarketType.WooCommerce)
            {
                return "woocoomerce.com";
            }
            if (market == (int)MarketType.Groupon)
            {
                return "groupon.com";
            }
            if (market == (int)MarketType.OverStock)
            {
                return "overstock.com";
            }
            if (market == (int)MarketType.DropShipper)
            {
                if (marketplaceId == MarketplaceKeeper.DsToMBG)
                {
                    return "MBG";
                }
            }
            return "-";
        }

        public static string GetPackingSlipDisplayName(int market, string marketplaceId)
        {
            if (market == (int)MarketType.AmazonAU)
            {
                return "Amazon";
            }
            if (market == (int)MarketType.AmazonIN)
            {
                return "Amazon";
            }
            if (market == (int)MarketType.AmazonEU)
            {
                return "Amazon";
            }
            if (market == (int)MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return "Amazon";
                if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    return "Amazon";
                return "Amazon";
            }
            if (market == (int)MarketType.eBay)
            {
                return "eBay";
            }
            if (market == (int)MarketType.Magento)
            {
                return "Web";
            }
            if (market == (int)MarketType.Walmart)
            {
                return "Walmart";
            }
            if (market == (int)MarketType.WalmartCA)
            {
                return "WalmartCA";
            }
            if (market == (int)MarketType.Jet)
            {
                return "Jet";
            }
            if (market == (int)MarketType.Shopify)
            {
                return "Shopify";
            }
            if (market == (int)MarketType.WooCommerce)
            {
                return "WooCommerce";
            }
            if (market == (int)MarketType.Groupon)
            {
                return "Groupon";
            }
            if (market == (int)MarketType.OverStock)
            {
                return "OverStock";
            }
            if (market == (int)MarketType.DropShipper)
            {
                if (marketplaceId == MarketplaceKeeper.DsToMBG)
                {
                    return "MBG";
                }
            }
            return "-";
        }

        public static string GetShortName(int market, string marketplaceId, bool detailedUK = false)
        {
            if (market == (int) MarketType.AmazonAU)
            {
                return "AU";
            }
            if (market == (int) MarketType.AmazonIN)
            {
                return "IN";
            }

            if (market == (int)MarketType.AmazonEU)
            {
                if (detailedUK)
                {
                    if (marketplaceId == MarketplaceKeeper.AmazonEsMarketplaceId)
                        return "ES";
                    if (marketplaceId == MarketplaceKeeper.AmazonFrMarketplaceId)
                        return "FR";
                    if (marketplaceId == MarketplaceKeeper.AmazonItMarketplaceId)
                        return "IT";
                    if (marketplaceId == MarketplaceKeeper.AmazonDeMarketplaceId)
                        return "DE";
                    if (marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                        return "UK";
                }
                else
                {
                    return "UK";
                }
            }
            if (market == (int)MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return "CA";
                if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    return "MX";
                return "US";
            }
            if (market == (int)MarketType.eBay)
            {
                if (marketplaceId == MarketplaceKeeper.eBayAll4Kids)
                    return "eBay4kids";
                return "eBay" + marketplaceId + "";
            }
            if (market == (int)MarketType.Magento)
            {
                return "MG";
            }
            if (market == (int)MarketType.Walmart)
            {
                return "WM";
            }
            if (market == (int)MarketType.WalmartCA)
            {
                return "WM-CA";
            }
            if (market == (int)MarketType.Jet)
            {
                return "Jet";
            }
            if (market == (int)MarketType.Shopify)
            {
                if (!String.IsNullOrEmpty(marketplaceId))
                    return marketplaceId;
                return "SH";
            }
            if (market == (int)MarketType.WooCommerce)
            {
                if (!String.IsNullOrEmpty(marketplaceId))
                    return marketplaceId;
                return "WOO";
            }
            if (market == (int)MarketType.Groupon)
            {
                return "GP";
            }
            if (market == (int)MarketType.OverStock)
            {
                return "OS";
            }
            if (market == (int)MarketType.DropShipper)
            {
                if (marketplaceId == MarketplaceKeeper.DsToMBG)
                {
                    return "MBG";
                }
            }
            return "-";
        }

        public static string GetDotShortName(int market, string marketplaceId)
        {
            if (market == (int)MarketType.eBay)
            {
                if (marketplaceId == MarketplaceKeeper.eBayAll4Kids)
                    return "eBay(4kids)";
                if (marketplaceId == MarketplaceKeeper.eBayPA)
                    return "eBay(pa)";
                return "ebay";
            }
            if (market == (int)MarketType.Magento)
                return "pa.com";
            if (market == (int)MarketType.Walmart)
                return "wm";
            if (market == (int)MarketType.WalmartCA)
                return "wm.ca";
            if (market == (int)MarketType.Jet)
                return "jet";
            if (market == (int)MarketType.Shopify)
            {
                return "shopify";
            }
            if (market == (int)MarketType.WooCommerce)
            {
                return "woocommerce";
            }
            if (market == (int)MarketType.Groupon)
            {
                return "groupon.com";
            }
            if (market == (int)MarketType.OverStock)
                return "overstock";
            switch (marketplaceId)
            {
                case MarketplaceKeeper.AmazonComMarketplaceId:
                    return ".com";
                case MarketplaceKeeper.AmazonCaMarketplaceId:
                    return ".ca";
                case MarketplaceKeeper.AmazonMxMarketplaceId:
                    return ".mx";
                case MarketplaceKeeper.AmazonUkMarketplaceId:
                    return ".uk";
                case MarketplaceKeeper.AmazonDeMarketplaceId:
                    return ".de";
                case MarketplaceKeeper.AmazonEsMarketplaceId:
                    return ".es";
                case MarketplaceKeeper.AmazonFrMarketplaceId:
                    return ".fr";
                case MarketplaceKeeper.AmazonItMarketplaceId:
                    return ".it";
                case MarketplaceKeeper.AmazonAuMarketplaceId:
                    return ".au";
            }
            if (market == (int)MarketType.AmazonEU) //NOTE: case when not marketplaceId (all EU marketplaces)
                return ".eu";
            if (market == (int)MarketType.AmazonIN)
                return ".in";
            if (market == (int)MarketType.AmazonAU) //NOTE: case when not marketplaceId (all AU marketplaces)
                return ".au";
            if (market == (int)MarketType.DropShipper)
            {
                if (marketplaceId == MarketplaceKeeper.DsToMBG)
                {
                    return "mbg.com";
                }
            }
            return "-";
        }

        public static float GetMarketIndex(MarketType market)
        {
            return GetMarketIndex(market, "");
        }

        public static float GetMarketIndex(MarketType market, string marketplaceId)
        {
            if (market == MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                    return 1;
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return 2;
                if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    return 3;
                return 5;
            }
            if (market == MarketType.AmazonEU)
                return 10;
            if (market == MarketType.AmazonAU)
                return 20;
            if (market == MarketType.eBay)
                return 50;
            if (market == MarketType.Walmart)
                return 54;
            if (market == MarketType.WalmartCA)
                return 55;
            if (market == MarketType.Jet)
                return 56;
            if (market == MarketType.Shopify)
                return 57;
            if (market == MarketType.OverStock)
                return 58;
            if (market == MarketType.Magento)
                return 60;
            if (market == MarketType.Groupon)
                return 65;
            if (market == MarketType.WooCommerce)
                return 70;
            return 100;
        }

        public static bool IsAmazon(MarketType market)
        {
            return market == MarketType.Amazon 
                || market == MarketType.AmazonEU
                || market == MarketType.AmazonAU
                || market == MarketType.AmazonIN
                || market == MarketType.AmazonPrime;
        }
    }
}
