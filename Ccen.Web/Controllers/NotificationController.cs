using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.ScanOrders;
using Amazon.Web.ViewModels.Vendors;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class NotificationController : BaseController
    {
        public override string TAG
        {
            get { return "NotificationController."; }
        }

        public virtual ActionResult Index(NotificationType? type)
        {
            LogI("Index");

            var model = new NotificationFilterViewModel();
            model.Type = (int?)type;
            return View(model);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            string orderNumber,
            bool? includeReaded,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? type,
            bool onlyPriority)
        {
            LogI("GetAll, orderNumber=" + orderNumber 
                + ", includeReaded=" + includeReaded
                + ", dateFrom=" + dateFrom
                + ", dateTo=" + dateTo
                + ", type=" + type
                + ", onlyPriority=" + onlyPriority);

            var searchFilter = new NotificationFilterViewModel()
            {
                OrderNumber = orderNumber,
                InlcudeReaded = includeReaded ?? false,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Type = type,
                OnlyPriority = onlyPriority
            };
            var items = NotificationViewModel.GetAll(Db, Time, searchFilter);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetInfo()
        {
            LogI("GetInfo");

            var info = NotificationInfoViewModel.GetInfo(Db, Settings, Time);
            return Json(info, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult MarkAsRead(MarkAsReadParams model)
        {
            LogI("MarkAsRead: " + model.ToString());

            NotificationViewModel.MarkAsRead(Db,
                LogService,
                model,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return Json(new MessageResult(true, null, null), JsonRequestBehavior.AllowGet);
        }
    }
}
