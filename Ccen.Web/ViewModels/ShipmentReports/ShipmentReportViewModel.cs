using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Exports.Attributes;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.OrderReports;

namespace Amazon.Web.ViewModels
{
    public class ShipmentReportViewModel
    {
        public long Id { get; set; }


        [ExcelSerializable("Shipping Date", Order = 1, Width = 25)]
        public DateTime? ShippingDate { get; set; }

        public string CustomerOrderId { get; set; }
        public string OrderId { get; set; }

        [ExcelSerializable("OrderId", Order = 4, Width = 25)]
        public string FormattedOrderId
        {
            get
            {
                if (Market == (int)MarketType.eBay)
                    return OrderId;
                return CustomerOrderId;
            }
        }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        [ExcelSerializable("Market", Order = 3, Width = 25)]
        public string MarketName
        {
            get { return MarketHelper.GetMarketName(Market, MarketplaceId); }
        }

        [ExcelSerializable("Ordered qty", Order = 4, Width = 25)]
        public int TotalOrderedQty { get; set; }

        [ExcelSerializable("Shipping cost", Order = 5, Width = 25)]
        public decimal? TotalUpchargedShippingCost { get; set; }

        //[ExcelSerializable("Shipping cost (w/o upcharges)", Order = 6, Width = 25)]
        public decimal? TotalShippingCost { get; set; }


        public string OrderUrl
        {
            get
            {
                return UrlHelper.GetOrderUrl(OrderId);
            }
        }

        public ShipmentReportViewModel(ShipmentReportItemDTO item)
        {
            Id = item.Id;
            ShippingDate = item.ShippingDate;
            OrderId = item.AmazonIdentifier;
            CustomerOrderId = item.CustomerOrderId;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;

            TotalOrderedQty = item.QuantityOrdered;
            TotalShippingCost = item.ShippingCost;
            TotalUpchargedShippingCost = item.ShippingCost + (item.UpChargeCost ?? 0);
        }

        public static MemoryStream ExportToExcel(ILogService log,
            ITime time,
            IUnitOfWork db,
            ShipmentReportSearchFilterViewModel filter,
            bool isFulfilment)
        {
            //string templateName = AppSettings.OrderReportTemplate;
            var gridItems = GetItems(db, filter);

            var b = new ExportColumnBuilder<ShipmentReportViewModel>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.ShippingDate, "Shipping Date", 25),
                b.Build(p => p.FormattedOrderId, "Order Id", 25),
                b.Build(p => p.MarketName, "Market", 25),
                b.Build(p => p.TotalOrderedQty, "Ordered Quantity", 15),
                b.Build(p => p.TotalUpchargedShippingCost, "Shipping Cost", 20)
            };
            if (isFulfilment)
                columns.Add(b.Build(p => p.TotalShippingCost, "Shipping Cost (w/o upcharge)", 20));

            return ExcelHelper.Export(gridItems.Items,
                columns);
        }


        public static GridResponse<ShipmentReportViewModel> GetItems(IUnitOfWork db,
            ShipmentReportSearchFilterViewModel filter)
        {
            if (filter.LimitCount == 0)
                filter.LimitCount = 50;

            var query = db.ShipmentReports.GetAllAsDto();
            if (filter.FromDate.HasValue)
                query = query.Where(i => i.ShippingDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(i => i.ShippingDate <= filter.ToDate.Value);

            if (!String.IsNullOrEmpty(filter.OrderString))
            {
                var digitalString = StringHelper.GetAllDigitSequences(filter.OrderString);
                query = query.Where(i =>
                    (i.Market != (int)MarketType.eBay
                        && (i.CustomerOrderId.Contains(filter.OrderString)
                        || i.CustomerOrderId.Contains(digitalString)))
                    || (i.Market == (int)MarketType.eBay &&
                        (i.AmazonIdentifier.Contains(filter.OrderString)
                        || i.AmazonIdentifier.Contains(digitalString)))
                  ); //i.CustomerOrderId.Contains(filter.OrderString) || 
            }

            var totalCount = query.Count();

            if (!String.IsNullOrEmpty(filter.SortField))
            {
                switch (filter.SortField)
                {
                    case "ShippingDate":
                        if (filter.SortMode == 0)
                            query = query.OrderBy(s => s.ShippingDate);
                        else
                            query = query.OrderByDescending(s => s.ShippingDate);
                        break;
                    default:
                        query = query.OrderByDescending(s => s.ShippingDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.ShippingDate);
            }

            var itemList = query
                .Skip(filter.StartIndex)
                .Take(filter.LimitCount)
                .ToList()
                .Select(i => new ShipmentReportViewModel(i))
                .ToList();

            return new GridResponse<ShipmentReportViewModel>(itemList, totalCount);
        }


        public static GridResponse<ShipmentReportViewModel> GetAll(IUnitOfWork db,
            ShipmentReportSearchFilterViewModel filter)
        {
            return GetItems(db, filter);
        }
    }
}