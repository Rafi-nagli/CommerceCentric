using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class IBCRateDTO
    {
        public long Id { get; set; }
        public string CountryCode { get; set; }
        public string ServiceType { get; set; }
        public decimal RatePerPiece { get; set; }
        public decimal RatePerPound { get; set; }
    }
}
