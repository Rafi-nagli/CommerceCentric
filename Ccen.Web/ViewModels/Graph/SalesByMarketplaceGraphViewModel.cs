using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Graphs;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.ViewModels.Graph
{
    public class SalesByMarketplaceGraphViewModel
    {
        public enum PeriodType
        {
            Day,
            Week,
            Month
        }

        public enum ValueType
        {
            Unit,
            Price
        }

        public IList<IList<decimal>> PriceSeries { get; set; }
        public IList<IList<int>> UnitSeries { get; set; }
        public IList<string> Labels { get; set; }

        public static SalesByDateGraphViewModel Build(IUnitOfWork db, PeriodType periodType, ValueType valueType)
        {
            var result = new SalesByDateGraphViewModel();
            var items = db.Items.GetSalesInfoByDayAndMarket().ToList();

            var labels = new List<string>();
            IList<int> unitSeries = new List<int>();
            IList<decimal> priceSeries = new List<decimal>();

            var startDate = DateHelper.GetAppNowTime().Date;
            if (periodType == PeriodType.Day)
            {
                startDate = DateHelper.GetAppNowTime().Date;
            }
            if (periodType == PeriodType.Week)
            {
                startDate = startDate.AddDays(-7);
            }
            if (periodType == PeriodType.Month)
            {
                startDate = startDate.AddDays(-31);
            }

            var periodItems = items.Where(it => it.Date >= startDate).ToList();

            periodItems.Where(pi => pi.Market == (int) MarketType.eBay).ToList().ForEach(pi => pi.MarketplaceId = null);

            var byMarket = periodItems.GroupBy(i => new {i.Market, i.MarketplaceId})
                    .Select(i => new
                    {
                        Market = i.Key.Market,
                        MarketplaceId = i.Key.MarketplaceId,
                        Price = i.Sum(j => j.Price),
                        Quantity = i.Sum(j => j.Quantity)
                    });

            foreach (var item in byMarket)
            {
                labels.Add(MarketHelper.GetMarketName(item.Market, item.MarketplaceId));
                unitSeries.Add(item.Quantity);
                priceSeries.Add(item.Price);
            }
            result.PriceSeries = new []{ priceSeries };
            result.UnitSeries = new [] { unitSeries };
            result.Labels = new[] { labels };
            return result;
        }
    }
}