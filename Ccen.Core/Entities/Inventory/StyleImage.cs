using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleImage : BaseDateAndByEntity
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public int Type { get; set; }
        public int Category { get; set; }
        public string Image { get; set; }
        public string SourceImage { get; set; }
        public string SourceMarketId { get; set; }
        public string Tag { get; set; }
        public string Tags { get; set; }

        public long? OrderIndex { get; set; }

        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }
    }
}
