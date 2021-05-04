using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.CustomReports
{
    public class CustomReportFilter
    { 
        [Key]
        public long Id { get; set; }
        public long CustomReportId { get; set; }

        public long CustomReportPredefinedFieldId { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }
               

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
