using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.OrderReports;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ShipmentReportController : BaseController
    {
        public override string TAG
        {
            get { return "ShipmentReportController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");
            var model = new ShipmentReportSearchFilterViewModel()
            {
                FromDate = Time.GetAppNowTime().AddDays(-7)
            };
            return View(model);
        }

        public virtual ActionResult ExportToExcel(DateTime? fromDate,
            DateTime? toDate,
            string orderString)
        {
            LogI("ExportToExcel, fromDate=" + fromDate
                + ", toDate=" + toDate
                + ", orderString=" + orderString);

            var searchFilter = new ShipmentReportSearchFilterViewModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                OrderString = orderString,
                StartIndex = 0,
                LimitCount = 100000,
            };

            string filename = "ShipmentReport_" + Time.GetAppNowTime().ToString("ddMMyyyyHHmmss") + ".xls";
            var output = ShipmentReportViewModel.ExportToExcel(LogService,
                Time,
                Db,
                searchFilter,
                AccessManager.IsFulfilment);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        [Compress]
        public virtual ActionResult GetAll(GridRequest request,
            string orderString,
            DateTime? fromDate,
            DateTime? toDate)
        {
            LogI("GetAll, fromDate=" + fromDate
                + ", toDate=" + toDate
                + ", orderString=" + orderString);

            var pageSize = request.ItemsPerPage;

            var searchFilter = new ShipmentReportSearchFilterViewModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                OrderString = orderString,
                StartIndex = (request.Page - 1) * pageSize,
                LimitCount = pageSize,
                SortField = request.SortField,
                SortMode = request.SortMode == "asc" ? 0 : 1,
            };

            var gridResult = ShipmentReportViewModel.GetAll(Db, searchFilter);
            //var data = new GridResponse<StyleViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(gridResult, JsonRequestBehavior.AllowGet);
        }
    }
}
