using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Utils
{
    public static class DictionaryExtensions
    {
        public static T TryGetValue<T>(this IDictionary<string, T> dict, string key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return default(T);
        }
    }
}
