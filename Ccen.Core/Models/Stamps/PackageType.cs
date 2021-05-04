using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum PackageTypeCode
    {
        None = 0,
        Flat = 1, //FlatRateEnvelope
        LargeEnvelopeOrFlat = 2,
        Regular = 3, //Package
        RegionalRateBoxA = 4,
    }
}
