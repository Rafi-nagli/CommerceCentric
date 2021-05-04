using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Common.Helpers
{
    public class CustomsHelper
    {
        public class CustomItemInfo
        {
            public string Title { get; set; }
            public string SKU { get; set; }
            public int Quantity { get; set; }
            public decimal Weight { get; set; }
            public decimal ItemPrice { get; set; }
            public decimal PerItemPrice { get; set; }
            public string Country { get; set; }
        }

        public static string GetType(string type, string defaultType)
        {
            //NOTE: exclude for PA
            //"Children Clothes";

            if (defaultType == "Car Parts")
                return defaultType;

            if (defaultType == "Children Clothes")
                return defaultType;

            if (String.IsNullOrEmpty(type))
                return defaultType;

            return type;
        }

        public static IList<CustomItemInfo> BuildCustomLines(IList<DTOOrderItem> items, 
            double orderWeightOz,
            string defaultCustomType,
            bool oneLine = false)
        {
            var nonAdjustedWeight = items.Sum(i => i.Weight * i.Quantity);
            var weightDelta = nonAdjustedWeight - orderWeightOz;
            var itemCount = items.Sum(i => i.Quantity);
            var adjustingWeight = weightDelta / itemCount; //1 / (float)items.Count;

            //Adjustment items weight / convert prices
            foreach (var item in items)
            {
                item.ItemPrice = PriceHelper.RougeConvertToUSD(item.ItemPriceCurrency, item.ItemPrice);
                item.Weight -= adjustingWeight;
            }

            ShippingUtils.FixUpItemPrices(items);

            var lines = CustomsHelper.PrepareLines(items);

            var results = lines.Select(i => new CustomItemInfo
            {
                Title = StringHelper.GetFirstNotEmpty(i.ItemStyle, defaultCustomType),
                Quantity = i.Quantity,
                ItemPrice = i.ItemPrice,
                PerItemPrice = i.Quantity != 0 ? PriceHelper.RoundToTwoPrecision(i.ItemPrice / i.Quantity) : 0,
                Weight = (decimal)i.Weight,
                SKU = i.SKU,
                Country = "US",
            }).ToArray();

            if (oneLine)
            {
                results = results.GroupBy(r => r.Title).Select(r => new CustomItemInfo()
                {
                    Title = r.Key,
                    Quantity = r.Sum(i => i.Quantity),
                    ItemPrice = r.Sum(i => i.ItemPrice),
                    PerItemPrice = r.Sum(i => i.Quantity) != 0 ? 
                        PriceHelper.RoundToTwoPrecision(r.Sum(i => i.ItemPrice) / r.Sum(i => i.Quantity)) 
                        : 0,
                    Weight = r.Sum(i => i.Weight),
                    SKU = r.FirstOrDefault()?.SKU,
                    Country = r.FirstOrDefault()?.Country
                }).ToArray();
            }

            return results;
        }


        private const double MinWeightDeltaThreshold = 0.1;
        private const double AnyWeightDeltaThreshold = double.MaxValue;
        private const decimal MinPriceDeltaThreshold = 0.1M;

        public static IList<DTOOrderItem> PrepareLines(IList<DTOOrderItem> sourceItems)
        {
            var lines = new List<DTOOrderItem>();

            var items = sourceItems.Select(i => new DTOOrderItem()
            {
                ItemPrice = i.ItemPrice,
                ItemStyle = i.ItemStyle,
                Quantity = i.Quantity,
                Weight = i.Weight,
                Title = i.Title,
                SKU = i.SKU,
            }).ToList();

            //Remove excess lines
            while (items.Count > 5)
            {
                var equals = FindByWeightThenPriceEquals(items, MinWeightDeltaThreshold);
                if (equals == null)
                {
                    equals = FindByPriceThenWeightEquals(items, MinPriceDeltaThreshold);
                    if (equals == null)
                        equals = FindByWeightThenPriceEquals(items, AnyWeightDeltaThreshold);
                }

                equals[0].ItemPrice += equals[1].ItemPrice;
                equals[0].Weight = (equals[0].Weight * equals[0].Quantity + equals[1].Weight * equals[1].Quantity) / (equals[0].Quantity + equals[1].Quantity);
                equals[0].Quantity += equals[1].Quantity;

                items.Remove(equals[1]);
            }


            //Compose customs lines
            foreach (var item in items)
            {
                var weight = item.Weight * item.Quantity;
                lines.Add(new DTOOrderItem()
                {
                    Title = "Clothes",
                    Quantity = item.Quantity,
                    ItemPrice = item.ItemPrice,
                    Weight = weight,
                    SKU = item.SKU,
                });
            }

            return lines;
        }



        private static IList<DTOOrderItem> FindByPriceThenWeightEquals(IList<DTOOrderItem> items, decimal minDeltaThreshold)
        {
            var currentPriceDelta = minDeltaThreshold;
            var currentWeightDelta = double.MaxValue;
            DTOOrderItem minFirstItem = null;
            DTOOrderItem minSecondItem = null;
            for (int i = 0; i < items.Count; i++)
            {
                var firstItem = items[i];
                for (int j = i + 1; j < items.Count; j++)
                {
                    var priceDelta = Math.Abs(firstItem.PricePerItem - items[j].PricePerItem);
                    if (MathHelper.Compare(priceDelta, currentPriceDelta) <= 0)
                    {
                        var weightDelta = Math.Abs(firstItem.Weight - items[j].Weight);
                        if (MathHelper.Compare(priceDelta, currentPriceDelta) == 0)
                        {
                            if (weightDelta < currentWeightDelta)
                            {
                                minFirstItem = firstItem;
                                minSecondItem = items[j];
                                currentWeightDelta = weightDelta;
                                currentPriceDelta = priceDelta;
                            }
                            else
                            {
                                //Nothing price the same, weight is better
                            }
                        }
                        else
                        {
                            minFirstItem = firstItem;
                            minSecondItem = items[j];
                            currentWeightDelta = weightDelta;
                            currentPriceDelta = priceDelta;
                        }
                    }
                }
            }
            if (minFirstItem == null
                || minSecondItem == null)
                return null;
            return new List<DTOOrderItem>() { minFirstItem, minSecondItem };
        }

        private static IList<DTOOrderItem> FindByWeightThenPriceEquals(IList<DTOOrderItem> items, double minDeltaThreshold)
        {
            var currentWeightDelta = minDeltaThreshold;
            var currentPriceDelta = decimal.MaxValue;
            DTOOrderItem minFirstItem = null;
            DTOOrderItem minSecondItem = null;
            for (int i = 0; i < items.Count; i++)
            {
                var firstItem = items[i];
                for (int j = i + 1; j < items.Count; j++)
                {
                    var weightDelta = Math.Abs(firstItem.Weight - items[j].Weight);
                    if (MathHelper.Compare(weightDelta, currentWeightDelta) <= 0)
                    {
                        var priceDelta = Math.Abs(firstItem.PricePerItem - items[j].PricePerItem);
                        if (MathHelper.Compare(weightDelta, currentWeightDelta) == 0)
                        {
                            if (priceDelta < currentPriceDelta)
                            {
                                minFirstItem = firstItem;
                                minSecondItem = items[j];
                                currentWeightDelta = weightDelta;
                                currentPriceDelta = priceDelta;
                            }
                            else
                            {
                                //Nothing, weight the same, price is better
                            }
                        }
                        else
                        {
                            minFirstItem = firstItem;
                            minSecondItem = items[j];
                            currentWeightDelta = weightDelta;
                            currentPriceDelta = priceDelta;
                        }
                    }
                }
            }
            if (minFirstItem == null
                || minSecondItem == null)
                return null;
            return new List<DTOOrderItem>() { minFirstItem, minSecondItem };
        }
    }
}
