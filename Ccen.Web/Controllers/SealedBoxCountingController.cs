using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Core.Models.SystemActions;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.Counting;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class SealedBoxCountingController : BaseController
    {
        public override string TAG
        {
            get { return "SealedBoxCountingController."; }
        }

        public virtual ActionResult OnCreateItem(long styleId, bool? isMobileMode)
        {
            LogI("OnCreateItem, id=" + styleId + ", isMobileMode=" + isMobileMode);

            var model = new SealedBoxCountingViewModel(Db, styleId);
            model.CountingDate = Time.GetAppNowTime();
            model.CanEditCountPerson = !(isMobileMode ?? false);
            if (isMobileMode == true)
                model.CountByName = AccessManager.UserName;

            ViewBag.PartialViewName = "SealedBoxViewModel";
            return View("EditNew", model);
        }

        public virtual ActionResult DeleteBox(int id)
        {
            LogI("DeleteBox, Id=" + id);

            var record = Db.SealedBoxCountings.Get(id);
            Db.SealedBoxCountings.Remove(record);
            Db.Commit();
            
            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public virtual ActionResult Submit(SealedBoxCountingViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var id = model.Apply(Db, Time.GetAppNowTime(), AccessManager.UserId);

                return Json(new UpdateRowViewModel(model, "SealedBox_" + id, null, true));
            }
            return View("SealedBoxViewModel", model);
        }

        public virtual ActionResult GetSealedBox([DataSourceRequest]DataSourceRequest request, long styleId)
        {
            LogI("GetSealedBox, styleId=" + styleId);

            var items = SealedBoxCountingViewModel.GetAll(Db, styleId).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult OnUpdateItem(long sealedBoxId, bool? isMobileMode)
        {
            LogI("OnUpdateItem, sealedBoxId=" + sealedBoxId);

            var item = Db.SealedBoxCountings.Get(sealedBoxId);
            var styleItems = Db.StyleItems.GetByStyleIdWithSizeGroupAsDto(item.StyleId).ToList();
            var boxSizeItems = Db.SealedBoxCountingItems.GetByBoxIdAsDto(sealedBoxId).ToList();

            var model = new SealedBoxCountingViewModel(item, styleItems, boxSizeItems);
            model.CanEditCountPerson = !(isMobileMode ?? false);
            
            ViewBag.PartialViewName = "SealedBoxViewModel";
            return View("EditNew", model);
        }
    }
}
