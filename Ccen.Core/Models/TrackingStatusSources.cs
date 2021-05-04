using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum TrackingStatusSources
    {
        None = 0,
        USPS = 1,
        Stamps = 2,
        CanadaPost = 3,

        Dhl = 10,
    }
}
