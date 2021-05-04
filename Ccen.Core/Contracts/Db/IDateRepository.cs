using System;
using Amazon.Core.Entities;

namespace Amazon.Core.Contracts.Db
{
    public interface IDateRepository : IRepository<Date>
    {
        DateTime GetOrderShippingDate(TimeSpan? closeDayTime);
        void AddHoliday(USPSHoliday holiday);
        //bool IsInTwoBizDays(DateTime shipDate, DateTime deliveryDate);
        int GetBizDaysCount(DateTime startDate, DateTime endDate);

        DateTime GetMaxBizDayByOffset(DateTime startDate, int bizDayOffset);
        DateTime GetMinBizDayByOffset(DateTime startDate, int bizDayOffset);

        bool IsWorkday(DateTime date);

        int? GetIndexByDate(DateTime date);
        DateTime? GetMaxDateByIndex(int index);
        DateTime? GetMinDateByIndex(int index);
    }
}
