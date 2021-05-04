using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Helpers
{
    public class WeightHelper
    {
        static public string FormatWeight(double weight)
        {
            var lb = ((int)weight)/16;
            var oz = weight%16;

            if (weight == 0)
                return String.Empty;

            return lb > 0
                ? lb + "lb " + oz.ToString("F2") + "oz"
                : oz.ToString("F2") + "oz";
        }
    }
}
