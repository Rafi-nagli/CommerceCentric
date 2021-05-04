using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class RateByCountry
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public int Weight { get; set; }
        public string PackageType { get; set; }
        public decimal Cost { get; set; }
        public string ShipmentProvider { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
