using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.Helpers
{
    public static class DictionaryExtensions
    {
        public static T2 GetValue<T1, T2>(this IDictionary<T1, T2> dict, T1 key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return default(T2);
        }
    }
}
