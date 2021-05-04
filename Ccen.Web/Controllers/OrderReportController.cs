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
    public partial class OrderReportController : BaseController
    {
        public override string TAG
        {
            get { return "OrderReportController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");
            var model = new OrderReportSearchFilterViewModel()
            {
                OrderStatus = String.Join(";", OrderStatusEnumEx.AllUnshippedWithShipped)
            };
            return View(model);
        }

        public virtual ActionResult ExportToExcel(long? dropShipperId,
            string orderString,
            string orderStatus,
            int? market,
            DateTime? fromDate,
            DateTime? toDate)
        {
            LogI("ExportToExcel, fromDate=" + fromDate
                + ", toDate=" + toDate
                + ", orderString=" + orderString
                + ", dropShipperId=" + dropShipperId
                + ", orderStatus=" + orderStatus
                + ", market=" + market);

            var searchFilter = new OrderReportSearchFilterViewModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                DropShipperId = dropShipperId,
                Market = market,
                OrderString = orderString,
                OrderStatus = orderStatus,
                StartIndex = 0,
                LimitCount = 100000,
            };

            string filename = "OrderReport_" + Time.GetAppNowTime().ToString("ddMMyyyyHHmmss") + ".xls";
            var output = OrderReportViewModel.ExportToExcel(LogService,
                Time,
                Db,
                searchFilter);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        [Compress]
        public virtual ActionResult GetAll(GridRequest request,
            long? dropShipperId,
            string orderString,
            string orderStatus,
            int? market,
            DateTime? fromDate,
            DateTime? toDate)
        {
            LogI("GetAll, fromDate=" + fromDate
                + ", toDate=" + toDate
                + ", orderString=" + orderString
                + ", dropShipperId=" + dropShipperId
                + ", orderStatus=" + orderStatus
                + ", market=" + market);

            var pageSize = request.ItemsPerPage;

            var searchFilter = new OrderReportSearchFilterViewModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                DropShipperId = dropShipperId,
                Market = market,
                OrderString = orderString,
                OrderStatus = orderStatus,
                StartIndex = (request.Page - 1) * pageSize,
                LimitCount = pageSize,
                SortField = request.SortField,
                SortMode = request.SortMode == "asc" ? 0 : 1,
            };

            var gridResult = OrderReportViewModel.GetAll(Db, searchFilter);
            //var data = new GridResponse<StyleViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(gridResult, JsonRequestBehavior.AllowGet);
        }
    }
}
