using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Amazon.Common.Helpers
{
    public static class StringExtensions
    {
        public static string SplitByUpper(this string s, bool toLowercase = true)
        {
            if (toLowercase)
            {
                var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                return r.Replace(s, "_").ToLower();
            }
            return s;
        }

        //public static string RetrieveStyleId(this string s)
        //{
        //    //var template21 = new Regex(@"^21[a-zA-Z]");
        //    //var templateK = new Regex(@"^K[0-9]");
        //    //if (template21.IsMatch(s) || templateK.IsMatch(s))
        //    //{
        //    //    var index = s.IndexOfAny(new [] { '-', '_', '=' });
        //    //    if (index != -1)
        //    //    {
        //    //        return s.Substring(0, index);
        //    //    }
        //    //}
        //    //return s;
        //    var index = s.LastIndexOfAny(new[] { '-', '_', '=' });
        //    if (index != -1)
        //    {
        //        var substring = s.Substring(0, index);
        //        var postfix = s.Substring(index);
        //        if(s.Length > 0 && )
        //    }

        //}

        public static string RemoveWhitespaces(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return String.Empty;
            return Regex.Replace(s, @"\s+", "");
        }


    }
}
