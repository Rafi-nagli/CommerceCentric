using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class DhlECommerceRateDTO
    {
        public long Id { get; set; }
        public string CountryCode { get; set; }
        public int ServiceType { get; set; }
        public int? Zone { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
    }
}
