using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;

namespace Amazon.Common.Helpers
{
    public class AppConfigWrapper : IAppConfigWrapper
    {
        private static IAppConfigWrapper _instance;
        private static readonly object Lock = new object();

        private AppConfigWrapper()
        {

        }

        public static IAppConfigWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppConfigWrapper();
                        }                        
                    }
                }
                return _instance;
            }
        }

        public string GetFilename(string name, string defValue = null)
        {
            try
            {
                string value = null;
                if (ConfigurationManager.AppSettings.AllKeys.Contains(name))
                    value = ConfigurationManager.AppSettings[name];
                else
                    return defValue;

                if (value.StartsWith("~") && System.Web.HttpContext.Current != null)
                    value = System.Web.HttpContext.Current.Server.MapPath(value);

                if (value.StartsWith("."))
                    value = System.Reflection.Assembly.GetEntryAssembly().Location.TrimEnd("\\/".ToCharArray()) +
                            value.TrimStart(".".ToCharArray());
                return value;
            }
            catch (Exception)
            {

            }
            return null;
        }

        public string[] GetStringList(string name)
        {
            string value;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(name))
                value = ConfigurationManager.AppSettings[name];
            else
                return new string[] { };

            if (!String.IsNullOrEmpty(value))
            {
                return value.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[] { };
        }

        private T GetValueInternal<T>(string name, T defValue = default(T))
        {
            string value;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(name))
                value = ConfigurationManager.AppSettings[name];
            else
                return defValue;

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }

            if (typeof(T) == typeof(long))
            {
                long longValue;
                if (long.TryParse(value, out longValue))
                    return (T)(object)longValue;
                return defValue;
            }

            if (typeof(T) == typeof(Int32))
            {
                int intValue;
                if (Int32.TryParse(value, out intValue))
                    return (T)(object)intValue;
                return defValue;
            }

            if (typeof(T) == typeof(float))
            {
                float floatValue;
                if (float.TryParse(value, out floatValue))
                    return (T)(object)floatValue;
                return defValue;
            }

            if (typeof(T) == typeof(double))
            {
                double doubleValue;
                if (double.TryParse(value, out doubleValue))
                    return (T)(object)doubleValue;
                return defValue;
            }

            if (typeof(T) == typeof(DateTime))
            {
                DateTime dateValue;
                if (DateTime.TryParse(value, out dateValue))
                    return (T)(object)dateValue;
                return defValue;
            }

            if (typeof(T) == typeof(TimeSpan))
            {
                TimeSpan timeValue;
                if (TimeSpan.TryParse(value, out timeValue))
                    return (T)(object)timeValue;
                return defValue;
            }

            if (typeof(T) == typeof(bool))
            {
                bool boolValue;
                if (bool.TryParse(value, out boolValue))
                    return (T)(object)boolValue;
                return defValue;
            }

            return defValue;
        }

        public T GetValue<T>(string name)
        {
            return GetValueInner<T>(name, default(T), false);
        }

        public T GetValue<T>(string name, T defValue)
        {
            return GetValueInner<T>(name, defValue, true);
        }

        private T GetValueInner<T>(string name, 
            T defValue,
            bool useDefault)
        {
            string value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains(name))
            {
                value = ConfigurationManager.AppSettings[name];
            } 
            else
            { 
                if (useDefault)
                    return defValue;
                else
                    throw new ArgumentNullException(name, "Invalid setting name in config");
            }
            

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }

            if (typeof(T) == typeof(long))
            {
                long longValue;
                if (long.TryParse(value, out longValue))
                    return (T)(object)longValue;
                return defValue;
            }

            if (typeof(T) == typeof(Int32))
            {
                int intValue;
                if (Int32.TryParse(value, out intValue))
                    return (T)(object)intValue;
                return defValue;
            }

            if (typeof(T) == typeof(float))
            {
                float floatValue;
                if (float.TryParse(value, out floatValue))
                    return (T)(object)floatValue;
                return defValue;
            }

            if (typeof(T) == typeof(double))
            {
                double doubleValue;
                if (double.TryParse(value, out doubleValue))
                    return (T)(object)doubleValue;
                return defValue;
            }

            if (typeof(T) == typeof(DateTime))
            {
                DateTime dateValue;
                if (DateTime.TryParse(value, out dateValue))
                    return (T)(object)dateValue;
                return defValue;
            }

            if (typeof(T) == typeof(TimeSpan))
            {
                TimeSpan timeValue;
                if (TimeSpan.TryParse(value, out timeValue))
                    return (T)(object)timeValue;
                return defValue;
            }

            if (typeof(T) == typeof(bool))
            {
                bool boolValue;
                if (bool.TryParse(value, out boolValue))
                    return (T)(object)boolValue;
                return defValue;
            }

            return defValue;
            //return Instance.GetValueInternal(name, defValue);
        }
    }
}
