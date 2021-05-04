using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Utils
{
    public static class ListExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list == null || action == null)
                return;
            foreach (T obj in list)
                action(obj);
        }
    }
}
