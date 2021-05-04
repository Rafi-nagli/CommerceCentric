using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class DateRepository : Repository<Date>, IDateRepository
    {
        public DateRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public bool IsWorkday(DateTime date)
        {
            var currentDatePart = date.Date;
            var maxDate = GetMaxBizDayByOffset(currentDatePart, 0);
            if (maxDate - currentDatePart < TimeSpan.FromHours(1))
                return true;
            return false;
        }

        public DateTime GetOrderShippingDate(TimeSpan? closeDayTime)
        {
            var currentDate = DateHelper.GetAppNowTime();
            var currentDatePart = currentDate.Date;

            //NOTE: Disable Saturday ShipDate
            //if (currentDatePart.DayOfWeek == DayOfWeek.Saturday
            //    && currentDate.TimeOfDay < new TimeSpan(19, 0, 0))
            //    return currentDatePart.Date;

            var currentDateOrder = unitOfWork.GetSet<Date>().First(d => d.BizDate == currentDatePart).DateOrder;

            var maxDateForDateOrder = unitOfWork.GetSet<Date>().Where(d => d.DateOrder == currentDateOrder).Max(d => d.BizDate);
            if (maxDateForDateOrder != currentDatePart
                || !closeDayTime.HasValue
                || currentDate.TimeOfDay < closeDayTime)
            {
                return maxDateForDateOrder;
            }
            return unitOfWork.GetSet<Date>()
                .Where(d => d.DateOrder == currentDateOrder + 1)
                .Max(d => d.BizDate);
        }

        public void AddHoliday(USPSHoliday holiday)
        {
            unitOfWork.GetSet<USPSHoliday>().Add(holiday);
            unitOfWork.Commit();
        }

        public DateTime GetMaxBizDayByOffset(DateTime startDate, int bizDayOffset)
        {
            var currentDatePart = startDate.Date;
            var currentDateOrder = unitOfWork.GetSet<Date>().First(d => d.BizDate == currentDatePart).DateOrder;
            currentDateOrder += bizDayOffset;

            var maxDateForOrder = unitOfWork.GetSet<Date>().Where(d => d.DateOrder == currentDateOrder).Max(d => d.BizDate);
            return maxDateForOrder;
        }

        public DateTime GetMinBizDayByOffset(DateTime startDate, int bizDayOffset)
        {
            var currentDatePart = startDate.Date;
            var currentDateOrder = unitOfWork.GetSet<Date>().First(d => d.BizDate == currentDatePart).DateOrder;
            currentDateOrder += bizDayOffset;

            var minDateForOrder = unitOfWork.GetSet<Date>().Where(d => d.DateOrder == currentDateOrder).Min(d => d.BizDate);
            return minDateForOrder;
        }

        public int GetBizDaysCount(DateTime startDate, DateTime endDate)
        {
            var holidays = unitOfWork.GetSet<USPSHoliday>().Count(h => h.Date >= startDate && h.Date <= endDate);
            var weekends = 0;
            var date = startDate;
            while (date <= endDate)
            {
                if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                    weekends++;
                date = date.AddDays(1);
            }
            return (int)Math.Round((endDate - startDate).TotalDays) - holidays - weekends;
        }

        public int? GetIndexByDate(DateTime date)
        {
            return unitOfWork.GetSet<Date>().FirstOrDefault(d => d.BizDate == date)?.DateOrder;
        }

        public DateTime? GetMaxDateByIndex(int index)
        {
            return unitOfWork.GetSet<Date>().Where(d => d.DateOrder == index).Max(d => (DateTime?)d.BizDate);
        }

        public DateTime? GetMinDateByIndex(int index)
        {
            return unitOfWork.GetSet<Date>().Where(d => d.DateOrder == index).Min(d => (DateTime?)d.BizDate);
        }
    }
}
