using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Helpers
{
    public class LocationHelper
    {
        public static long GetLocationIndex(int isle, int section, int shelf)
        {
            var sortIsle = isle;
            var sortSection = section;
            var sortShelf = shelf;
            if (sortIsle == Int32.MaxValue)
                sortIsle = 9998;
            if (sortSection == Int32.MaxValue)
                sortSection = 9998;
            if (sortShelf == Int32.MaxValue)
                sortShelf = 9998;
            return ((sortIsle + 1L) * 100000000L + (sortSection + 1L) * 10000L + sortShelf + 1L); //NOTE: +1 if Section = 0
        }
    }
}
