using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;

namespace Amazon.Common.Helpers
{
    public class SettingsHelper
    {
        public const string KeyOversoldToNotifiers = "OversoldToNotifiers";
        public const string KeyOversoldCcNotifiers = "OversoldCcNotifiers";

        public static DateTime? GetDateTimeSetting(IDbFactory dbFactory, string key)
        {
            try
            {
                using (var db = dbFactory.GetRDb())
                {
                    var setting = db.Settings.GetByName(key);
                    if (setting == null)
                        return null;
                    return GetDateTimeValue(setting.Value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static TimeSpan? GetTimeSetting(IDbFactory dbFactory, string key)
        {
            try
            {
                using (var db = dbFactory.GetRDb())
                {
                    var setting = db.Settings.GetByName(key);
                    if (setting == null)
                        return null;
                    return GetTimeValue(setting.Value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DateTime? GetDateTimeSetting(IList<Setting> settings, string key)
        {
            if (settings == null)
                return null;
            
            var setting = settings.FirstOrDefault(s => s.Name == key);
            if (setting == null)
                return null;
            return GetDateTimeValue(setting.Value);
        }

        public static DateTime? GetDateTimeValue(string value)
        {
            var str = value;
            if (String.IsNullOrEmpty(str))
                return null;

            return DateTime.Parse(str);
        }

        public static TimeSpan? GetTimeValue(string value)
        {
            var str = value;
            if (String.IsNullOrEmpty(str))
                return null;

            return TimeSpan.Parse(str);
        }


        public static bool? GetInProgressSetting(IDbFactory dbFactory, string key)
        {
            try
            {
                using (var db = dbFactory.GetRDb())
                {
                    var setting = db.Settings.GetByName(key);
                    if (setting == null)
                        return null;
                    var inProgress = GetBoolValue(setting.Value);
                    if (inProgress == true)
                    {
                        //In-progress can't be more than 2 hours (somethings goes wrong)
                        if (!setting.UpdateDate.HasValue || setting.UpdateDate.Value.AddHours(2) < DateTime.UtcNow)
                            return false;
                    }
                    return inProgress;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool? GetBoolSetting(IDbFactory dbFactory, string key)
        {
            try
            {
                using (var db = dbFactory.GetRDb())
                {
                    var setting = db.Settings.GetByName(key);
                    if (setting == null)
                        return null;
                    return GetBoolValue(setting.Value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool? GetBoolSetting(IList<Setting> settings, string key)
        {
            if (settings == null)
                return null;

            var setting = settings.FirstOrDefault(s => s.Name == key);
            if (setting == null)
                return null;
            return GetBoolValue(setting.Value);
        }

        public static bool? GetBoolValue(string value)
        {
            var str = value;
            if (String.IsNullOrEmpty(str))
                return null;

            return bool.Parse(str);
        }

        public static int? GetIntSetting(IDbFactory dbFactory, string key)
        {
            try
            {
                using (var db = dbFactory.GetRDb())
                {
                    var setting = db.Settings.GetByName(key);
                    if (setting == null)
                        return null;
                    return GetIntValue(setting.Value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetStrSetting(IDbFactory dbFactory, string key)
        {
            try
            {
                using (var db = dbFactory.GetRDb())
                {
                    var setting = db.Settings.GetByName(key);
                    if (setting == null)
                        return null;
                    return setting.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int? GetIntSetting(IList<Setting> settings, string key)
        {
            if (settings == null)
                return null;

            var setting = settings.FirstOrDefault(s => s.Name == key);
            if (setting == null)
                return null;
            return GetIntValue(setting.Value);
        }

        public static int? GetIntValue(string value)
        {
            var str = value;
            if (String.IsNullOrEmpty(str))
                return null;

            return Int32.Parse(str);
        }


        public static bool SetDateTimeSetting(IDbFactory dbFactory, string key, DateTime? value)
        {
            try
            {
                using (var db = dbFactory.GetRWDb())
                {
                    var str = value.HasValue
                        ? value.Value.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))
                        : null;
                    db.Settings.Set(key, str);
                    db.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SetTimeSetting(IDbFactory dbFactory, string key, TimeSpan? value)
        {
            try
            {
                using (var db = dbFactory.GetRWDb())
                {
                    var str = value.HasValue
                        ? value.Value.ToString()
                        : null;
                    db.Settings.Set(key, str);
                    db.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SetBoolSetting(IDbFactory dbFactory, string key, bool? value)
        {
            try
            {
                using (var db = dbFactory.GetRWDb())
                {
                    var str = value.HasValue
                        ? value.Value.ToString()
                        : null;
                    db.Settings.Set(key, str);
                    db.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SetIntSetting(IDbFactory dbFactory, string key, int? value)
        {
            try
            {
                using (var db = dbFactory.GetRWDb())
                {
                    var str = value.HasValue
                        ? value.Value.ToString()
                        : null;
                    db.Settings.Set(key, str);
                    db.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SetStrSetting(IDbFactory dbFactory, string key, string value)
        {
            try
            {
                using (var db = dbFactory.GetRWDb())
                {
                    var str = value;
                    db.Settings.Set(key, str);
                    db.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
