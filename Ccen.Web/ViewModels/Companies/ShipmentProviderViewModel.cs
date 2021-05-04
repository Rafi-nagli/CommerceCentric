using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Models.Settings;

namespace Amazon.Web.ViewModels.Status
{
    public class ShipmentProviderViewModel
    {
        public int Type { get; set; }

        public decimal Balance { get; set; }

        public string ShortName { get; set; }
    }
}