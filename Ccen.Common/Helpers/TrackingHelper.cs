using Amazon.Core.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Helpers
{
    public class TrackingHelper
    {
        public static string UndefinedPrefix = "Undefined:";

        public static string BuildUndefinedMessage(string message)
        {
            return UndefinedPrefix + " " + message;
        }

        public static CallResult<int> GetCheckDigit(string trackingNumber)
        {
            //9114 9023 0722 4934 2530 2 4
            //9305 5201 1140 5036 2861 15
            if (trackingNumber.Length != 21
                && trackingNumber.Length != 22)
                return CallResult<int>.Fail("Invalid tracking number format", null);

            if (trackingNumber.Length == 22)
                trackingNumber = trackingNumber.Substring(0, 21);

            int f1 = 0;
            for (var i = 0; i < trackingNumber.Length; i += 2)
            {
                f1 += Int32.Parse("" + trackingNumber[i]);
            }
            int f2 = f1 * 3;
            int f3 = 0;
            for (var i = 1; i < trackingNumber.Length; i += 2)
            {
                f3 += Int32.Parse("" + trackingNumber[i]);
            }
            int f4 = f2 + f3;
            int f5 = (int)Math.Ceiling(f4 / 10.0M) * 10 - f4;
            return CallResult<int>.Success(f5);
        }
    }
}
