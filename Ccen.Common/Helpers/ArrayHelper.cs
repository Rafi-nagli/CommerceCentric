using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.Helpers
{
    public class ArrayHelper
    {
        public static IList<T> ToArray<T>(params T[] items)
        {
            var results = new List<T>();
            foreach (var i in items)
            {
                if (i != null)
                    results.Add(i);
            }
            return results;
        }
    }
}
