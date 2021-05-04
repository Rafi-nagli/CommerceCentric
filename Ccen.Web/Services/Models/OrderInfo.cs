using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.Services.Models
{
    public class OrderInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public IList<BarcodeInfo> Barcodes { get; set; }
        public DateTime OrderDate { get; set; }

        public InventoryOrderType Type { get; set; }
    }
}