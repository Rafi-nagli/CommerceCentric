using Amazon.Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.Settings
{
    public class ShippingChargeViewModel
    {
        public long ShippingChargeId { get; set; }
        public int ShippingMethodId { get; set; }
        public string ShippingMethodName { get; set; }
        public int ShippingProviderType { get; set; }
        public decimal? ShippingChargePercent { get; set; }

        public string ShippingProviderName
        {
            get
            {
                return ((ShipmentProviderType)ShippingProviderType).ToString();
            }
        }
    }
}