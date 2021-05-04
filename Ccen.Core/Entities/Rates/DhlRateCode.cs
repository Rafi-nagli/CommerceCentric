using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class DhlRateCode
    {
        [Key]
        public int Id { get; set; }

        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string RateCode { get; set; }
    }
}
