using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.DAL;
using Ccen.Model.USHolidayService;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallHolidaysProcessing
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public CallHolidaysProcessing(IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        private bool IsHoliday(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday
                || date.DayOfWeek == DayOfWeek.Sunday)
                return true;

            if (date == new DateTime(2020, 1, 1)
                || date == new DateTime(2020, 7, 4)
                || date == new DateTime(2020, 11, 28)
                || date == new DateTime(2020, 12, 25))
            {
                return true;
            }

            return false;
        }

        public void AddDates()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var dbStartItem = db.Dates.GetAll().OrderByDescending(d => d.BizDate).FirstOrDefault();
                var startDate = dbStartItem.BizDate;
                var startDateOrder = dbStartItem.DateOrder;
                var endDate = startDate.AddYears(1);
                var prevDate = startDate;
                var prevDateOrder = startDateOrder;
                var isPrevDateHoliday = IsHoliday(prevDate);
                startDate = startDate.AddDays(1);

                while (startDate < endDate)
                {
                    var bizDate = startDate;

                    var dbDateItem = db.Dates.GetAll().FirstOrDefault(d => d.BizDate == bizDate);
                    if (dbDateItem != null)
                    {
                        throw new Exception("Duplicate BizDate: " + bizDate);
                    }

                    if (!IsHoliday(prevDate))
                    {
                        startDateOrder += 1;
                    }

                    var dbDate = new Date()
                    {
                        BizDate = startDate,
                        DateOrder = startDateOrder,
                        Deleted = false
                    };
                    _log.Info("Added BizDate=" + startDate + ", DateOrder=" + startDateOrder);
                    db.Dates.Add(dbDate);                    

                    prevDate = startDate;
                    prevDateOrder = startDateOrder;
                    startDate = startDate.AddDays(1);
                }
                db.Commit();
            }
        }

        public void GetHolidays()
        {
            //SettingsHelper.SetIsSyncOrdersEnabled(Db, value);
            USHolidayServiceSoap holidayClient = new USHolidayServiceSoapClient();
            for (var i = 2014; i < 2040; i++)
            {
                var holidaySet = holidayClient.GetHolidaysForYear(i);
                //var holidays = new List<Holiday>();
                var table = holidaySet.Tables["Holidays"];
                using (var db = new UnitOfWork(_log))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var holiday = new Holiday
                        {
                            Name = row["Name"].ToString(),
                            Key = row["Key"].ToString(),
                            Date = DateTime.Parse(row["Date"].ToString())
                        };
                        if (USPSHolidayKeys.Contains(holiday.Key))
                        {
                            db.Dates.AddHoliday(new USPSHoliday
                            {
                                Name = holiday.Name,
                                Key = holiday.Key,
                                Date = holiday.Date
                            });
                        }
                    }
                }
            }

        }

        public class Holiday
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public DateTime Date { get; set; }
        }

        public static List<string> USPSHolidayKeys = new List<string>
        {
            "NEW_YEARS",
            "MLK",
            "GEORGE_WASHINGTON_BDAY",
            "MEMORIAL",
            "INDEPENDENCE",
            "LABOR",
            "COLUMBUS",
            "VETERANS",
            "THANKSGIVING",
            "CHRISTMAS"
        };
    }
}
