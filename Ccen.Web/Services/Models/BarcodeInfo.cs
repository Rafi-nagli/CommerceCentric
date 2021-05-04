using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.Services.Models
{
    public class BarcodeInfo
    {
        public string Barcode { get; set; }
        public int Quantity { get; set; }

        //Output info
        public string StyleId { get; set; }
        public string Image { get; set; }
        public string Size { get; set; }
    }
}