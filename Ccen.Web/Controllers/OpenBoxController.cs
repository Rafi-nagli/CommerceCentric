using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL.Repositories;
using Amazon.DTO.Sizes;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class OpenBoxController : BaseController
    {
        public override string TAG
        {
            get { return "OpenBoxController."; }
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult OnCreateItem(long styleId)
        {
            LogI("OnCreateItem, styleId=" + styleId);

            var model = OpenBoxViewModel.BuildFromStyleId(Db, styleId);

            ViewBag.PartialViewName = "OpenBoxViewModel";
            return View("EditNew", model);
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult DeleteBox(int id)
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

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult OnUpdateItem(long openBoxId)
        {
            LogI("OnUpdateItem, openBoxId=" + openBoxId);

            var model = OpenBoxViewModel.BuildFromBoxId(Db, openBoxId);

            ViewBag.PartialViewName = "OpenBoxViewModel";
            return View("EditNew", model);
        }

        public virtual ActionResult Validate(OpenBoxViewModel model)
        {
            LogI("Validate");
            var results = model.Validate(Db);

            return JsonGet(results);
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        [HttpPost]
        public virtual ActionResult Submit(OpenBoxViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var id = model.Apply(Db, QuantityManager, Time.GetAppNowTime(), AccessManager.UserId);

                Cache.RequestStyleIdUpdates(Db, 
                    new List<long>() { model.StyleId },
                    UpdateCacheMode.IncludeChild,
                    AccessManager.UserId);

                return Json(new UpdateRowViewModel(model, "OpenBox_" + id, null, true));
            }
            return View("OpenBoxViewModel", model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult GetOpenBox([DataSourceRequest]DataSourceRequest request, long styleId)
        {
            LogI("GetOpenBox, styleId=" + styleId);

            var items = OpenBoxViewModel.GetAll(Db, 
                styleId,
                includeArchive: AccessManager.IsAdmin).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
