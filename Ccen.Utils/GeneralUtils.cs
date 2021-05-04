using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Amazon.Utils
{
    public class GeneralUtils
    {
        public static long TryGetLong(string text, long def)
        {
            long num = 0;
            if (long.TryParse(text, out num))
                return num;
            return def;
        }

        public static int TryGetInt(string text, int def)
        {
            int num = 0;
            if (int.TryParse(text, out num))
                return num;
            return def;
        }

        public static object StripNullStrings(object obj)
        {
            var tp = obj.GetType();
            var props = tp.GetProperties();
            if (props.Length > 0)
            {
                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        var strProp = (string)prop.GetValue(obj, null);
                        if (!string.IsNullOrWhiteSpace(strProp) && strProp == "null")
                        {
                            prop.SetValue(obj, null, null);
                        }
                        if (!string.IsNullOrEmpty(strProp))
                        {
                            prop.SetValue(obj, strProp.Trim(), null);
                        }
                    }
                }
            }
            return obj;
        }

        public static decimal GetPrice(string priceString)
        {
            var trimmed = priceString.Trim(new[] {' ', '$'});
            decimal price;
            if (decimal.TryParse(trimmed, out price))
            {
                return price;
            }
            return 0;
        }

        public static IList<long> IntersectIfNotNull(IList<long> list1, IList<long> list2)
        {
            if (list1 == null)
                return list2;
            return list1.Intersect(list2).ToList();
        }

        public static IList<long> RemoveIntersect(IList<long> list1, IList<long> list2)
        {
            var intersectList = list1.Intersect(list2).ToList();
            foreach (var item in intersectList)
                list1.Remove(item);
            return list1;
        }
    }
}
