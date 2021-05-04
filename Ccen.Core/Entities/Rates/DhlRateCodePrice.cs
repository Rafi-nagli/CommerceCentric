using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class DhlRateCodePrice
    {
        [Key]
        public long Id { get; set; }

        public string ProductCode { get; set; }
        public string Package { get; set; }

        public double Weight { get; set; }
        public string RateCode { get; set; }
        public decimal Price { get; set; }
    }
}
