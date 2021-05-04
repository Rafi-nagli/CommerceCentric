using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class IBCRate
    {
        [Key]
        public int Id { get; set; }

        public string ServiceType { get; set; }
        public string CountryCode { get; set; }

        public decimal RatePerPiece { get; set; }
        public decimal RatePerPound { get; set; }
    }
}
