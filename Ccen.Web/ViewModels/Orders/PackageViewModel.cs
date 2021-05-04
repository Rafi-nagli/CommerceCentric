using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Orders
{
    public class PackageViewModel
    {
        public long ShippingId { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
    }
}