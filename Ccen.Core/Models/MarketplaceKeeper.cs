using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO;

namespace Amazon.Core.Models
{
    //https://images-na.ssl-images-amazon.com/images/G/02/mwsportal/doc/en_US/bde/MWSDeveloperGuide._V327338421_.pdf
    public class MarketplaceKeeper : IMarketplaceService
    {
        /* https://images-na.ssl-images-amazon.com/images/G/01/mwsportal/doc/en_US/bde/MWSDeveloperGuide._V384366295_.pdf
         Table 1: NA region
        Amazon Marketplace Amazon MWS Endpoint MarketplaceId
        CA https://mws.amazonservices.ca A2EUQ1WTGCTBG2
        US https://mws.amazonservices.com ATVPDKIKX0DER
        Mexico	www.amazon.com.mx MX https://mws.amazonservices.com.mx	A1AM78C64UM0Y8
        Table 2: EU region
        Amazon Marketplace Amazon MWS Endpoint MarketplaceId'-
'        DE https://mws-eu.amazonservices.com A1PA6795UKMFR9
        ES https://mws-eu.amazonservices.com A1RKKUPIHCS9HS
        FR https://mws-eu.amazonservices.com A13V1IB3VIYZZH
        IN https://mws.amazonservices.in A21TJRUUN4KGV
        IT https://mws-eu.amazonservices.com APJ6JRA9NG5V4
        UK https://mws-eu.amazonservices.com A1F83G8C2ARO7P

        India	IN	https://mws.amazonservices.in	A21TJRUUN4KGV
        Italy	IT	https://mws-eu.amazonservices.com	APJ6JRA9NG5V4
        Turkey	TR	https://mws-eu.amazonservices.com	A33AVAJ2PDY3EV

        Table 3: FE region
        Amazon Marketplace Amazon MWS Endpoint MarketplaceId
        JP https://mws.amazonservices.jp A1VC38T7YXB528
        Table 4: CN region
        Amazon Marketplace Amazon MWS Endpoint MarketplaceId
        CN https://mws.amazonservices.com.cn AAHKV2X7AFYLW
         * */


        private static IList<MarketplaceDTO> _marketplaces;

        public const string GrouponPA1 = "PA1";
        public const string GrouponPA2 = "PA2";

        public const string AmazonComMarketplaceId = "ATVPDKIKX0DER";
        public const string AmazonCaMarketplaceId = "A2EUQ1WTGCTBG2";
        public const string AmazonMxMarketplaceId = "A1AM78C64UM0Y8";

        public const string AmazonUkMarketplaceId = "A1F83G8C2ARO7P";
        public const string AmazonDeMarketplaceId = "A1PA6795UKMFR9";
        public const string AmazonEsMarketplaceId = "A1RKKUPIHCS9HS";
        public const string AmazonFrMarketplaceId = "A13V1IB3VIYZZH";
        public const string AmazonItMarketplaceId = "APJ6JRA9NG5V4";

        public const string AmazonAuMarketplaceId = "A39IBJ37TRP1C6";

        public const string AmazonInMarketplaceId = "A21TJRUUN4KGV";

        public const string eBayAll4Kids = "All4Kids";
        public const string eBayPA = "PA";
        public const string eBayTMX = "TMX";
        public const string eBayTNR = "TNR";

        public const string ShopifyBNW = "BNW";
        public const string ShopifySDW = "SDW";
        public const string ShopifyDWS = "DWS";
        public const string ShopifyZZG = "ZZG";

        public const string MagentoDWS = "DWS";
        public const string MagentoASH = "ASHFORD";

        public const string ShopifyMBB = "MBB";
        public const string ShopifyTonarex = "TONAREX";
        public const string ShopifyPreorderTBO = "PREORDERTBO";
        public const string ShopifyTBO = "TBO";
        public const string ShopifyAgape = "AGAPE";

        public const string ShopifyEveryCh = "EVERYCH";
        public const string ShopifyRBN = "RBN";
        public const string ShopifySOTea = "SOTEA";
                
        public const string ShopifyBlonde = "BLONDE";

        public const string WooHdea = "HDEA";

        public const string DsDWS = "DSDWS";
        public const string DsToMBG = "DSMBG";
        public const string DsToTMX = "DSTMX";
        public const string DsPAWMCom = "DSPA";

        public const string GrouponDWS = "DWS";
        public const string GrouponGSD = "GSD";

        public const string CustomAshford = "ASH";

        public const string ManuallyCreated = "USER";

        //public const string eBayMarketplaceId = "eBay";
        //public const string MagentoMarketplaceId = "Magento";

        private IDbFactory _dbFactory;
        private bool _isEnablePrime;

        public MarketplaceKeeper(IDbFactory dbFactory, bool isEnablePrime)
        {
            _dbFactory = dbFactory;
            _isEnablePrime = isEnablePrime;
        }

        public void Init()
        {
            if (_marketplaces == null)
            {
                using (var db = _dbFactory.GetRDb())
                {
                    _marketplaces = db.Marketplaces.GetAllAsDto()
                        .Where(m => m.IsActive)
                        .OrderBy(m => m.SortOrder)
                        .ToList();
                }
            }
        }

        public static string GetMarketplaceCodeName(MarketType market, string marketplaceId)
        {
            if (market == MarketType.eBay)
                return "EBay";
            if (market == MarketType.Magento)
                return "MG";
            if (market == MarketType.Groupon)
                return "GP";
            if (market == MarketType.CustomAshford)
                return "ASH";
            if (market == MarketType.Walmart)
                return "WM";
            if (market == MarketType.OfflineOrders)
                return "SJ";
            if (market == MarketType.WooCommerce)
                return "WOO";
            if (market == MarketType.DropShipper)
            {
                if (marketplaceId == DsDWS)
                {
                    return "DWS";
                }
                if (marketplaceId == DsToMBG)
                {
                    return "MBG";
                }
                if (marketplaceId == DsPAWMCom)
                {
                    return "PA";
                }
            }
            if (marketplaceId == AmazonComMarketplaceId)
                return "COM";
            if (marketplaceId == AmazonCaMarketplaceId)
                return "CA";
            if (marketplaceId == AmazonMxMarketplaceId)
                return "MX";
            if (marketplaceId == AmazonUkMarketplaceId)
                return "UK";
            if (marketplaceId == AmazonDeMarketplaceId)
                return "DE";
            if (marketplaceId == AmazonEsMarketplaceId)
                return "ES";
            if (marketplaceId == AmazonFrMarketplaceId)
                return "FR";
            if (marketplaceId == AmazonItMarketplaceId)
                return "IT";
            if (marketplaceId == AmazonAuMarketplaceId)
                return "AU";

            return "";
        }

        public static string DefaultMarketplaceId
        {
            get
            {
                return AmazonComMarketplaceId;
            }
        }

        public MarketplaceDTO Default
        {
            get
            {
                return GetAll().FirstOrDefault(m => m.MarketplaceId == DefaultMarketplaceId);
            }
        }

        public IList<MarketplaceDTO> GetAll()
        {
            Init();
            return _marketplaces;
        }

        public IList<MarketplaceDTO> GetAllWithVirtual()
        {
            var markets = GetAll().ToList();
            if (markets.Any(m => m.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId))
            {
                if (_isEnablePrime)
                {
                    markets.Add(new MarketplaceDTO()
                    {
                        Market = (int)MarketType.AmazonPrime,
                        MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId
                    });
                }
            }
            return markets;
        }
    }
}
