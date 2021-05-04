using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class ReportRequestDTO
    {
        public string Status { get; set; }
        public string RequestId { get; set; }
        public string ReportId { get; set; }
        public string FileName { get; set; }
    }
}
