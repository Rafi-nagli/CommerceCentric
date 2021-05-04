using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum ShippingCalculationStatusEnum
    {
        NoCalculation = 0,
        WoWeightCalculation = 1,
        FullCalculation = 2,
        FullWithNoRateCalculation = 5
    }
}
