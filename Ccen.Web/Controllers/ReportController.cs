using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Features;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Reports;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class ReportController : BaseController
    {
        public override string TAG
        {
            get { return "ReportController."; }
        }

        //
        // GET: /Reports/

        public virtual ActionResult SalesByDate()
        {
            LogI("SalesByDate");

            return View("SalesReport", SalesReportViewModel.SalesReportType.ByStyle);
        }

        public virtual ActionResult SalesByLicense()
        {
            LogI("SalesByLicense");

            return View("SalesReport", SalesReportViewModel.SalesReportType.ByLicense);
        }

        public virtual ActionResult SalesBySleeve()
        {
            LogI("SalesBySleeve");

            return View("SalesReport", SalesReportViewModel.SalesReportType.BySleeve);
        }

        public virtual ActionResult SalesByGender()
        {
            LogI("SalesByGender");

            return View("SalesReport", SalesReportViewModel.SalesReportType.ByGender);
        }

        public virtual ActionResult GetAll([DataSourceRequest] DataSourceRequest request, int reportType, int period)
        {
            IList<SalesReportViewModel> items = new List<SalesReportViewModel>();
            switch ((SalesReportViewModel.SalesReportType) reportType)
            {
                case SalesReportViewModel.SalesReportType.ByStyle:
                    items = SalesReportViewModel.GetAllByDate(Db, Time, (SalesReportViewModel.FilterPeriod)period);
                    break;
                case SalesReportViewModel.SalesReportType.ByLicense:
                    items = SalesReportViewModel.GetAllByFeature(Db, Time, (SalesReportViewModel.FilterPeriod)period, StyleFeatureHelper.SUB_LICENSE1);
                    break;
                case SalesReportViewModel.SalesReportType.BySleeve:
                    items = SalesReportViewModel.GetAllByFeature(Db, Time, (SalesReportViewModel.FilterPeriod)period, StyleFeatureHelper.SLEEVE);
                    break;
                case SalesReportViewModel.SalesReportType.ByGender:
                    items = SalesReportViewModel.GetAllByFeature(Db, Time, (SalesReportViewModel.FilterPeriod)period, StyleFeatureHelper.GENDER);
                    break;
            }
            
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
