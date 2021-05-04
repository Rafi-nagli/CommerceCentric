using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL.Repositories;
using Amazon.DTO.Sizes;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.Counting;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class OpenBoxCountingController : BaseController
    {
        public override string TAG
        {
            get { return "OpenBoxCountingController."; }
        }

        public virtual ActionResult OnCreateItem(long styleId,
            bool? isMobileMode)
        {
            LogI("OnCreateItem, styleId=" + styleId + ", isMobileMode=" + isMobileMode);

            var model = new OpenBoxCountingViewModel(Db, styleId);
            model.CountingDate = Time.GetAppNowTime();
            model.CanEditCountPerson = !(isMobileMode ?? false);
            if (isMobileMode == true)
                model.CountByName = AccessManager.UserName;

            ViewBag.PartialViewName = "OpenBoxViewModel";
            return View("EditNew", model);
        }

        public virtual ActionResult DeleteBox(int id)
        {
            LogI("DeleteBox, id=" + id);

            var record = Db.OpenBoxCountings.Get(id);
            Db.OpenBoxCountings.Remove(record);
            Db.Commit();

            var result = StyleLiteCountingViewModel.GetAll(Db, new StyleSearchFilterViewModel()
            {
                StyleId = record.StyleId
            }).FirstOrDefault();

            return Json(new UpdateRowViewModel(result,
                    "Styles",
                    new[]
                    {
                            "StyleItems"
                    },
                    false));
        }

        public virtual ActionResult OnUpdateItem(long openBoxId, bool? isMobileMode)
        {
            LogI("OnUpdateItem, openBoxId=" + openBoxId + ", isMobileMode=" + isMobileMode);

            var item = Db.OpenBoxCountings.Get(openBoxId);
            
            var styleItems = Db.StyleItems.GetByStyleIdWithSizeGroupAsDto(item.StyleId).ToList();
            var boxSizeItems = Db.OpenBoxCountingItems.GetByBoxIdAsDto(openBoxId).ToList();

            var model = new OpenBoxCountingViewModel(item, styleItems, boxSizeItems);
            model.CanEditCountPerson = !(isMobileMode ?? false);

            ViewBag.PartialViewName = "OpenBoxViewModel";
            return View("EditNew", model);
        }

        
        [HttpPost]
        public virtual ActionResult Submit(OpenBoxCountingViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var id = model.Apply(Db, Time.GetAppNowTime(), AccessManager.UserId);
                
                return Json(new UpdateRowViewModel(model, "OpenBox_" + id, null, true));
            }
            return View("OpenBoxViewModel", model);
        }

        public virtual ActionResult GetOpenBox([DataSourceRequest]DataSourceRequest request, long styleId)
        {
            LogI("GetOpenBox, styleId=" + styleId);

            var items = OpenBoxCountingViewModel.GetAll(Db, styleId).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
