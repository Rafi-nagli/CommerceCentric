using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class ScanForm
    {
        [Key]
        public long Id { get; set; }

        public string FormId { get; set; }
        public string FileName { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
