using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.DTO;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Inventory;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class EmailStatisticReportController : BaseController
    {
        public override string TAG
        {
            get { return "EmailStatisticReportController."; }
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest] DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            LogI("GetAll, dateFrom=" + dateFrom + ", dateTo=" + dateTo);

            var items = EmailStatisticReportViewModel.GetAll(Db,
                Time,
                dateFrom,
                dateTo);

            var dataSource = items.ToDataSourceResult(request);
            return JsonGet(dataSource);
        }
    }
}