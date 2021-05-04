using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.History;
using Amazon.Web.ViewModels.Reports;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class HistoryController : BaseController
    {
        public virtual ActionResult SyncHistory()
        {
            LogI("SyncHistory");

            return View();
        }

        public virtual ActionResult GetSyncHistory([DataSourceRequest] DataSourceRequest request)
        {
            LogI("GetSyncHistory");

            var items = SyncHistoryViewModel.GetAll(Db);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetSyncMessages([DataSourceRequest] DataSourceRequest request, long syncHistoryId)
        {
            LogI("GetSyncMessages, syncHistoryId=" + syncHistoryId);

            var items = SyncMessageViewModel.GetAll(Db, syncHistoryId);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource };
        }
    }
}