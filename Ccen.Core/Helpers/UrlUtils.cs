using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Helpers
{
    public class UrlUtils
    {
        public static string CombinePath(string part1, string part2)
        {
            if (String.IsNullOrEmpty(part1)
                || String.IsNullOrEmpty(part2))
                return part1 + part2;

            return part1.TrimEnd("/\\ ".ToCharArray()) + "/" + part2.TrimStart(" /\\".ToCharArray());
        }
    }
}
