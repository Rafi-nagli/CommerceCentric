using System;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.ScanOrders;
using Amazon.Web.ViewModels.Vendors;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ScanOrderItemController : BaseController
    {
        public override string TAG
        {
            get { return "ScanOrderItemController."; }
        }

        private const string PopupContentView = "ScanOrderItemPopupContent";


        public virtual ActionResult Update(long scanItemId, long scanOrderId)
        {
            LogI("Update, scanItemId=" + scanItemId + ", scanOrderId=" + scanOrderId);

            SessionHelper.ClearUploadedImages();

            var model = ScanOrderItemViewModel.GetById(Db, scanItemId, scanOrderId);

            ViewBag.PartialViewName = PopupContentView;
            return View("EditNew", model);
        }
        
        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            long scanOrderId)
        {
            LogI("GetAll, scanOrderId=" + scanOrderId);

            //var searchFilter = new StyleSearchFilterViewModel()
            //{
            //    Barcode = barcode
            //};
            var items = ScanOrderItemViewModel.GetAll(Db, scanOrderId).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Submit(ScanOrderItemViewModel model)
        {
            LogI("Submit, model=" + model);

            long? id = null;
            //Save
            if (ModelState.IsValid)
            {
                var errors = model.Validate(Db);
                if (!errors.Any())
                {
                    id = model.Save(Db, 
                        DateHelper.GetAppNowTime(),
                        AccessManager.UserId);
                }
                else
                {
                    errors.Each(e => ModelState.AddModelError(e.Key, e.Message));
                    
                    return PartialView(PopupContentView, model);
                }

                //TODO: Add "Status", now only updates StatusCode
                return Json(new UpdateRowViewModel(model,
                    "ScanOrderItem_" + model.ScanOrderId, 
                    null,
                    false));
            } 
            return PartialView(PopupContentView, model);
        }
    }
}
