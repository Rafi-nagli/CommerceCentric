using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Events
{
    public class SaleEventEntry
    {
        [Key]
        public long Id { get; set; }

        public long SaleEventId { get; set; }

        public long StyleId { get; set; }

        public decimal QuantityPercent { get; set; }

        public decimal Price { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
