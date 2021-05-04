using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class ZipCodeZoneRangeDTO
    {
        public long Id { get; set; }
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }
        public int Zone { get; set; }
        public string Description { get; set; }
    }
}
