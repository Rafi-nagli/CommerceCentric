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
    public partial class InventoryCountingController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryCountingController."; }
        }

        private const string PopupContentView = "LocationPopupContent";

        public virtual ActionResult Styles()
        {
            LogI("Styles");

            return View("Styles");
        }

        public virtual ActionResult EditStyleLocation(long styleId)
        {
            LogI("EditStyleLocation, id=" + styleId);

            var model = new StyleLocationViewModel(Db, styleId, Time.GetAppNowTime());
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [HttpPost]
        public virtual ActionResult Submit(StyleLocationViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                model.Apply(Db,
                    StyleHistoryService,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);

                var outputModel = StyleCountingViewModel.GetAll(Db, new StyleSearchFilterViewModel()
                {
                    StyleId = model.StyleId
                }).FirstOrDefault();

                return Json(new UpdateRowViewModel(outputModel,
                    "Styles",
                    new[]
                    {
                        "Locations",
                        "MainLocation",
                        "HasLocation",
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
            var items = StyleCountingViewModel.GetAll(Db, searchFilter).ToList();
            var data = new GridResponse<StyleCountingViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [Compress]
        public virtual ActionResult GetAll(GridRequest request)
        {
            LogI("GetAll");

            var searchFilter = new StyleSearchFilterViewModel();
            var items = StyleCountingViewModel.GetAll(Db, searchFilter).ToList();
            var data = new GridResponse<StyleCountingViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
