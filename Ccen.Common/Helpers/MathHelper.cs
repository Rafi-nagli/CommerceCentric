using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Helpers
{
    public class MathHelper
    {
        public static double EpsDouble = 0.0000001;
        public static decimal EpsDecimal = 0.0000001M;

        public static int Compare(double value1, double value2)
        {
            if (Math.Abs(value1 - value2) < EpsDouble)
                return 0;
            if (value1 < value2)
                return -1;
            return 1;
        }

        public static int Compare(decimal value1, decimal value2)
        {
            if (Math.Abs(value1 - value2) < EpsDecimal)
                return 0;
            if (value1 < value2)
                return -1;
            return 1;
        }
    }
}
