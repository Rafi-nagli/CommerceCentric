using System;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.ScanOrders;
using Amazon.Web.ViewModels.Vendors;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ScanOrderController : BaseController
    {
        public override string TAG
        {
            get { return "ScanOrderController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAll");

            //var searchFilter = new StyleSearchFilterViewModel()
            //{
            //    Barcode = barcode
            //};
            var items = ScanOrderViewModel.GetAll(Db).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }
    }
}
