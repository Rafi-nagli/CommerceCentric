using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Models.Bargains
{
    public class BargainItem
    {
        public string Barcode { get; set; }

        public string Name { get; set; }
        public string WalmartImage { get; set; }

        public decimal? AmazonPrice { get; set; }
        public bool AvailableOnAmazon { get; set; }

        public decimal? WalmartPrice { get; set; }
        public bool AvailableOnWalmart { get; set; }

        public ItemDTO WalmartItem { get; set; }
        public ItemDTO AmazonItem { get; set; }
    }
}
