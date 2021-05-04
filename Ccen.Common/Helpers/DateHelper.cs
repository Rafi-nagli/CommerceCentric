using System;
using System.Linq;

namespace Amazon.Common.Helpers
{
    public class DateHelper
    {
        public const string DateFormat = "MM/dd/yyyy";
        public const string DateTimeFormat = "MM/dd/yyyy HH:mm";

        private const string AppTimeZone = "Eastern Standard Time";
        private const string AmazonTimeZone = "Pacific Standard Time";

        private static TimeZoneInfo mAppTimeZone;
        private static TimeZoneInfo mAmazonTimeZone;

        public static TimeZoneInfo AppTimeZoneInfo
        {
            get
            {
                if (mAppTimeZone != null)
                    return mAppTimeZone;

                mAppTimeZone = TimeZoneInfo.GetSystemTimeZones().Single(tz => tz.Id == AppTimeZone);
                return mAppTimeZone;
            }
        }

        public static TimeZoneInfo AmazonTimeZoneInfo
        {
            get
            {
                if (mAmazonTimeZone != null)
                    return mAmazonTimeZone;

                mAmazonTimeZone = TimeZoneInfo.GetSystemTimeZones().Single(tz => tz.Id == AmazonTimeZone);
                return mAmazonTimeZone;
            }
        }

        public static readonly DateTime MSSqlMinDateTime = new DateTime(1753, 1, 1);

        public static DateTime? FitTOSQLDateTime(DateTime? date)
        {
            if (!date.HasValue)
                return null;

            if (date < MSSqlMinDateTime)
                return null;

            return date;
        }

        static public DateTime? CheckDateRange(DateTime? date)
        {
            if (!date.HasValue)
                return null;

            if (date == DateTime.MinValue)
                return null;

            //NOTE: not required just in case (previous condition should be enough
            if (date < MSSqlMinDateTime)
                return null;

            return date;
        }

        public static DateTime GetAppNowTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, AppTimeZoneInfo); //Eastern Standard Time
        }

        public static DateTime GetAmazonNowTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, AmazonTimeZoneInfo);
        }

        public static DateTime ConvertUtcToApp(DateTime date)
        {
            try
            {
                return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, AppTimeZoneInfo);
            }
            catch (Exception ex)
            {
                return date;
            }
        }

        public static DateTime ConvertUtcToAmazon(DateTime date)
        {
            return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, AmazonTimeZoneInfo);
        }

        public static DateTime? ConvertUtcToApp(DateTime? date)
        {
            if (date.HasValue)
                return ConvertUtcToApp(date.Value);
            return null;
        }

        public static DateTime? ConvertUtcToAmazon(DateTime? date)
        {
            if (date.HasValue)
                return ConvertUtcToAmazon(date.Value);
            return null;
        }

        public static DateTime ConvertAppToUtc(DateTime date)
        {
            try
            {
                return TimeZoneInfo.ConvertTime(date, AppTimeZoneInfo, TimeZoneInfo.Utc);
            }
            catch
            {
                return date;
            }
        }

        public static DateTime ConvertAppToAmazon(DateTime date)
        {
            return TimeZoneInfo.ConvertTime(date, AmazonTimeZoneInfo, TimeZoneInfo.Utc);
        }

        public static DateTime? ConvertAppToUtc(DateTime? date)
        {
            if (date.HasValue)
                return ConvertAppToUtc(date.Value);
            return null;
        }

        public static DateTime? ConvertAppToAmazon(DateTime? date)
        {
            if (date.HasValue)
                return ConvertAppToAmazon(date.Value);
            return null;
        }

        public static DateTime? FromDateString(string dateString)
        {
            if (String.IsNullOrEmpty(dateString))
                return null;

            DateTime outDate;
            if (DateTime.TryParse(dateString, out outDate))
                return outDate;

            return null;
        }

        public static DateTime? ComposeDateTime(DateTime? date, TimeSpan? time)
        {
            if (!date.HasValue)
                return null;
            if (!time.HasValue)
                return date;

            var dateTime = date.Value.Date;

            return dateTime.Add(time.Value);
        }

        public static string ToDateString(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToString(DateFormat);
            return "";
        }

        public static string ToDateTimeString(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToString(DateTimeFormat);
            return "";
        }

        public static string ConvertToReadableStringAgo(DateTime? dateTime, bool isShort = false)
        {
            if (!dateTime.HasValue)
                return "-";

            return ConvertToReadableString(DateTime.UtcNow - dateTime.Value, isShort);
        }

        public static string ConvertToReadableStringAfter(DateTime? dateTime, bool isShort = false)
        {
            if (!dateTime.HasValue)
                return "-";

            return ConvertToReadableString(dateTime.Value - DateTime.UtcNow, isShort);
        }

        public static string ConvertToReadableString(TimeSpan? timeSpan, bool isShort = false)
        {
            if (!timeSpan.HasValue)
                return "-";

            var minutes = (int) Math.Round(timeSpan.Value.TotalMinutes);
            var hours = (int) Math.Round(timeSpan.Value.TotalHours);
            var days = (int) Math.Round(timeSpan.Value.TotalDays);

            if (Math.Abs(minutes) < 60)
                return minutes + MinuteLabel(Math.Abs(minutes), isShort);

            if (Math.Abs(hours) < 24)
                return hours + HourLabel(Math.Abs(hours), isShort);// + " " + (minutes % 60) + MinuteLabel(Math.Abs(minutes % 60), isShort);

            return days + DayLabel(days, isShort); // + " " + (hours % 60) + HourLabel(Math.Abs(hours % 24), isShort);
        }

        private static string MinuteLabel(int minutes, bool isShort)
        {
            return isShort ? " min" : ((minutes == 1) ? " minute" : " minutes");
        }

        private static string HourLabel(int hours, bool isShort)
        {
            return isShort ? "h" : ((hours == 1) ? " hour" : " hours");
        }

        private static string DayLabel(int days, bool isShort)
        {
            return isShort ? "d" : ((days == 1) ? " day" : " days");
        }


        public static DateTime? SetEndOfDay(DateTime? date)
        {
            if (date.HasValue)
            {
                return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59);
            }
            return null;
        }

        public static DateTime? SetBeginOfDay(DateTime? date)
        {
            if (date.HasValue)
            {
                return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0);
            }
            return null;
        }

        public static DateTime? Min(DateTime? date1, DateTime? date2)
        {
            if (!date2.HasValue)
                return date1;
            if (!date1.HasValue)
                return date2;

            return date1 < date2 ? date1 : date2;
        }

        public static DateTime? Max(DateTime? date1, DateTime? date2)
        {
            if (!date2.HasValue)
                return date1;
            if (!date1.HasValue)
                return date2;

            return date1 > date2 ? date1 : date2;
        }

        public static DateTime GetStartOfWeek(DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }

        public static DateTime GetEndOfWeek(DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }
        
        public static DateTime FindPrevousMonday(DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }
    }
}
