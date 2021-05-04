using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets.Magento;
using DropShipper.Api;
using eBay.Api;
using Groupon.Api;
using Jet.Api;
using Jet.Api.Models;
using Magento.Api.Wrapper;
using Shopify.Api;
using Supplieroasis.Api;
using Walmart.Api;
using WalmartCA.Api;
using WooCommerce.Api;

namespace Amazon.Model.Implementation
{
    public class MarketFactory : IMarketFactory
    {
        private IList<MarketplaceDTO> _marketplaces;
        private ITime _time;
        private ILogService _log;
        private IDbFactory _dbFactory;
        private string _javaPath;

        public MarketFactory(IList<MarketplaceDTO> marketplaces,
            ITime time, 
            ILogService log,
            IDbFactory dbFactory,
            string javaPath)
        {
            _marketplaces = marketplaces;
            _time = time;
            _log = log;
            _dbFactory = dbFactory;
            _javaPath = javaPath;
        }

        public IMarketApi GetApi(long companyId, MarketType market, string marketplaceId)
        {
            var query = _marketplaces.Where(m => m.Market == (int) market && m.CompanyId == companyId);
            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(m => m.MarketplaceId == marketplaceId);
            
            var marketplace = query.FirstOrDefault();
            if (marketplace == null)
                return null;
                //throw new ObjectNotFoundException("Marketplace, for market=" + market + ", marketplaceId=" + marketplaceId);

            switch (market)
            {
                case MarketType.Amazon:
                case MarketType.AmazonEU:
                case MarketType.AmazonAU:
                case MarketType.AmazonIN:
                    return new AmazonApi(_time,
                        _log,
                        marketplace.Key3,
                        marketplace.Key4,
                        marketplace.Key5,
                        marketplace.Key1,
                        marketplace.Key2,
                        marketplace.Token,
                        marketplace.SellerId,
                        marketplace.MarketplaceId,
                        marketplace.EndPointUrl);
                case MarketType.eBay:
                    return new eBayApi(_time,
                        _log,
                        marketplace.MarketplaceId,
                        marketplace.Key1,
                        marketplace.Key2,
                        marketplace.Key3,
                        marketplace.Token,
                        marketplace.Key4,
                        marketplace.Key5,
                        marketplace.EndPointUrl,
                        marketplace.TemplateFolder);
                case MarketType.Magento:
                    return new Magento20MarketApi(_time,
                        _log,
                        marketplace.MarketplaceId,
                        marketplace.Key1,
                        marketplace.Key2,
                        marketplace.EndPointUrl,
                        marketplace.Token,
                        () => GetFreshTokenInfo(MarketType.Magento),
                        (t) => StoreNewTokenInfo(MarketType.Magento, t),
                        new PAMagentoFeatures());
                case MarketType.Walmart:
                    return new WalmartApi(_log,
                        _time,
                        marketplace.SellerId,

                        marketplace.Key4,
                        marketplace.Key5,
                        marketplace.Token,

                        () => GetFreshTokenInfo(MarketType.Walmart),
                        (t) => StoreNewTokenInfo(MarketType.Walmart, t),

                        marketplace.Key1, //channel type
                        marketplace.EndPointUrl,
                        StringHelper.TryGetInt(marketplace.Key2) ?? 1,
                        marketplace.Key3,
                        _javaPath);
                case MarketType.WalmartCA:
                    return new WalmartCAApi(_log,
                        _time,
                        marketplace.SellerId,
                        marketplace.Token,
                        marketplace.Key1,
                        marketplace.EndPointUrl,
                        _javaPath);
                case MarketType.Jet:
                    return new JetApi(_log,
                        _time,
                        marketplace.Key1,
                        marketplace.Key2,
                        marketplace.SellerId,
                        marketplace.Key3,
                        marketplace.Token,
                        () => GetFreshTokenInfo(MarketType.Jet),
                        (t) => StoreNewTokenInfo(MarketType.Jet, t));
                case MarketType.Shopify:
                    return new ShopifyApi(_log,
                        _time,
                        marketplace.MarketplaceId,
                        marketplace.Key1,
                        marketplace.Key2,
                        marketplace.Token,
                        marketplace.EndPointUrl,
                        marketplace.Key3,
                        ShopifyApi.QuantityUpdateMode.Default,
                        false);
                case MarketType.WooCommerce:
                    return new WooCommerceApi(_log,
                        _time,
                        marketplace.MarketplaceId,
                        marketplace.Key1,
                        marketplace.Key2,
                        marketplace.EndPointUrl,
                        (marketplace.Key3 ?? "").Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                case MarketType.Groupon:
                    return new GrouponApi(_log,
                        _time,
                        marketplace.MarketplaceId,
                        marketplace.SellerId,
                        marketplace.Token,
                        marketplace.EndPointUrl);
                case MarketType.DropShipper:
                    return new DropShipperApi(_log,
                        _time,
                        marketplace.MarketplaceId,
                        marketplace.StoreUrl,
                        marketplace.Token,
                        StringHelper.TryGetInt(marketplace.Key2) ?? 0,
                        marketplace.EndPointUrl);
                case MarketType.OverStock:
                    return new SupplieroasisApi(_log,
                        _time,
                        marketplace.Token,
                        marketplace.Key1,
                        marketplace.EndPointUrl);
            }
            throw new NotImplementedException("GetApi, market=" + market.ToString());
        }

        private TokenInfo GetFreshTokenInfo(MarketType market)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var marketInfo = db.Marketplaces.GetAllAsDto().FirstOrDefault(m => m.Market == (int)market);
                if (marketInfo != null)
                {
                    return new TokenInfo()
                    {
                        Token = marketInfo.Token
                    };
                }
            }
            return null;
        }

        private void StoreNewTokenInfo(MarketType market, TokenInfo token)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var marketInfo = db.Marketplaces.GetAll().FirstOrDefault(m => m.Market == (int) market);
                if (marketInfo != null)
                {
                    marketInfo.Token = token.Token;
                    db.Commit();
                }

                //NOTE: refresh info in local copy
                if (_marketplaces != null)
                {
                    var marketplace = _marketplaces.FirstOrDefault(m => m.Market == (int) market);
                    if (marketplace != null)
                        marketplace.Token = token.Token;
                }
            }
        }
    }
}
