using Amazon.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IPriceService
    {
        decimal GetPrimePart(decimal? weight);
        decimal GetSourcePrice(decimal currentPrice,
            bool isPrime,
            bool isFBA,
            decimal weight,
            MarketType market,
            string marketplaceId,
            long? styleId,
            long? styleItemId);
        decimal GetMarketPrice(decimal sourcePrice,
            decimal? sourceSFPPrice,
            bool isPrime,
            bool isFBA,
            double? weight,
            MarketType market,
            string marketplaceId,
            long styleItemId);
        decimal GetMarketPrice(decimal sourcePrice,
            decimal? sourceSFPPrice,
            bool isPrime,
            bool isFBA,
            double? weight,
            MarketType market,
            string marketplaceId,
            IDictionary<string, decimal?> rateForMarketplace);
        decimal? GetMarketDefaultPrice(decimal sourcePrice,
            MarketType market,
            string marketplaceId,
            IDictionary<string, decimal?> rateForMarketplace);
        decimal? ApplyMarketSpecified(decimal? defaultPrice,
            decimal? sourceSFPPrice,
            MarketType market,
            string marketplaceId,
            double? weight,
            bool isPrime,
            bool isFBA);
    }
}
