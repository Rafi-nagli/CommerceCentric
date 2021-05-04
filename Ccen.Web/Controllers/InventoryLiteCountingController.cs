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
    public partial class InventoryLiteCountingController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryLiteCountingController."; }
        }

        private const string PopupContentView = "StyleCountingViewModel";

        public virtual ActionResult Styles()
        {
            LogI("Styles, HTTP_USER_AGENT=" + Request.ServerVariables["HTTP_USER_AGENT"]);
            LogI("IsMobileDeviceCustom=" + MobileHelper.IsMobileDeviceCustom() + ", IsMobileDevice=" + Request.Browser.IsMobileDevice);

            return View("Styles");
        }

        public virtual ActionResult Approve()
        {
            LogI("Styles, HTTP_USER_AGENT=" + Request.ServerVariables["HTTP_USER_AGENT"]);
            LogI("IsMobileDeviceCustom=" + MobileHelper.IsMobileDeviceCustom() + ", IsMobileDevice=" + Request.Browser.IsMobileDevice);

            return View("Approve");
        }

        //public virtual ActionResult EditStyleLocation(long styleId)
        //{
        //    LogI("EditStyleLocation, id=" + styleId);

        //    var model = new StyleLocationViewModel(Db, styleId, Time.GetAppNowTime());
        //    ViewBag.PartialViewName = PopupContentView;
        //    return View("EditEmpty", model);
        //}

        //[HttpPost]
        //public virtual ActionResult SaveCountingStatus(StyleLiteCountingViewModel model)
        //{
        //    LogI("Submit, model=" + model.Id + ", style=" + model.StyleId + ", status=" + model.CountingStatus + ", by=" + model.CountingName);

        //    model.SaveStatus(Db,
        //        Time.GetAppNowTime(),
        //        AccessManager.UserId);

        //    return Json(true);
        //}


        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult Edit(long id)
        {
            LogI("UpdateStyleCountingInfo, Id=" + id);

            var item = Db.Styles.Get(id);

            var model = new StyleLiteCountingViewModel(Db, item);
                
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult SetApproveStatus(long styleId, long styleItemId)
        {
            LogI("SetApproveStatus, styleItemId=" + styleItemId);

            StyleLiteCountingViewModel.SetApproveStatus(Db,
                styleItemId, 
                (int)ApproveStatuses.Approved,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            var result = StyleLiteCountingViewModel.GetAll(Db, new StyleSearchFilterViewModel()
            {
                StyleId = styleId
            }).FirstOrDefault();

            return Json(new UpdateRowViewModel(result,
                    "Styles",
                    new[]
                    {
                            "StyleItems"
                    },
                    false));
        }


        [Authorize(Roles = AccessManager.AllBaseRole)]
        [HttpPost]
        public virtual ActionResult Submit(StyleLiteCountingViewModel model)
        {

            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                model.UpdateStyleItems(Db, Time.GetAppNowTime(), AccessManager.UserName);

                var result = StyleLiteCountingViewModel.GetAll(Db, new StyleSearchFilterViewModel()
                {
                    StyleId = model.Id
                }).FirstOrDefault();

                return Json(new UpdateRowViewModel(result,
                        "Styles",
                        new[]
                        {
                            "StyleItems",
                        },
                        false));
            }
            ViewBag.IsAdd = false;
            return PartialView(PopupContentView, model);
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
            var items = StyleLiteCountingViewModel.GetAll(Db, searchFilter).ToList();
            var data = new GridResponse<StyleLiteCountingViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
