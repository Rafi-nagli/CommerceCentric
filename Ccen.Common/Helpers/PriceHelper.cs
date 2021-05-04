using System;
using Amazon.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.Common.Helpers
{
    public class PriceHelper
    {
        public const decimal CADtoUSD = 0.71M;        
        public const decimal GBPtoUSD = 1.23M;
        public const decimal EURtoUSD = 1.08M;
        public const decimal MXNtoUSD = 0.041M;
        public const decimal AUDtoUSD = 0.63M;

        public const decimal USDtoCAD = 1 / CADtoUSD;
        public const decimal USDtoAUD = 1/AUDtoUSD;

        public const string USDSymbol = "USD";
        public const string CADSymbol = "CAD";
        public const string GBPSymbol = "GBP";
        public const string MXNSymbol = "MXN";
        public const string EURSymbol = "EUR";
        public const string AUDSymbol = "AUD";

        static public float GetCurrencyIndex(string currency)
        {
            if (currency == "$")
                return 1;
            if (currency == "C$")
                return 2;
            if (currency == "£")
                return 3;
            if (currency == "€")
                return 4;
            if (currency == "A$")
                return 5;
            if (currency == "M$")
                return 6;
            return 10;
        }

        public static decimal RougeConvertToUSD(string currency, decimal? value)
        {
            if (!value.HasValue)
                return 0;
            return RougeConvertToUSD(currency, value.Value);
        }

        static public decimal RougeConvertToUSD(string currency, decimal value)
        {
            if (currency == "GBP" || currency == "£")
                return value * GBPtoUSD;
            if (currency == "CAD" || currency == "C$")
                return value * CADtoUSD;
            if (currency == "EUR" || currency == "€")
                return value * EURtoUSD;
            if (currency == "MXN" || currency == "M$")
                return value * MXNtoUSD;
            if (currency == "AUD" || currency == "A$")
                return value * AUDtoUSD;
            return value;
        }

        static public string FormatCurrency(string currency)
        {
            if (currency == "USD")
                return "$";
            if (currency == "EUR")
                return "€";
            if (currency == "CAD")
                return "C$";
            if (currency == "GBP")
                return "£";
            if (currency == "MXN")
                return "M$";
            if (currency == "AUD")
                return "A$";
            if (String.IsNullOrEmpty(currency))
                return "$";
            return currency;
        }

        static public string GetCurrencySymbol(MarketType market, string marketplaceId)
        {
            if (market == MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return "C$";
                if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    return "M$";
            }

            if (market == MarketType.WalmartCA)
                return "C$";

            if (market == MarketType.AmazonEU)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonDeMarketplaceId)
                    return "€";
                if (marketplaceId == MarketplaceKeeper.AmazonEsMarketplaceId)
                    return "€";
                if (marketplaceId == MarketplaceKeeper.AmazonFrMarketplaceId)
                    return "€";
                if (marketplaceId == MarketplaceKeeper.AmazonItMarketplaceId)
                    return "€";
                if (marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                    return "£";

                return "£";
            }

            if (market == MarketType.AmazonAU)
            {
                return "A$";
            }


            //if (market == MarketType.Walmart)
                //    return "$";
                //if (market == MarketType.Jet
                //    || market == MarketType.Magento
                //    || market == MarketType.Shopify)
                //    return "$";

            return "$";
        }

        static public string GetCurrencyAbbr(MarketType market, string marketplaceId)
        {
            /*<xsd:simpleType name="BaseCurrencyCodeWithDefault">
		        <xsd:restriction base="xsd:string">
			        <xsd:enumeration value="USD"/>
			        <xsd:enumeration value="GBP"/>
			        <xsd:enumeration value="EUR"/>
			        <xsd:enumeration value="JPY"/>
			        <xsd:enumeration value="CAD"/>
			        <xsd:enumeration value="CNY"/>
			        <xsd:enumeration value="INR"/>
			        <xsd:enumeration value="AUD"/>
			        <xsd:enumeration value="BRL"/>
			        <xsd:enumeration value="MXN"/>
			        <xsd:enumeration value="DEFAULT"/>
		        </xsd:restriction>
	        </xsd:simpleType>
             * */

            if (market == MarketType.Amazon)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                    return "CAD";
                if (marketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId)
                    return "MXN";
            }
            if (market == MarketType.WalmartCA)
            {
                return "CAD";
            }
            if (market == MarketType.AmazonEU)
            {
                if (marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                    return "GBP";

                if (marketplaceId == MarketplaceKeeper.AmazonDeMarketplaceId)
                    return "EUR";
                if (marketplaceId == MarketplaceKeeper.AmazonEsMarketplaceId)
                    return "EUR";
                if (marketplaceId == MarketplaceKeeper.AmazonFrMarketplaceId)
                    return "EUR";
                if (marketplaceId == MarketplaceKeeper.AmazonItMarketplaceId)
                    return "EUR";
            }

            if (market == MarketType.AmazonAU)
            {
                return "AUD";
            }

            //if (market == MarketType.Walmart)
            //{
            //    return "USD";
            //}
            //if (market == MarketType.Jet)
            //{
            //    return "USD";
            //}
            //if (market == MarketType.Magento)
            //{
            //    return "USD";
            //}
            //if (market == MarketType.Shopify)
            //{
            //    return "USD";
            //}
            return "USD";
        }

        static public string FormatCurrency(string value, string currency)
        {
            return value != null ? FormatCurrency(currency) + value : String.Empty;
        }

        //static public decimal? Convert(decimal? value, decimal? exchangeRate, bool dec88 = false)
        //{
        //    if (value.HasValue)
        //        return Convert(value.Value, exchangeRate, dec88);
        //    return null;
        //}

        static public decimal GetExchangeRateToUSD(string currency)
        {
            switch (currency)
            {
                case USDSymbol:
                    return 1;
                case CADSymbol:
                    return 1/CADtoUSD;
                case GBPSymbol:
                    return 1/GBPtoUSD;
                case MXNSymbol:
                    return 1/MXNtoUSD;
                case EURSymbol:
                    return 1/EURtoUSD;
                case AUDSymbol:
                    return 1/AUDtoUSD;
                default:
                    throw new Exception("Not supported currency: " + currency);
            }
        }

        static public decimal GetExchangeRateFromUSD(string currency)
        {
            switch (currency)
            {
                case USDSymbol:
                    return 1;
                case CADSymbol:
                    return CADtoUSD;
                case GBPSymbol:
                    return GBPtoUSD;
                case MXNSymbol:
                    return MXNtoUSD;
                case EURSymbol:
                    return EURtoUSD;
                case AUDSymbol:
                    return AUDtoUSD;
                default:
                    throw new Exception("Not supported currency: " + currency);
            }
        }

        static public decimal ConvertToUSD(decimal value, string currency, bool dec99 = false)
        {
            var exchangeRate = GetExchangeRateToUSD(currency);

            return Convert(value, exchangeRate, dec99);
        }

        static public decimal ConvertFromUSD(decimal value, string currency, bool dec99 = false)
        {
            var exchangeRate = GetExchangeRateFromUSD(currency);

            return Convert(value, exchangeRate, dec99);
        }

        static public decimal Convert(decimal value, decimal? exchangeRate, bool dec99 = false)
        {
            if (exchangeRate.HasValue)
            {
                if (dec99)
                {
                    var wholeValue = Math.Floor(value/exchangeRate.Value);
                    wholeValue += 0.99M;
                    return wholeValue;
                }
                else
                {
                    return Math.Round(value / exchangeRate.Value, 2);
                }
            }
            return value;
        }

        static public decimal RoundTo99(decimal value)
        {
            var wholeValue = Math.Floor(value);
            wholeValue += 0.99M;
            return wholeValue;
        }

        static public decimal RoundToFloor99(decimal value)
        {
            var wholeValue = Math.Floor(value);
            if (wholeValue > 1)
                wholeValue = wholeValue - 1;
            wholeValue += 0.99M;
            return wholeValue;
        }

        static public decimal RoundTill89ToFloor99(decimal value)
        {
            var roundedValue = RoundToFloor99(value);
            if (value - roundedValue > 0.89M)
                roundedValue += 1;
            return roundedValue;
        }

        static public decimal? RoundToTwoPrecision(decimal? value)
        {
            if (!value.HasValue)
                return value;

            return RoundToTwoPrecision(value.Value);
        }

        static public double? RoundToTwoPrecision(double? value)
        {
            if (!value.HasValue)
                return value;

            return (double)RoundToTwoPrecision((decimal)value.Value);
        }

        static public decimal RoundToTwoPrecision(decimal value)
        {
            var wholeValue = Math.Floor(value * 100);
            wholeValue = wholeValue/100;
            return wholeValue;
        }

        static public decimal RoundRoundToTwoPrecision(decimal value)
        {
            var wholeValue = Math.Round(value * 100);
            wholeValue = wholeValue / 100;
            return wholeValue;
        }

        static public decimal RoundToCustomPrecision(decimal value,  decimal precision)
        {
            if (precision <= 0 || precision >= 1)
            {
                throw new ArgumentException("Invalid precision Value " + precision);
            }
            var rest = 1 - precision;
            var wholeValue = Math.Floor(value);            
            return wholeValue - rest;
        }

        static public decimal? Max(decimal? price1, decimal? price2)
        {
            if (!price1.HasValue)
                return price2;
            if (!price2.HasValue)
                return price1;

            return Math.Max(price1.Value, price2.Value);
        }

        static public decimal? Min(params decimal?[] prices)
        {
            
            List<decimal> notEmptyPrices = prices
                .ToList()
                .Where(p => p.HasValue)
                .Select(p => p.Value)
                .ToList();

            var minPrice = notEmptyPrices.Min(p => p);

            return minPrice;
        }

        public static decimal? GetDefaultMSRP(string itemStyle)
        {
            /*
             * [8:51:17 AM] Rafi Nagli: т.и. если цена уже стоит больше $25 не трогаем если нет
            [8:51:29 AM] Rafi Nagli: то менем по следующему правилу:
            [8:51:38 AM] Rafi Nagli: 2 piece - $32
            [8:51:45 AM] Rafi Nagli: 3 piece - $34
            [8:51:56 AM] Rafi Nagli: 4 piece - $38
            [8:52:20 AM] Rafi Nagli: nightgowns- $31
             */

            decimal? price = null;

            if (itemStyle == "Pajama – 2pc" || itemStyle == "Pajama – 2pc coat")
                price = 32;

            if (itemStyle == "Pajama – 3pc")
                price = 34;

            if (itemStyle == "Pajama – 4pc")
                price = 36;

            if (itemStyle == "Nightgown")
                price = 31;

            if (itemStyle == "Robe")
                price = 42;

            if (itemStyle == "Footed")
                price = 38;

            if (itemStyle == "Sleepwear Pants")
                price = 26;

            if (itemStyle == "Underwear")
                price = 26;

            return price;
        }


        public static decimal GetSalePriceCorrectionByMarket(MarketType market)
        {
            if (market == MarketType.Walmart)
                return -0.02M;
            if (market == MarketType.WalmartCA)
                return -0.02M;
            if (market == MarketType.Jet)
                return -0.03M;

            return 0.0M;
        }

        public static string PriceToString(decimal? price)
        {
            if (!price.HasValue)
                return "";
            return RoundToTwoPrecision(price.Value).ToString("########0.00");
        }

        public static decimal GetNetPriceForMarketplace(decimal price, 
            MarketType market,
            string marketplaceId)
        {
            if (market == MarketType.Amazon)
            {
                return RoundToTwoPrecision(price * 0.84M);
            }
            if (market == MarketType.eBay)
            {
                return RoundToTwoPrecision(price * 0.87M);
            }
            if (market == MarketType.Walmart)
            {
                return RoundToTwoPrecision(price * 0.85M);
            }
            return price;
        }
    }
}
