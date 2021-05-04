using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Amazon.DTO.CustomReports
{
    public class CustomReportFilterDTO
    {
        public long Id { get; set; }
        public long CustomReportPredefinedFieldId { get; set; }
        public long CustomReportId { get; set; }        
        public string Operation { get; set; }
        public string Value { get; set; }

        public CustomReportPredefinedFieldDTO CustomReportPredefinedField { get; set; }



        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }

        public object ValueObject { get; set; }
        public IList<FilterOperation> AvailableOperations { get; set; }
    }

    public enum FilterOperation
    {
        [Description("Equals")]
        Equals,
        [Description("Does not equal")]
        NotEquals,
        [Description("Greater")]
        Greater,
        [Description("Less")]
        Less,
        [Description("Contains")]
        Contains,
        [Description("Does not contains")]
        NotContains,
        [Description("Begins with")]
        StartsWith,
        [Description("Ends with")]
        EndsWith,
        [Description("Contains any")]
        ContainsAny,
    }
}
