using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
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

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryDescriptionController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryDescriptionController."; }
        }

        private const string PopupContentView = "DescriptionPopupContent";

        public virtual ActionResult Styles()
        {
            LogI("Styles");

            var model = new StylePageViewModel();
            model.Init(Db);

            return View("Styles", model);
        }

        public virtual ActionResult EditStyleDescription(long styleId)
        {
            LogI("EditStyleDescription, id=" + styleId);

            var model = new StyleDescriptionViewModel(Db, styleId);
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [HttpPost]
        public virtual ActionResult SetDescription(long fromStyleId, long toStyleId)
        {
            LogI("SetDescription, toStyleId=" + toStyleId + ", fromStyleId=" + fromStyleId);
            var model = new StyleDescriptionViewModel(Db, fromStyleId); //NOTE: get all fields from Db include LongDescriptions
            model.Id = toStyleId;

            model.Apply(Db,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);

            var outputModel = StyleDescriptionViewModel.GetAll(Db, new StyleSearchFilterViewModel()
            {
                StyleId = model.Id
            }).FirstOrDefault();

            return Json(CallResult<StyleDescriptionViewModel>.Success(outputModel));
        }

        [HttpPost]
        public virtual ActionResult Submit(StyleDescriptionViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                model.Apply(Db,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);

                var outputModel = StyleDescriptionViewModel.GetAll(Db, new StyleSearchFilterViewModel()
                {
                    StyleId = model.Id
                }).FirstOrDefault();

                return Json(new UpdateRowViewModel(outputModel,
                    "Styles",
                    new[]
                    {
                        "Description",
                        "ShortDescription",
                        "SearchTerms",
                        "BulletPoint1",
                        "BulletPoint2",
                        "BulletPoint3",
                        "BulletPoint4",
                        "BulletPoint5",
                    },
                    false));
            }
            return View(PopupContentView, model);
        }

        public virtual ActionResult GetAllUpdates(DateTime? fromDate)
        {
            LogI("GetAllUpdates, fromDate=" + fromDate);

            var searchFilter = new StyleSearchFilterViewModel()
            {
                FromReSaveDate = fromDate
            };
            var items = StyleDescriptionViewModel.GetAll(Db, searchFilter).ToList();
            var data = new GridResponse<StyleDescriptionViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [Compress]
        public virtual ActionResult GetAll(GridRequest request)
        {
            LogI("GetAll");

            var searchFilter = new StyleSearchFilterViewModel();
            var items = StyleDescriptionViewModel.GetAll(Db, searchFilter).ToList();
            var data = new GridResponse<StyleDescriptionViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
