using Amazon.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Reports
{
    public class SalesExtReportFiltersViewModel
    {
        public int StartIndex { get; set; }
        public int LimitCount { get; set; }

        public string SortField { get; set; }
        public int SortMode { get; set; }
        
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int? Market { get; set; }
        public string MarketplaceId { get; set; }
        
        public string Keywords { get; set; }
        public List<int> Genders { get; set; }

        public List<int> ItemStyles { get; set; }

        public int? MainLicense { get; set; }
        public int? SubLicense { get; set; }

        public static SalesExtReportFiltersViewModel Empty
        {
            get { return new SalesExtReportFiltersViewModel(); }
        }
    }
}