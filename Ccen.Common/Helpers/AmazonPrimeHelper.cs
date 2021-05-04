using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.Helpers
{
    public class AmazonPrimeHelper
    {
        public static decimal GetShippingAmount(double? weight)
        {
            if (weight > 16)
            {
                return 8.49M;
            }
            return 7.49M;
        }
    }
}
