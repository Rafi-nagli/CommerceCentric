using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Events
{
    public class SaleEvent
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public string Site { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? CutOffDate { get; set; }

        public int Status { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
