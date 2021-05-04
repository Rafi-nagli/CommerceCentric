using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class DhlRateCodePriceDTO
    {
        public long Id { get; set; }

        public string ProductCode { get; set; }
        public string Package { get; set; }
        public double Weight { get; set; }
        public string RateCode { get; set; }
        public decimal Price { get; set; }
    }
}
