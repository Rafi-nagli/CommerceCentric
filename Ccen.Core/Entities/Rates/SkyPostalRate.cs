using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class SkyPostalRate
    {
        [Key]
        public long Id { get; set; }
        public int ZoneId { get; set; }
        public decimal Rate { get; set; }
        public decimal Weight { get; set; }
        public int ServiceTypeId { get; set; }
        public int ServiceTypeZoneId { get; set; }
    }
}
