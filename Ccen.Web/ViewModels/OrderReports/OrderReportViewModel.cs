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
using Ccen.Web;

namespace Amazon.Web.ViewModels
{
    public class OrderReportViewModel
    {
        public long Id { get; set; }

        //[ExcelSerializable("DropShipperId", Order = 0, Width = 25)]
        public string DropShipperName { get; set; }

        public long? DropShipperId { get; set; }

        [ExcelSerializable("OrderDate", Order = 1, Width = 25)]
        public DateTime OrderDate { get; set; }

        //OrderId
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string OrderId { get; set; }

        public string CustomerOrderId { get; set; }
        //[ExcelSerializable("DSOrderId", Order = 5, Width = 25)]
        public string DSOrderId { get; set; }
        public string MarketOrderId { get; set; }

        [ExcelSerializable("Tracking #", Order = 5, Width = 25)]
        public string TrackingNumber { get; set; }

        public string ShippingMethodName { get; set; }

        public string CarrierName { get; set; }

        [ExcelSerializable("Shipping Service", Order = 6, Width = 25)]
        public string ShippingMethodFormatted
        {
            get { return StringHelper.JoinTwo(" ", CarrierName, ShippingMethodName) ?? ""; }
        }


        [ExcelSerializable("PersonName", Order = 7, Width = 25)]
        public string PersonName { get; set; }

        [ExcelSerializable("City", Order = 8, Width = 25)]
        public string ShippingCity { get; set; }

        [ExcelSerializable("State", Order = 9, Width = 25)]
        public string ShippingState { get; set; }

        [ExcelSerializable("QuantityOrdered", Order = 10, Width = 25)]
        public int QuantityOrdered { get; set; }

        [ExcelSerializable("Model", Order = 11, Width = 25)]
        public string Model { get; set; }

        [ExcelSerializable("ItemPaid", Order = 12, Width = 25)]
        public decimal ItemPaid { get; set; }

        [ExcelSerializable("ShippingPaid", Order = 13, Width = 25)]
        public decimal ShippingPaid { get; set; }

        [ExcelSerializable("Cost", Order = 14, Width = 25)]
        public decimal Cost { get; set; }

        [ExcelSerializable("ItemTax", Order = 15, Width = 25)]
        public decimal ItemTax { get; set; }

        [ExcelSerializable("ShippingTax", Order = 16, Width = 25)]
        public decimal ShippingTax { get; set; }

        [ExcelSerializable("ItemRefunded", Order = 17, Width = 25)]
        public decimal ItemRefunded { get; set; }

        [ExcelSerializable("ShippingRefunded", Order = 18, Width = 25)]
        public decimal ShippingRefunded { get; set; }

        [ExcelSerializable("ShippingCost", Order = 19, Width = 25)]
        public decimal? ShippingCost { get; set; }

        public string OrderStatus { get; set; }

        public string MarketOrderUrl
        {
            get
            {
                return UrlHelper.GetSellerCentralOrderUrl((MarketType)Market, MarketplaceId, MarketOrderId);
            }
        }

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

        [ExcelSerializable("Market", Order = 0, Width = 25)]
        public string FormattedMarket
        {
            get { return MarketHelper.GetMarketName(Market, MarketplaceId); }
        }

        [ExcelSerializable("Transaction", Order = 3, Width = 25)]
        public string FormattedPaidStatus
        {
            get
            {
                if (OrderStatus == OrderStatusEnumEx.Canceled)
                {
                    return "Canceled";
                }
                if (OrderStatus == "Refund")
                {
                    return "Refund";
                }
                if (OrderStatus == "Pending")
                {
                    return "Pending";
                }
                return "Paid";
            }
        }

        [ExcelSerializable("OrderStatus", Order = 2, Width = 25)]
        public string FormattedOrderStatus
        {
            get
            {
                //if (OrderStatus == OrderStatusEnumEx.Canceled)
                //{
                //    return "Canceled";
                //}
                //if (ItemRefunded + ShippingRefunded == 
                //    ItemPaid + ShippingPaid)
                //{
                //    return "Refunded";
                //}

                //if (ItemRefunded > 0
                //    || ShippingRefunded > 0)
                //{
                //    return "Partially Refunded";
                //}
                if (OrderStatus == "Refund")
                {
                    return "";
                }

                return OrderStatus;
            }
        }

        public OrderReportViewModel(OrderReportItemDTO item)
        {
            Id = item.Id;
            DropShipperName = item.DropShipperName;
            DropShipperId = item.DropShipperId;
            OrderDate = item.OrderDate;
            OrderId = item.AmazonIdentifier;
            CustomerOrderId = item.CustomerOrderId;
            MarketOrderId = item.MarketOrderId;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;
            PersonName = item.PersonName;
            ShippingCity = item.ShippingCity;
            ShippingState = item.ShippingState;
            DSOrderId = item.DSOrderId;
            TrackingNumber = item.TrackingNumber;
            CarrierName = item.CarrierName;
            ShippingMethodName = item.ShippingMethodName;

            Model = item.Model;
            QuantityOrdered = item.QuantityOrdered;
            ItemPaid = item.ItemPaid ?? 0;
            ShippingPaid = item.ShippingPaid ?? 0;
            Cost = item.Cost ?? 0;
            ItemTax = item.ItemTax ?? 0;
            ShippingTax = item.ShippingTax ?? 0;
            OrderStatus = item.OrderStatus;
            ShippingCost = item.ShippingCost + (item.UpChargeCost ?? 0);

            if (item.OrderStatus == OrderStatusEnumEx.Canceled)
                ShippingRefunded = item.ShippingPaid ?? 0;
            else
                ShippingRefunded = item.ShippingRefunded ?? 0;

            if (item.OrderStatus == OrderStatusEnumEx.Canceled)
                ItemRefunded = item.ItemPaid ?? 0;
            else
                ItemRefunded = item.ItemRefunded ?? 0;
        }

        public static MemoryStream ExportToExcel(ILogService log,
            ITime time,
            IUnitOfWork db,
            OrderReportSearchFilterViewModel filter)
        {
            var templateName = AppSettings.OrderReportTemplate;
            var gridItems = GetItems(db, filter);

            return ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(templateName),
                "Template",
                gridItems.Items,
                null,
                1);
        }


        public static GridResponse<OrderReportViewModel> GetItems(IUnitOfWork db,
            OrderReportSearchFilterViewModel filter)
        {
            if (filter.LimitCount == 0)
                filter.LimitCount = 50;

            var query = db.OrderReports.GetAllAsDto();
            if (filter.FromDate.HasValue)
                query = query.Where(i => i.OrderDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(i => i.OrderDate <= filter.ToDate.Value);

            if (filter.DropShipperId.HasValue)
                query = query.Where(i => i.DropShipperId == filter.DropShipperId.Value);

            if (filter.Market.HasValue)
                query = query.Where(i => i.Market == filter.Market.Value);

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

            if (!String.IsNullOrEmpty(filter.OrderStatus))
            {
                var orderStatuses = new List<string>();
                if (filter.OrderStatus == OrderStatusEnumEx.Canceled)
                {
                    orderStatuses = new List<string>() { OrderStatusEnumEx.Canceled };
                    query = query.Where(i => orderStatuses.Contains(i.OrderStatus)
                        || ((i.ItemRefunded ?? 0) + (i.ShippingRefunded ?? 0)
                            == (i.ItemPaid ?? 0) + (i.ShippingPaid ?? 0)));
                }
                if (filter.OrderStatus == OrderStatusEnumEx.Refunded)
                {
                    orderStatuses = OrderStatusEnumEx.AllUnshippedWithShipped.ToList();
                    query = query.Where(i =>
                             //orderStatuses.Contains(i.OrderStatus)
                             //    && ((i.ItemRefunded ?? 0) + (i.ShippingRefunded ?? 0)) > 0) ||
                             (i.OrderStatus == "Refund"));
                }
                if (filter.OrderStatus == OrderStatusEnumEx.Shipped)
                {
                    orderStatuses = new List<string>() { OrderStatusEnumEx.Shipped };
                    query = query.Where(i => orderStatuses.Contains(i.OrderStatus));
                }
                if (filter.OrderStatus == OrderStatusEnumEx.Unshipped)
                {
                    orderStatuses = new List<string>() { OrderStatusEnumEx.Unshipped };
                    query = query.Where(i => orderStatuses.Contains(i.OrderStatus));
                }
                if (filter.OrderStatus == String.Join(";", OrderStatusEnumEx.AllUnshippedWithShipped))
                {
                    orderStatuses = OrderStatusEnumEx.AllUnshippedWithShipped.ToList();
                    orderStatuses.Add("Refund");
                    query = query.Where(i => orderStatuses.Contains(i.OrderStatus));
                }

            }

            var totalCount = query.Count();

            if (!String.IsNullOrEmpty(filter.SortField))
            {
                switch (filter.SortField)
                {
                    case "DropShipperName":
                        if (filter.SortMode == 0)
                            query = query.OrderBy(s => s.DropShipperName).ThenByDescending(s => s.OrderDate);
                        else
                            query = query.OrderByDescending(s => s.DropShipperName).ThenByDescending(s => s.OrderDate);
                        break;
                    case "OrderDate":
                        if (filter.SortMode == 0)
                            query = query.OrderBy(s => s.OrderDate);
                        else
                            query = query.OrderByDescending(s => s.OrderDate);
                        break;
                    case "ShippingState":
                        if (filter.SortMode == 0)
                            query = query.OrderBy(s => s.ShippingState).ThenByDescending(s => s.OrderDate);
                        else
                            query = query.OrderByDescending(s => s.ShippingState).ThenByDescending(s => s.OrderDate);
                        break;
                    default:
                        query = query.OrderByDescending(s => s.OrderDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.OrderDate);
            }

            var itemList = query
                .Skip(filter.StartIndex)
                .Take(filter.LimitCount)
                .ToList()
                .Select(i => new OrderReportViewModel(i))
                .ToList();

            return new GridResponse<OrderReportViewModel>(itemList, totalCount);
        }


        public static GridResponse<OrderReportViewModel> GetAll(IUnitOfWork db,
            OrderReportSearchFilterViewModel filter)
        {
            return GetItems(db, filter);
        }
    }
}