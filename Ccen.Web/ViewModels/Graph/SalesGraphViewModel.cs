using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Common.Helpers;
using Amazon.DTO.Graphs;
using Amazon.Core.Helpers;

namespace Amazon.Web.ViewModels.Graph
{
    public class SalesGraphViewModel
    {
        public IList<List<int>> Sales { get; set; }
        public IList<string> SeriesLabels { get; set; }
        public IList<string> LabelsX { get; set; } 
        public IList<PriceHistoryDTO> Annotations { get; set; }
        public int Max { get; set; }


        //public static SalesGraphViewModel ComposeByStyleItemId(IUnitOfWork db,
        //    long styleItemId,
        //    int periodValue)
        //{
        //    var labels = new List<string> { };
        //    var seriesLabels = new List<string>();
        //    var allDates = new List<DateTime>();
        //    var series = new List<List<int>>();
        //    var period = (SalesPeriod)periodValue;

        //    DateTime? startDate = null;
        //    DateTime endDate = DateHelper.GetAppNowTime().Date;

        //    //NOTE: marketplaceId specially settings to "", show Sales Graph for all marketplaces into current market
        //    var itemsBySKUQuery = db.Items.GetSalesInfoBySKU();

        //    itemsBySKUQuery = itemsBySKUQuery.Where(q => q.StyleItemId == styleItemId);

        //    if (period != SalesPeriod.Overall)
        //    {
        //        startDate = endDate;
        //        var requestFromDate = startDate.Value;
        //        switch (period)
        //        {
        //            case SalesPeriod.Week:
        //                startDate = endDate.AddDays(-7);
        //                requestFromDate = startDate.Value;
        //                break;
        //            case SalesPeriod.TwoWeeks:
        //                startDate = endDate.AddDays(-14);
        //                requestFromDate = startDate.Value;
        //                break;
        //            case SalesPeriod.TwoMonth:
        //                startDate = endDate.AddMonths(-2);
        //                requestFromDate = startDate.Value.AddDays(-7); //NOTE: with a reserve
        //                break;
        //            case SalesPeriod.Year:
        //                startDate = endDate.AddYears(-1);
        //                requestFromDate = startDate.Value;
        //                break;
        //        }
        //        itemsBySKUQuery = itemsBySKUQuery.Where(q => q.Date >= requestFromDate);
        //    }

        //    var items = itemsBySKUQuery.ToList();

        //    if (!startDate.HasValue)
        //        startDate = items.Min(i => (DateTime?)i.Date);
        //    startDate = startDate ?? endDate;

        //    var date = endDate;
        //    if (period == SalesPeriod.TwoMonth)
        //        date = DateHelper.FindPrevousMonday(endDate); //Begin of week
        //    while (date > startDate.Value.Date)
        //    {
        //        allDates.Insert(0, new DateTime(date.Year, date.Month, date.Day));
        //        labels.Insert(0, DateToLabel(date, period));

        //        switch (period)
        //        {
        //            case SalesPeriod.Week:
        //            case SalesPeriod.TwoWeeks:
        //                date = date.AddDays(-1);
        //                break;
        //            case SalesPeriod.TwoMonth:
        //                date = date.AddDays(-7);
        //                break;
        //            case SalesPeriod.Year:
        //            case SalesPeriod.Overall:
        //                date = date.AddMonths(-1);
        //                break;
        //            default:
        //                throw new NotImplementedException("Sales Period");
        //        }
        //    }
        //    //NOTE: Add start date interval
        //    allDates.Insert(0, new DateTime(date.Year, date.Month, date.Day));
        //    labels.Insert(0, DateToLabel(date, period));
            
        //    var saleGraph = new List<int>();
        //    for (int i = 0; i < allDates.Count; i++)
        //    {
        //        var fromDate = allDates[i];
        //        var toDate = i == allDates.Count - 1 ? endDate.AddDays(1) : allDates[i + 1];

        //        IList<PurchaseByDateDTO> dateItems;
        //        if (period == SalesPeriod.Year || period == SalesPeriod.Overall)
        //            dateItems = items.Where(d => d.Date.Month == allDates[i].Month
        //                                            && d.Date.Year == allDates[i].Year
        //                                            && d.StyleItemId == styleItemId).ToList();
        //        else
        //            dateItems = items.Where(d => d.Date >= fromDate
        //                                            && d.Date < toDate
        //                                            && d.StyleItemId == styleItemId).ToList();


        //        saleGraph.Add(dateItems.Sum(d => d.Quantity));
        //    }

        //    series.Add(saleGraph);

        //    return new SalesGraphViewModel()
        //    {
        //        SeriesLabels = new List<string>() { "" },
        //        Sales = series,
        //        LabelsX = labels,
        //    };
        //}

        public static SalesGraphViewModel ComposeByParams(IUnitOfWork db, 
            MarketType market, 
            string marketplaceId, 
            string[] skuList, 
            long[] styleItemIdList,
            int periodValue)
        {
            var labels = new List<string> { };
            var seriesLabels = new List<string>();
            var allDates = new List<DateTime>();
            var series = new List<List<int>>();
            var period = (SalesPeriod) periodValue;

            DateTime? startDate = null;
            DateTime endDate = DateHelper.GetAppNowTime().Date;

            //NOTE: marketplaceId specially settings to "", show Sales Graph for all marketplaces into current market
            var itemsBySKUQuery = db.Items.GetSalesInfoBySKU();
            
            if (market != MarketType.None)
                itemsBySKUQuery = itemsBySKUQuery.Where(p => p.Market == (int)market);

            if (!String.IsNullOrEmpty(marketplaceId))
                itemsBySKUQuery = itemsBySKUQuery.Where(p => p.MarketplaceId == marketplaceId);

            if (skuList != null && skuList.Any())
                itemsBySKUQuery = itemsBySKUQuery.Where(q => skuList.Contains(q.SKU));

            if (styleItemIdList != null && styleItemIdList.Any())
                itemsBySKUQuery = itemsBySKUQuery.Where(q => styleItemIdList.Contains(q.StyleItemId));

            if (period != SalesPeriod.Overall)
            {
                startDate = endDate;
                var requestFromDate = startDate.Value;
                switch (period)
                {
                    case SalesPeriod.Week:
                        startDate = endDate.AddDays(-7);
                        requestFromDate = startDate.Value;
                        break;
                    case SalesPeriod.TwoWeeks:
                        startDate = endDate.AddDays(-14);
                        requestFromDate = startDate.Value;
                        break;
                    case SalesPeriod.TwoMonth:
                        startDate = endDate.AddMonths(-2);
                        requestFromDate = startDate.Value.AddDays(-7); //NOTE: with a reserve
                        break;
                    case SalesPeriod.Year:
                        startDate = endDate.AddYears(-1);
                        requestFromDate = startDate.Value;
                        break;
                }
                itemsBySKUQuery = itemsBySKUQuery.Where(q => q.Date >= requestFromDate);
            }

            var items = itemsBySKUQuery.ToList();

            if (!startDate.HasValue)
                startDate = items.Min(i => (DateTime?)i.Date);
            startDate = startDate ?? endDate;
            
            var date = endDate;
            if (period == SalesPeriod.TwoMonth)
                date = DateHelper.FindPrevousMonday(endDate); //Begin of week
            while (date > startDate.Value.Date)
            {
                allDates.Insert(0, new DateTime(date.Year, date.Month, date.Day));
                labels.Insert(0, DateToLabel(date, period));
                
                switch (period)
                {
                    case SalesPeriod.Week:
                    case SalesPeriod.TwoWeeks:
                        date = date.AddDays(-1);
                        break;
                    case SalesPeriod.TwoMonth:
                        date = date.AddDays(-7);
                        break;
                    case SalesPeriod.Year:
                    case SalesPeriod.Overall:
                        date = date.AddMonths(-1);
                        break;
                    default:
                        throw new NotImplementedException("Sales Period");
                }
            }
            //NOTE: Add start date interval
            allDates.Insert(0, new DateTime(date.Year, date.Month, date.Day));
            labels.Insert(0, DateToLabel(date, period));

            if (skuList != null)
            {
                foreach (var sku in skuList)
                {
                    var saleGraph = new List<int>();
                    for (int i = 0; i < allDates.Count; i++)
                    {
                        var fromDate = allDates[i];
                        var toDate = i == allDates.Count - 1 ? endDate.AddDays(1) : allDates[i + 1];

                        IList<PurchaseByDateDTO> dateItems;
                        if (period == SalesPeriod.Year || period == SalesPeriod.Overall)
                            dateItems = items.Where(d => d.Date.Month == allDates[i].Month
                                                         && d.Date.Year == allDates[i].Year
                                                         && d.SKU == sku).ToList();
                        else
                            dateItems = items.Where(d => d.Date >= fromDate
                                                         && d.Date < toDate
                                                         && d.SKU == sku).ToList();


                        saleGraph.Add(dateItems.Sum(d => d.Quantity));
                    }

                    series.Add(saleGraph);
                }

                return new SalesGraphViewModel()
                {
                    SeriesLabels = skuList,
                    Sales = series,
                    LabelsX = labels,
                };
            }

            if (styleItemIdList != null)
            {
                var styleItems = db.StyleItems.GetAllAsDto()
                    .Where(si => styleItemIdList.Contains(si.StyleItemId))
                    .ToList()
                    .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                    .ThenBy(si => si.Color)
                    .ThenBy(si => si.StyleItemId)
                    .ToList();

                foreach (var styleItem in styleItems)
                {
                    var saleGraph = new List<int>();
                    for (int i = 0; i < allDates.Count; i++)
                    {
                        var fromDate = allDates[i];
                        var toDate = i == allDates.Count - 1 ? endDate.AddDays(1) : allDates[i + 1];

                        IList<PurchaseByDateDTO> dateItems;
                        if (period == SalesPeriod.Year || period == SalesPeriod.Overall)
                            dateItems = items.Where(d => d.Date.Month == allDates[i].Month
                                                         && d.Date.Year == allDates[i].Year
                                                         && d.StyleItemId == styleItem.StyleItemId).ToList();
                        else
                            dateItems = items.Where(d => d.Date >= fromDate
                                                         && d.Date < toDate
                                                         && d.StyleItemId == styleItem.StyleItemId).ToList();


                        saleGraph.Add(dateItems.Sum(d => d.Quantity));
                    }

                    series.Add(saleGraph);
                }                

                return new SalesGraphViewModel()
                {
                    SeriesLabels = styleItems.Select(si => StringHelper.JoinTwo("/", si.Size, si.Color)).ToList(),
                    Sales = series,
                    LabelsX = labels,
                };
            }
            return null;
        }

        private static string DateToLabel(DateTime date, SalesPeriod period)
        {
            switch (period)
            {
                case SalesPeriod.Week:
                case SalesPeriod.TwoWeeks:
                    return date.ToString("MM/dd");
                    break;
                case SalesPeriod.TwoMonth:
                    return date.ToString("MM/dd");
                    break;
                case SalesPeriod.Year:
                case SalesPeriod.Overall:
                    return date.ToString("yyyy/MM");
                    break;
            }
            return "-";
        }
    }
}