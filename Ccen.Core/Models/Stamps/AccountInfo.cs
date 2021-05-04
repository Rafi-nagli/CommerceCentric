using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models.Settings;

namespace Amazon.Core.Models.Stamps
{
    public class AccountInfo
    {
        public ShipmentProviderType ProviderType { get; set; }
        public decimal Balance { get; set; }
    }
}
