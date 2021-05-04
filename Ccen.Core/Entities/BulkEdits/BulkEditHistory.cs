using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities.BulkEdits
{

    public class BulkEditHistory
    {
        [Key]
        public long Id { get; set; }
        public long BulkEditOperationId { get; set; }        
        public long EntityId { get; set; }
        public long StyleId { get; set; }
        public long CustomReportPredefinedFieldId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime OperationDate { get; set; }
    }
}
