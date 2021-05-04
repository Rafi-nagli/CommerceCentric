using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class DhlRateCodeDTO
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string RateCode { get; set; }
    }
}
