using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ITime
    {
        DateTime GetAppNowTime();
        DateTime GetAmazonNowTime();
        DateTime GetUtcTime();
        DateTime? AddBusinessDays(DateTime? date, int days);
        DateTime? AddBusinessDays2(DateTime? date, int days);
        int GetBizDaysCount(DateTime? startDate, DateTime? endDate);
        int GetBizDaysCount(DateTime startDate, DateTime endDate);
        int GetBizHoursCount(DateTime startDate, DateTime endDate);

        bool IsBusinessDay(DateTime date);
        DateTime?[] GetDateRangeOfBusinessDay(DateTime? date);

        //DateTime? ConvertTimezone(DateTime? date, string fromTimezone, string toTimezone);
    }
}
