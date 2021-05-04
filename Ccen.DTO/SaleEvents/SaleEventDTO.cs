using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Events
{
    public class SaleEventDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }
        public int Status { get; set; }

        public string Site { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? CutOffDate { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
