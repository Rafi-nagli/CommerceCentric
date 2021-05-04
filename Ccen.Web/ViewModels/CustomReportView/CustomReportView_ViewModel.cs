using Amazon.Web.ViewModels.CustomReports;
using Ccen.Web.ViewModels.CustomReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReportView
{
    public class CustomReportView_ViewModel
    {
        public long ReportId { get; set; }
        public string ReportTitle { get; set; }
        public List<CustomFilterAttribute> FilterAttributes { get; set; }
        public List<CustomReportFieldViewModel> Fields { get; set; }
        public List<CustomReportFilterViewModel> Filters { get; set; }
        
        public string ReportDataType { get; set; }

        public List<string> ValuesList { get; set; }
        public List<long> IdsList { get; set; }

        public string ValuesListString { get; set; }
        public string IdsListString { get; set; }
        public List<CustomReportActionViewModel> Actions { get; set; }

    }

    public class CustomReportActionViewModel
    {
        public string ActionUrl { get; set; }
    }
}