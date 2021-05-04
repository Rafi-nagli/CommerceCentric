using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Graphs;

namespace Amazon.Web.ViewModels.Graph
{
    public class SalesByProductTypeGraphViewModel
    {
        /* Sales by product type – Pie chart. Types
            a.       Girls Nightgowns
            b.       Girls Pajamas
            c.       Baby Girls Nightgowns
            d.       Baby Girls Pajamas
            e.       Boys Pajamas
            f.        Baby-boys pajamas
            g.       Men Pajamas
            h.       Woman nightgowns or pajamas
         */

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
            var items = db.Items.GetSalesInfoByDayAndItemStyle().ToList();

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

            var byMarket = periodItems.GroupBy(i => new {i.ItemStyle})
                    .Select(i => new
                    {
                        ItemStyle = i.Key.ItemStyle,
                        Price = i.Sum(j => j.Price),
                        Quantity = i.Sum(j => j.Quantity < 0 ? 0 : j.Quantity)
                    });

            foreach (var item in byMarket)
            {
                labels.Add(String.IsNullOrEmpty(item.ItemStyle) ? "n/a" : item.ItemStyle);
                unitSeries.Add(item.Quantity);
                priceSeries.Add(item.Price);
            }
            result.PriceSeries = new []{ priceSeries };
            result.UnitSeries = new [] { unitSeries };
            result.Labels = new [] { labels };
            return result;
        }
    }
}