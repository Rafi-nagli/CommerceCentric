using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.Boxes;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class SealedBoxController : BaseController
    {
        public override string TAG
        {
            get { return "SealedBoxController."; }
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult OnCreateItem(long styleId)
        {
            LogI("OnCreateItem, id=" + styleId);

            var model = SealedBoxViewModel.BuildFromStyleId(Db, styleId);

            ViewBag.PartialViewName = "SealedBoxViewModel";
            return View("EditNew", model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult OnCreateBoxWizard(long styleId)
        {
            LogI("OnCreateBoxWizard, id=" + styleId);

            var model = AddBoxWizardViewModel.BuildFromStyleId(Db, styleId);

            ViewBag.PartialViewName = "AddBoxesWizard";
            return View("EditEmpty", model);
        }

        public virtual ActionResult ValidateWizard(AddBoxWizardViewModel model)
        {
            LogI("ValidateWizard, model=" + model);
            var messages = model.Validate(Db, LogService, Time.GetAppNowTime());
            foreach (var message in messages)
            {
                LogI("ValidateWizard, message=" + message.Message);
            }
            return JsonGet(new ValueResult<IList<MessageString>>(true, "", messages));
        }

        public virtual ActionResult SubmitWizard(AddBoxWizardViewModel model)
        {
            LogI("SubmitWizard, model=" + model);

            model.Apply(Db, 
                QuantityManager,
                LogService,
                Cache,
                ActionService,
                Time.GetAppNowTime(), 
                AccessManager.UserId);

            return JsonGet(new MessageResult(true, "", ""));
        }


        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult DeleteBox(int id)
        {
            LogI("DeleteBox, Id=" + id);

            SealedBoxViewModel.Remove(Db, 
                id, 
                QuantityManager,
                Cache,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult CopyBox(int id)
        {
            LogI("CopyBox, Id=" + id);

            var model = SealedBoxViewModel.CopyBox(Db, 
                id, 
                Time.GetAppNowTime(),
                AccessManager.UserId);

            Cache.RequestStyleIdUpdates(Db,
                new List<long>() { model.StyleId },
                UpdateCacheMode.IncludeChild,
                AccessManager.UserId); 

            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        [HttpPost]
        public virtual ActionResult Submit(SealedBoxViewModel model)
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

                return Json(new UpdateRowViewModel(model, "SealedBox_" + id, null, true));
            }
            return View("SealedBoxViewModel", model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult GetSealedBox([DataSourceRequest]DataSourceRequest request, long styleId)
        {
            LogI("GetSealedBox, styleId=" + styleId);

            var items = SealedBoxViewModel.GetAll(Db, 
                styleId,
                includeArchive: AccessManager.IsAdmin).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult OnUpdateItem(long sealedBoxId)
        {
            LogI("OnUpdateItem, sealedBoxId=" + sealedBoxId);

            var model = SealedBoxViewModel.BuildFromBoxId(Db, sealedBoxId);
            ViewBag.PartialViewName = "SealedBoxViewModel";
            return View("EditNew", model);
        }
    }
}
