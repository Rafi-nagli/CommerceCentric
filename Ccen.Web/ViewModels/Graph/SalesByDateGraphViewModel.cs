using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.DTO.Graphs;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Amazon.Web.ViewModels.Graph
{
    public class SalesByDateGraphViewModel
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
            Price,
            Order,
            FBAOrder
        }

        public static List<ValueType> OrderValueTypes => new List<ValueType>() { ValueType.Order, ValueType.FBAOrder };

        public IList<IList<decimal>> PriceSeries { get; set; }
        public IList<IList<int>> UnitSeries { get; set; }
        public IList<IList<string>> Labels { get; set; }

        public static SalesByDateGraphViewModel Build(IUnitOfWork db, PeriodType periodType, ValueType valueType)
        {
            var result = new SalesByDateGraphViewModel();

            var statusListToExclude = valueType == ValueType.FBAOrder ? OrderStatusEnumEx.AllUnshippedWithShipped.ToList() : new List<string>();
            var items = OrderValueTypes.Contains(valueType) ? db.Orders.GetSalesInfoByDayAndMarket(statusListToExclude).ToList() :  db.Items.GetSalesInfoByDayAndMarket().ToList();

            IList<string> labelSeries1 = new List<string>();
            IList<string> labelSeries2 = new List<string>();
            IList<int> unitSeries1 = new List<int>();
            IList<int> unitSeries2 = new List<int>();
            IList<decimal> priceSeries1 = new List<decimal>();
            IList<decimal> priceSeries2 = new List<decimal>();
            IList<string> generalLabelSeries = new List<string>();

            var startDate = DateHelper.GetAppNowTime().Date;
            if (periodType == PeriodType.Day)
            {
                var dayCount = 7 + 5;

                //decimal coefficient = 1.3M;
                decimal coefficient = 0;
                for (var i = 0; i <= 7; i++)
                {
                    var date = startDate.AddDays(-i - 1); //Exclude today
                    var prevDate = date.AddYears(-1).AddDays(1); //Task: lets do weekly and 3 month comparison to be be compared to previous year.
                    var dateItems = items.Where(it => it.Date == date).ToList();
                    var prevDateItems = items.Where(it => it.Date == prevDate).ToList();
                    var currentPeriodSum = dateItems.Sum(d => d.Quantity);
                    var prevPeriodSum = prevDateItems.Sum(d => d.Quantity);
                    if (prevPeriodSum != 0)
                        coefficient += (currentPeriodSum - prevPeriodSum) / (decimal)prevPeriodSum;
                }
                coefficient = 1 + coefficient / 7;

                startDate = startDate.AddDays(5);
                for (int i = dayCount - 1; i >= 0; i--)
                {
                    var date = startDate.AddDays(-i - 1); //Exclude today
                    var prevDate = date.AddYears(-1).AddDays(1); //Task: lets do weekly and 3 month comparison to be be compared to previous year.
                    var dateItems = items.Where(it => it.Date == date).ToList();
                    var prevDateItems = items.Where(it => it.Date == prevDate).ToList();
                    
                    if (i >= 5)
                        priceSeries1.Add(dateItems.Sum(d => d.Price));
                    else
                        priceSeries1.Add(prevDateItems.Sum(d => d.Price) * coefficient);
                    priceSeries2.Add(prevDateItems.Sum(d => d.Price));

                    if (i >= 5)
                        unitSeries1.Add(dateItems.Sum(d => d.Quantity));
                    else
                        unitSeries1.Add((int)((decimal)prevDateItems.Sum(d => d.Quantity) * coefficient));
                    unitSeries2.Add(prevDateItems.Sum(d => d.Quantity));

                    labelSeries1.Add(date.ToString("MM/dd/yyyy"));
                    labelSeries2.Add(prevDate.ToString("MM/dd/yyyy"));
                    generalLabelSeries.Add(date.ToString("MM/dd"));
                }
            }

            if (periodType == PeriodType.Week)
            {
                var weekCount = 12;
                for (int i = weekCount-1; i >= 0; i--)
                {
                    var dateTo = startDate.AddDays(-i*7 - 1); //Exclude today
                    var dateFrom = dateTo.AddDays(-7);
                    var prevDateTo = dateTo.AddYears(-1); //Task: lets do weekly and 3 month comparison to be be compared to previous year.
                    var prevDateFrom = prevDateTo.AddDays(-7);
                    var dateItems = items.Where(it => it.Date < dateTo && it.Date >= dateFrom).OrderBy(x => x.Date).ToList();
                    var prevDateItems = items.Where(it => it.Date < prevDateTo && it.Date >= prevDateFrom).ToList();

                    priceSeries1.Add(dateItems.Sum(d => d.Price));
                    priceSeries2.Add(prevDateItems.Sum(d => d.Price));

                    unitSeries1.Add(dateItems.Sum(d => d.Quantity));
                    unitSeries2.Add(prevDateItems.Sum(d => d.Quantity));

                    labelSeries1.Add(dateFrom.ToString("yyyy") + ": " + dateFrom.ToString("MM/dd") + "-" + dateTo.ToString("MM/dd"));
                    labelSeries2.Add(prevDateFrom.ToString("yyyy") + ": " + prevDateFrom.ToString("MM/dd") + "-" + prevDateTo.ToString("MM/dd"));
                    generalLabelSeries.Add(dateFrom.ToString("MM/dd") + "-" + dateTo.ToString("MM/dd"));
                }
            }

            if (periodType == PeriodType.Month)
            {
                var monthCount = 12;
                for (int i = monthCount; i > 0; i--)
                {
                    var date = startDate.AddMonths(-i);
                    var prevDate = date.AddMonths(-monthCount);
                    var dateItems = items.Where(it => it.Date.Month == date.Month && it.Date.Year == date.Year).ToList();
                    var prevDateItems = items.Where(it => it.Date.Month == prevDate.Month && it.Date.Year == prevDate.Year).ToList();

                    priceSeries1.Add(dateItems.Sum(d => d.Price));
                    priceSeries2.Add(prevDateItems.Sum(d => d.Price));

                    unitSeries1.Add(dateItems.Sum(d => d.Quantity));
                    unitSeries2.Add(prevDateItems.Sum(d => d.Quantity));

                    labelSeries1.Add(date.ToString("MM/yyyy"));
                    labelSeries2.Add(prevDate.ToString("MM/yyyy"));
                    generalLabelSeries.Add(date.ToString("MM/yyyy"));
                }
            }


            result.PriceSeries = new[] { priceSeries1, priceSeries2 };
            result.UnitSeries = new[] { unitSeries1, unitSeries2 };
            result.Labels = new[] { labelSeries1, labelSeries2, generalLabelSeries };

            foreach (var series in result.PriceSeries)
            {
                for (var i = 0; i < series.Count; i++)
                {
                    series[i] = Math.Round(series[i]);
                }
            }

            return result;
        }
    }
}