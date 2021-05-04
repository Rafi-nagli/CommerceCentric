using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO.Reports;

namespace Amazon.DAL.Repositories
{
    public class ReportRepository : Repository<ReportSource>, IReportRepository
    {
        public ReportRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        private class SalesOrderDTO
        {
            public long StyleId { get; set; }
            public string StyleString { get; set; }
            public int NumberOfSoldUnits { get; set; }
            public decimal? ItemPrice { get; set; }
            public decimal? CurrentPrice { get; set; }
        }

        private IQueryable<SalesOrderDTO> GetSalesInfoByOrder(DateTime fromDate)
        {
            var query = from o in unitOfWork.GetSet<Order>()
                        join oi in unitOfWork.GetSet<OrderItem>() on o.Id equals oi.OrderId
                        join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                        where o.FulfillmentChannel == FulfillmentChannelTypeEx.MFN
                              && o.OrderStatus == OrderStatusEnumEx.Shipped
                              && o.OrderDate >= fromDate
                        select new SalesOrderDTO
                        {
                            StyleId = l.StyleId ?? 0,
                            StyleString = l.StyleString,
                            ItemPrice = oi.ItemPriceInUSD,
                            CurrentPrice = l.CurrentPriceInUSD ?? 0,
                            NumberOfSoldUnits = oi.QuantityOrdered,
                        };
            return query;
        }

        public IList<SalesReportDTO> GetSalesByDateReport(DateTime fromDate)
        {
            var remainingByStyleItem = unitOfWork.GetSet<StyleItemCache>().ToList();

            var groupedByStyle = from s in GetSalesInfoByOrder(fromDate)
                group s by s.StyleId
                into byStyle
                select new SalesReportDTO
                {
                    StyleId = byStyle.Key,
                    StyleString = byStyle.Max(s => s.StyleString),
                    NumberOfSoldUnits = byStyle.Sum(s => s.NumberOfSoldUnits),
                    AveragePrice = byStyle.Average(s => s.ItemPrice),
                    AveragePriceCount = byStyle.Count(),
                    MinCurrentPrice = byStyle.Min(s => s.CurrentPrice),
                    MaxCurrentPrice = byStyle.Max(s => s.CurrentPrice),
                };
            var byStyles = groupedByStyle.ToList();

            foreach (var style in byStyles)
            {
                var styleItems = remainingByStyleItem.Where(s => s.StyleId == style.StyleId).ToList();
                style.RemainingUnits =
                    styleItems.Sum(si => si.InventoryQuantity)
                    - styleItems.Sum(si => si.MarketsSoldQuantityFromDate)
                    - styleItems.Sum(si => si.ScannedSoldQuantityFromDate)
                    - styleItems.Sum(si => si.SentToFBAQuantityFromDate)
                    - styleItems.Sum(si => si.SpecialCaseQuantityFromDate);
            }

            return byStyles;
        }

        public IList<SalesReportDTO> GetSalesByFeatureReport(DateTime fromDate, int featureId)
        {
            var remainingByStyleItem = unitOfWork.GetSet<StyleItemCache>().ToList();
            
            var featureByStyle = (from fv in unitOfWork.GetSet<ViewStyleFeatureValue>()
                                  where fv.FeatureId == featureId
                                  select fv).ToList();

            var groupedByStyle = from s in GetSalesInfoByOrder(fromDate)
                group s by s.StyleId
                into byStyle
                select new SalesReportDTO
                {
                    StyleId = byStyle.Key,
                    StyleString = byStyle.Max(s => s.StyleString),
                    NumberOfSoldUnits = byStyle.Sum(s => s.NumberOfSoldUnits),
                    AveragePrice = byStyle.Average(s => s.ItemPrice),
                    AveragePriceCount = byStyle.Count(),
                    MinCurrentPrice = byStyle.Min(s => s.CurrentPrice),
                    MaxCurrentPrice = byStyle.Max(s => s.CurrentPrice),
                };
            var byStyles = groupedByStyle.ToList();

            foreach (var style in byStyles)
            {
                var styleItems = remainingByStyleItem.Where(s => s.StyleId == style.StyleId).ToList();
                style.RemainingUnits =
                    styleItems.Sum(si => si.InventoryQuantity)
                    - styleItems.Sum(si => si.MarketsSoldQuantityFromDate)
                    - styleItems.Sum(si => si.ScannedSoldQuantityFromDate)
                    - styleItems.Sum(si => si.SentToFBAQuantityFromDate)
                    - styleItems.Sum(si => si.SpecialCaseQuantityFromDate);

                var feature = featureByStyle.FirstOrDefault(fv => fv.Id == style.StyleId);
                if (feature != null)
                    style.FeatureValue = feature.Value;
            }

            var byFeatureQuery = from s in byStyles
                group s by s.FeatureValue
                into byFeature
                select new SalesReportDTO()
                {
                    FeatureValue = byFeature.Key,
                    NumberOfSoldUnits = byFeature.Sum(s => s.NumberOfSoldUnits),
                    AveragePrice = byFeature.Sum(s => s.AveragePrice*s.AveragePriceCount),
                    AveragePriceCount = byFeature.Sum(s => s.AveragePriceCount ?? 0),
                    RemainingUnits = byFeature.Sum(s => s.RemainingUnits),
                    MaxCurrentPrice = byFeature.Max(s => s.MaxCurrentPrice),
                    MinCurrentPrice = byFeature.Min(s => s.MinCurrentPrice),
                };

            var items = byFeatureQuery.ToList();
            foreach (var item in items)
            {
                if (item.AveragePriceCount != 0)
                    item.AveragePrice = item.AveragePrice/item.AveragePriceCount;
            }
            return items;
        }
    }
}
