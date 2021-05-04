using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;

namespace Amazon.Core.Models
{
    public class TimeService : ITime
    {
        private IDbFactory _dbFactory;

        public TimeService(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }        

        public DateTime GetAppNowTime()
        {
            return DateHelper.GetAppNowTime();
        }

        public DateTime GetAmazonNowTime()
        {
            return DateHelper.GetAmazonNowTime();
        }

        public DateTime GetUtcTime()
        {
            return DateTime.UtcNow;
        }

        public int GetBizDaysCount(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue
                || !endDate.HasValue)
                return -1;

            return GetBizDaysCount(startDate.Value, endDate.Value);
        }

        public int GetBizDaysCount(DateTime startDate, DateTime endDate)
        {
            if (startDate == DateTime.MinValue
                || endDate == DateTime.MinValue)
                return -1;

            if (startDate < DateHelper.MSSqlMinDateTime
                || endDate < DateHelper.MSSqlMinDateTime)
                return -1;

            using (var db = _dbFactory.GetRDb())
            {
                return db.Dates.GetBizDaysCount(startDate, endDate);
            }
        }

        public int GetBizHoursCount(DateTime startDate, DateTime endDate)
        {
            var bizDayCount = GetBizDaysCount(startDate.Date, endDate.Date);
            //var totalDayCount = Math.Max(0, endDate.Day - startDate.Day - 1);
            //var noBusinessDays = totalDayCount - bizDayCount;

            var bizHours = 0;
            if (IsBusinessDay(startDate))
            {
                if (startDate.Date != endDate.Date)
                {
                    bizHours += (24 - startDate.Hour);
                }
                else
                {
                    return endDate.Hour - startDate.Hour;
                }
            }

            bizHours += Math.Max(0, bizDayCount - 1) * 24;
            if (IsBusinessDay(endDate))
            {
                bizHours += endDate.Hour;
            }
            
            if (bizHours < 0)
                bizHours = 0;

            return bizHours;
        }

        public bool IsBusinessDay(DateTime date)
        {
            using (var db = _dbFactory.GetRDb())
            {
                return db.Dates.IsWorkday(date);
            }
        }

        public DateTime? AddBusinessDays(DateTime? date, int days)
        {
            if (!date.HasValue)
                return date;
            if (days == 0)
                return date;

            if (days > 0)
            {
                var workDays = 0;
                while (workDays < days)
                {
                    date = date.Value.AddDays(1);
                    if (date.Value.DayOfWeek != DayOfWeek.Saturday &&
                        date.Value.DayOfWeek != DayOfWeek.Sunday &&
                        IsBusinessDay(date.Value))
                        workDays++;
                }
                return date;
            }
            else
            {
                var workDays = 0;
                while (workDays > days)
                {
                    date = date.Value.AddDays(-1);
                    if (date.Value.DayOfWeek != DayOfWeek.Saturday &&
                        date.Value.DayOfWeek != DayOfWeek.Sunday &&
                        IsBusinessDay(date.Value))
                        workDays--;
                }
                return date;
            }
        }

        public DateTime? AddBusinessDays2(DateTime? date, int days)
        {
            if (!date.HasValue)
                return date;
            if (days == 0)
                return date;

            using (var db = _dbFactory.GetRDb())
            {
                var dateOrder = db.Dates.GetIndexByDate(date.Value.Date);
                if (dateOrder.HasValue)
                    return db.Dates.GetMaxDateByIndex(dateOrder.Value + days);
            }

            return null;
        }

        public DateTime?[] GetDateRangeOfBusinessDay(DateTime? date)
        {
            if (!date.HasValue)
                return null;
            
            using (var db = _dbFactory.GetRDb())
            {
                var dateOrder = db.Dates.GetIndexByDate(date.Value);
                if (dateOrder.HasValue)
                    return new DateTime?[] { db.Dates.GetMinDateByIndex(dateOrder.Value), db.Dates.GetMaxDateByIndex(dateOrder.Value) };
            }

            return null;
        }
    }
}
