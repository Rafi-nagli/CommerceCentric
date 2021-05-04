using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DTO.BulkEdit
{
    public class BulkEditHistoryDTO
    {
        public long Id { get; set; }
        public long BulkEditOperationId { get; set; }       
        public long EntityId { get; set; }
        public long StyleId { get; set; }        
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime OperationDate { get; set; }
        public string Info { get; set; }
        public string StyleID { get; set; }
        public string StyleName { get; set; }
    }
}
