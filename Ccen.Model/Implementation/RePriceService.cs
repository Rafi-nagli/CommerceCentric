using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Model.Implementation
{
    public class PriceService : IPriceService
    {
        private IDbFactory _dbFactory;

        public PriceService(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public decimal GetPrimePart(decimal? weight)
        {
            return (weight >= 16 ? 8.49M : 6.49M);
        }

        public decimal GetSourcePrice(decimal currentPrice,
            bool isPrime,
            bool isFBA,
            decimal weight,
            MarketType market,
            string marketplaceId,
            long? styleId,
            long? styleItemId)
        {
            if (market == MarketType.Amazon)
            {
                if (isPrime || isFBA)
                    return currentPrice - GetPrimePart(weight);

                //TODO: calculate based on Cost
                decimal? cost = null;
                if (styleItemId.HasValue)
                {
                    using (var db = _dbFactory.GetRWDb())
                    {
                        var cacheItem = db.StyleCaches.GetAll().FirstOrDefault(si => si.Id == styleId);
                        cost = cacheItem?.Cost;
                    }
                }
                if (cost.HasValue && cost > 0)
                    return cost.Value + 2 + 0.16M * currentPrice; //comissions

                return currentPrice;
            }
            if (market == MarketType.Walmart)
            {
                if (isPrime || isFBA)
                    return currentPrice - 5M;
                return currentPrice;

            }
            if (market == MarketType.eBay)
            {
                if (weight >= 16) //NOTE: ==OversizeTemplate
                    return currentPrice - 3.5M;
                return currentPrice;
            }
            return currentPrice;
        }

        public decimal GetMarketPrice(decimal sourcePrice,
            decimal? sourceSFPPrice,
            bool isPrime,
            bool isFBA,
            double? weight,
            MarketType market,
            string marketplaceId,
            long styleItemId)
        {
            IDictionary<string, decimal?> rateForMarketplace;
            using (var db = _dbFactory.GetRWDb())
            {
                rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, styleItemId);
            }

            return GetMarketPrice(sourcePrice,
                sourceSFPPrice,
                isPrime,
                isFBA,
                weight,
                market,
                marketplaceId,
                rateForMarketplace);
        }

        public decimal GetMarketPrice(decimal sourcePrice,
            decimal? sourceSFPPrice,
            bool isPrime,
            bool isFBA,
            double? weight,
            MarketType market,
            string marketplaceId,
            IDictionary<string, decimal?> rateForMarketplace)
        {
            var defaultPrice = GetMarketDefaultPrice(sourcePrice,
                market,
                marketplaceId,
                rateForMarketplace);

            var resultPrice = ApplyMarketSpecified(defaultPrice,
                sourceSFPPrice,
                market,
                marketplaceId,
                weight,
                isPrime,
                isFBA);
            
            return resultPrice ?? sourcePrice;
        }

        public decimal? GetMarketDefaultPrice(decimal sourcePrice,
            MarketType market,
            string marketplaceId,
            IDictionary<string, decimal?> rateForMarketplace)
        {
            var defaultPrice =
                      RateHelper.CalculateForMarket(market,
                        marketplaceId,
                        sourcePrice,
                        rateForMarketplace[MarketplaceKeeper.AmazonComMarketplaceId],
                        rateForMarketplace[MarketplaceKeeper.AmazonCaMarketplaceId],
                        rateForMarketplace[MarketplaceKeeper.AmazonUkMarketplaceId],
                        rateForMarketplace[MarketplaceKeeper.AmazonAuMarketplaceId],
                        RateService.GetMarketShippingAmount(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId),
                        RateService.GetMarketShippingAmount((MarketType)market, marketplaceId),
                        RateService.GetMarketExtraAmount((MarketType)market, marketplaceId));

            return defaultPrice;
        }

        public decimal? ApplyMarketSpecified(decimal? defaultPrice,
            decimal? sourceSFPorFBAPrice,
            MarketType market,
            string marketplaceId,
            double? weight,
            bool isPrime,
            bool isFBA)
        {
            if (market == MarketType.Amazon)
            {
                if (isPrime || isFBA)
                {
                    if (sourceSFPorFBAPrice.HasValue)
                    {
                        return sourceSFPorFBAPrice.Value;// + (weight >= 16 ? 2M : 0M);
                    }
                    else
                    {
                        if (defaultPrice.HasValue)
                            return defaultPrice.Value + GetPrimePart((decimal?)weight);
                    }
                }
            }
            if (market == MarketType.Walmart
                && defaultPrice.HasValue)
            {
                if (isPrime || isFBA)
                {
                    if (sourceSFPorFBAPrice.HasValue)
                    {
                        return sourceSFPorFBAPrice.Value;
                    }
                    else
                    {
                        if (defaultPrice.HasValue)
                            return defaultPrice.Value + (weight >= 16 ? 8M : 6M); //5
                    }
                }
            }
            if (market == MarketType.eBay
                && defaultPrice.HasValue)
            {
                if (weight >= 16) //NOTE: ==OversizeTemplate
                    return defaultPrice.Value + 3.5M;
            }

            return defaultPrice;
        }
    }
}
