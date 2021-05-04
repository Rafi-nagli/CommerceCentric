using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.Model.Implementation;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.PurchaseOrders;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllClients)]
    public partial class PurchaseOrderController : BaseController
    {
        public override string TAG
        {
            get { return "PurchaseOrderController."; }
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult OnCreateItem()
        {
            LogI("OnCreateItem");

            var model = new PurchaseOrderViewModel();

            ViewBag.PartialViewName = "_EditPurchaseOrder";
            return View("EditNew", model);
        }

        public virtual ActionResult Delete(int id)
        {
            LogI("DeleteBox, id=" + id);

            OpenBoxViewModel.Remove(Db,
                id,
                QuantityManager,
                Cache,
                Time.GetAppNowTime(),
                AccessManager.UserId);


            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult OnUpdateItem(long openBoxId)
        {
            LogI("OnUpdateItem, openBoxId=" + openBoxId);

            var model = OpenBoxViewModel.BuildFromBoxId(Db, openBoxId);

            ViewBag.PartialViewName = "OpenBoxViewModel";
            return View("EditNew", model);
        }

        //public virtual ActionResult Validate(PurchaseOrderViewModel model)
        //{
        //    LogI("Validate");
        //    var results = model.Validate(Db);

        //    return JsonGet(results);
        //}

        [HttpPost]
        public virtual ActionResult Submit(PurchaseOrderViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var box = model.ToBox(Db, Time);

                var id = box.Apply(Db, QuantityManager, Time.GetAppNowTime(), AccessManager.UserId);

                Cache.RequestStyleIdUpdates(Db,
                    new List<long>() { box.StyleId },
                    UpdateCacheMode.IncludeChild,
                    AccessManager.UserId);

                return Json(new UpdateRowViewModel(model, "OpenBox_" + id, null, true));
            }
            return JsonGet(model);
        }

        public virtual ActionResult GetPurchaseOrder(long id)
        {
            LogI("GetPurchaseOrder, id=" + id);

            var model = OpenBoxViewModel.BuildFromBoxId(Db, id);
            var purchaseModel = new PurchaseOrderViewModel(model);

            return JsonGet(new ValueResult<PurchaseOrderViewModel>(true, null, purchaseModel));
        }


        public virtual ActionResult GetAll([DataSourceRequest] DataSourceRequest request)
        {
            LogI("GetOpenBox");

            var items = OpenBoxViewModel.GetAll(Db,
                null,
                includeArchive: true).ToList();

            var data = new GridResponse<OpenBoxViewModel>(items, items.Count, Time.GetAppNowTime());
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
