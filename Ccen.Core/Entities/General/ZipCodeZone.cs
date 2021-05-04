
using System;

namespace Amazon.Core.Entities
{
    public class ZipCodeZone
    {
        public long Id { get; set; }
        public int RangeStart{ get; set; }
        public int RangeEnd { get; set; }
        public int Zone { get; set; }
        public string Description { get; set; }
    }
}
