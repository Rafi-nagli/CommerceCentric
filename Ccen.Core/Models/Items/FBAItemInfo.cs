using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Items
{
    public class FBAItemInfo
    {
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string SKU { get; set; }

        public int Quantity { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
    }
}
