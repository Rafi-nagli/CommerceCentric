using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Settings
{
    public enum ShipmentProviderType
    {
        None = 0,
        Stamps = 1,
        StampsPriority = 2,
        Amazon = 3,
        Dhl = 4,
        DhlECom = 5,
        IBC = 6,
        FedexSmartPost = 7,
        FedexOneRate = 8,
        FedexGeneral = 9,
        FIMS = 10,
        SkyPostal = 11,
        FirstMile = 12,
    }
}
