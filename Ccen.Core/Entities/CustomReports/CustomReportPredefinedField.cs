using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.CustomReports
{
    public class CustomReportPredefinedField
    { 
        [Key]
        public long Id { get; set; }
        public string EntityName { get; set; }
        public string ColumnName { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Title { get; set; }
        public int? Width { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
