using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class SkyPostalCityCode
    {
        public string CTRY_ISO_CODE { get; set; }
        public int STATE_CODE { get; set; }
        [Key]
        public int CITY_CODE { get; set; }
        public string STATE_NAME { get; set; }
        public string CITY_NAME { get; set; }
    }
}
