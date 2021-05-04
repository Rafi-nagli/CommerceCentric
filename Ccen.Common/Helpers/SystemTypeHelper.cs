using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Common.Helpers
{
    /// <summary>
    /// Gets Type by system name or alias Ignore Case 
    /// </summary>
    public class SystemTypeHelper
    {
        public static Type GetSystemTypeFromAlias(string alias)
        {
            if (alias == "feature")
            {
                return typeof(int);
            }
            var mscorlib = typeof(string).Assembly;
            var types = mscorlib.GetTypes()
                                .Where(x => x.Namespace == "System");
            var type = types.FirstOrDefault(x => x.Name.ToLower() == alias.ToLower());
            if (type == null)
            {
                throw new ArgumentException("System does not contain type " + alias);
            }
            return type;
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static object GetDefault(string alias)
        {
            return GetDefault(GetSystemTypeFromAlias(alias));
        }

        public static object GetObjectByString(string value, Type type)
        {
            return Convert.ChangeType(value, type);
        }

        public static object GetObjectByString(string value, string alias, bool multi = false)
        {
            if (multi)
            {
                return String.IsNullOrEmpty(value) ? new List<int>() : value.Split(',').Select(x => int.Parse(x)).ToList();
            }
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }
            return Convert.ChangeType(value, GetSystemTypeFromAlias(alias), CultureInfo.InvariantCulture);
        }
    }
}
