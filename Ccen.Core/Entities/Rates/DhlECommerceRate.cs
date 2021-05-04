using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class DhlECommerceRate
    {
        [Key]
        public long Id { get; set; }

        public int ServiceType { get; set; }
        public string CountryCode { get; set; }
        public int? Zone { get; set; }

        public decimal Rate { get; set; }
        public decimal Weight { get; set; }
    }
}
