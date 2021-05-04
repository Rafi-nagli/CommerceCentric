using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Mailing
{
    public class ShippingMethodViewModel
    {
        public long Id { get; set; }
        public string ProviderPrefix { get; set; }
        public string Carrier { get; set; }
        public string Name { get; set; }
        public decimal? Rate { get; set; }
    }
}