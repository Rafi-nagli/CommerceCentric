using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels.OrderReports
{
    public class OrderReportSearchFilterViewModel
    {
        public int StartIndex { get; set; }
        public int LimitCount { get; set; }
        public string SortField { get; set; }
        public int SortMode { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? DropShipperId { get; set; }
        public int? Market { get; set; }
        public string OrderString { get; set; }

        public string OrderStatus { get; set; }

        public SelectList OrderStatusList
        {
            get
            {
                var statusList = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "Shipped + Unshipped", Value = String.Join(";", OrderStatusEnumEx.AllUnshippedWithShipped) },
                    new SelectListItem() { Text = "Shipped", Value = OrderStatusEnumEx.Shipped },
                    new SelectListItem() { Text = "Unshipped", Value = OrderStatusEnumEx.Unshipped },
                    new SelectListItem() { Text = "Refunded", Value = OrderStatusEnumEx.Refunded },
                };
                return new SelectList(statusList, "Value", "Text");
            }
        }
    }
}