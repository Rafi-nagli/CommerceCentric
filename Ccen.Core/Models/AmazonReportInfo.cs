using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class AmazonReportInfo
    {
        public string ReportRequestId { get; set; }
        public string ReportId { get; set; }
        public string ReportFileName { get; set; }

        public int? FeedId { get; set; }
        public bool WasModified { get; set; }
    }
}
