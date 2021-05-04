using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Rates
{
    public class DhlGBZipCodeZone
    {
        [Key]
        public long Id { get; set; }

        public string Facility { get; set; }
        public string Zip { get; set; }
        public string Lookup { get; set; }
        public int Zone { get; set; }
    }
}
