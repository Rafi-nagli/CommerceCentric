using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.CustomReports
{
    public class CustomReportField
    { 
        [Key]
        public long Id { get; set; }
        public long CustomReportId { get; set; }

        public long CustomReportPredefinedFieldId { get; set; }

        public int SortOrder { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
