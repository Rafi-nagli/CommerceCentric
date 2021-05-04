using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.General
{
    public class ChartPoint
    {
        [Key]
        public long Id { get; set; }
        public long ChartId { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
