using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amazon.Core.Helpers
{
    public class ToStringHelper
    {
        public static string ToString(IEnumerable<string> strList)
        {
            if (strList == null)
                return "";

            return String.Join(", ", strList);
        }


        //TODO: 
        //- Add caching for obj.GetType().GetProperties()
        //- Add ignore attribute
        //- Add prosessing arrays
        public static string ToString(object obj)
        {
            if (obj == null)
                return "[null]";

            var sb = new StringBuilder();

            var handledTypes = new[]
            {
                typeof (Int32),
                typeof (Int32?),
                typeof (long),
                typeof (long?),
                typeof (String),
                typeof (bool),
                typeof (bool?),
                typeof (DateTime),
                typeof (DateTime?),
                typeof (TimeSpan),
                typeof (TimeSpan?),
                typeof (float),
                typeof (float?),
                typeof (double),
                typeof (double?),
                typeof (decimal),
                typeof (decimal?)
            };

            //TODO: caching
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(prop => 
                    handledTypes.Contains(prop.PropertyType) 
                    && prop.CanRead 
                    && prop.CanWrite
                    && prop.GetCustomAttributes(typeof(ToStringIgnoreAttribute), true).Length == 0);

            foreach (PropertyInfo property in properties)
            {
                sb.Append(property.Name);

                sb.Append(": ");

                var value = property.GetValue(obj, null);
                sb.Append(value == null ? "[null]" : value.ToString());

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
