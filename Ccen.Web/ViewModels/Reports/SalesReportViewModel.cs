using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.DTO.Reports;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.ViewModels.Reports
{
    public class SalesReportViewModel
    {
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public string FeatureValue { get; set; }

        public int NumberOfSoldUnits { get; set; }
        public decimal? AveragePrice { get; set; }
        public decimal? MinCurrentPrice { get; set; }
        public decimal? MaxCurrentPrice { get; set; }
        public decimal RemainingUnits { get; set; }


        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString);  }
        }

        #region Search Filters

        public static SelectList PeriodList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Day", ((int)FilterPeriod.Day).ToString()),
                    new KeyValuePair<string, string>("Week", ((int)FilterPeriod.Week).ToString()),
                    new KeyValuePair<string, string>("Month", ((int)FilterPeriod.Month).ToString()),
                    new KeyValuePair<string, string>("3 Month", ((int)FilterPeriod.ThreeMonth).ToString()),
                }, "Value", "Key", ((int)FilterPeriod.Week).ToString());
            }
        }


        public enum FilterPeriod
        {
            Day = 0,
            Week = 1,
            Month = 2,
            ThreeMonth = 3,
        }

        #endregion

        public enum SalesReportType
        {
            ByStyle = 0,
            ByLicense = 1,
            BySleeve = 2,
            ByGender = 3,
        }

        private static DateTime GetFromPeriod(ITime time, FilterPeriod period)
        {
            DateTime fromDate = DateTime.Today;
            var today = time.GetAppNowTime().Date;
            switch (period)
            {
                case FilterPeriod.Day: //Day
                    fromDate = today;
                    break;
                case FilterPeriod.Week: //Week
                    fromDate = today.AddDays(-7);
                    break;
                case FilterPeriod.Month: //Month
                    fromDate = today.AddMonths(-1);
                    break;
                case FilterPeriod.ThreeMonth: //3 month
                    fromDate = today.AddMonths(-3);
                    break;
            }
            return fromDate;
        }

        public static IList<SalesReportViewModel> GetAllByDate(IUnitOfWork db,
            ITime time,
            FilterPeriod period)
        {
            DateTime fromDate = GetFromPeriod(time, period);

            var items = db.SalesReports.GetSalesByDateReport(fromDate);
            
            return items.Select(i => new SalesReportViewModel(i)).ToList();
        }

        public static IList<SalesReportViewModel> GetAllByFeature(IUnitOfWork db,
            ITime time,
            FilterPeriod period,
            int featureId)
        {
            DateTime fromDate = GetFromPeriod(time, period);

            var items = db.SalesReports.GetSalesByFeatureReport(fromDate, featureId);

            return items.Select(i => new SalesReportViewModel(i)).ToList();
        }


        public SalesReportViewModel()
        {
            
        }

        public SalesReportViewModel(SalesReportDTO model)
        {
            StyleId = model.StyleId;
            StyleString = model.StyleString;
            FeatureValue = model.FeatureValue;

            NumberOfSoldUnits = model.NumberOfSoldUnits;
            AveragePrice = model.AveragePrice;
            MinCurrentPrice = model.MinCurrentPrice;
            MaxCurrentPrice = model.MaxCurrentPrice;
            RemainingUnits = model.RemainingUnits ?? 0;
        }
    }
}