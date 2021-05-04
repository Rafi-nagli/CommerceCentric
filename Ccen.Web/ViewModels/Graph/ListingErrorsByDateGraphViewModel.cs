using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO.Graphs;
using Amazon.Model.Implementation.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Amazon.Web.ViewModels.Graph
{
    public class ListingErrorsByDateGraphViewModel
    {
        public enum PeriodType
        {
            Day,
            Week,
            Month
        }
        
        public IList<IList<int>> UnitSeries { get; set; }
        public IList<IList<string>> Labels { get; set; }

        public static ListingErrorsByDateGraphViewModel Build(IUnitOfWork db, PeriodType periodType)
        {
            var result = new ListingErrorsByDateGraphViewModel();
            var items = db.Items.GetSalesInfoByDayAndMarket().ToList();

            IList<List<string>> labelSeries = new List<List<string>>();
            IList<List<int>> unitSeries = new List<List<int>>();
            IList<string> generalLabelSeries = new List<string>();

            var tags = new List<string>()
            {
                (int)MarketType.Amazon + "_" + MarketplaceKeeper.AmazonComMarketplaceId,
                (int)MarketType.eBay + "_" + MarketplaceKeeper.eBayPA,
                (int)MarketType.Walmart + "_"
            };
            var charts = db.Charts.GetAll().Where(ch => ch.ChartName == ChartHelper.ListingErrorChartName
                && tags.Contains(ch.ChartTag)).ToList();
            var chartIds = charts.Select(ch => ch.Id).ToList();
            var chartPoints = db.ChartPoints.GetAllAsDto().Where(ch => chartIds.Contains(ch.Id)).ToList();

            var startDate = DateHelper.GetAppNowTime().Date;
            //if (periodType == PeriodType.Day)
            {
                var dayCount = 7;

                startDate = startDate.AddDays(5);
                for (int i = dayCount - 1; i >= 0; i--)
                {
                    var date = startDate.AddDays(-i); //Exclude today

                    for (var j = 0; j < tags.Count; j++)
                    {
                        var tag = tags[j];
                        var chartId = charts.FirstOrDefault(ch => ch.ChartTag == tag)?.Id;
                        var value = chartPoints.FirstOrDefault(ch => ch.Date == date && ch.ChartId == chartId)?.Value;
                        unitSeries[j].Add((int)Math.Round(value ?? 0M));
                        labelSeries[j].Add(date.ToString("MM/dd/yyyy"));
                    }
                    generalLabelSeries.Add(date.ToString("MM/dd"));
                }
            }

            result.UnitSeries = unitSeries.ToArray();// new[] { unitSeries1, unitSeries2 };
            result.Labels = labelSeries.ToArray();// new[] { labelSeries1, labelSeries2, generalLabelSeries };

            return result;
        }
    }
}