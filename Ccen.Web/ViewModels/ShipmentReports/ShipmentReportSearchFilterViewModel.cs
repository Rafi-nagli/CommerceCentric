using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels.OrderReports
{
    public class ShipmentReportSearchFilterViewModel
    {
        public int StartIndex { get; set; }
        public int LimitCount { get; set; }
        public string SortField { get; set; }
        public int SortMode { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string OrderString { get; set; }
    }
}