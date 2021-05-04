using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class RateByCountryDTO
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public string PackageType { get; set; }
        public int Weight { get; set; }
        public decimal Cost { get; set; }
        public string ShipmentProvider { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
