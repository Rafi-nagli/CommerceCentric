using System;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Vendors;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class VendorOrderItemController : BaseController
    {
        public override string TAG
        {
            get { return "VendorOrderItemController."; }
        }

        private const string PopupContentView = "VendorOrderItemPopupContent";


        public virtual ActionResult Update(long id)
        {
            LogI("Updater, Id=" + id);

            SessionHelper.ClearUploadedImages();

            var model = VendorOrderItemViewModel.GetById(Db, id);

            ViewBag.PartialViewName = PopupContentView;
            return View("Edit", model);
        }

        public virtual ActionResult Delete(long id)
        {
            LogI("Delete, Id=" + id);

            VendorOrderItemViewModel.Delete(Db, id);
            
            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Create(long parentId)
        {
            LogI("Create");

            SessionHelper.ClearUploadedImages();

            var model = VendorOrderItemViewModel.Create(Db);
            model.VendorOrderId = parentId;

            ViewBag.PartialViewName = PopupContentView;
            ViewBag.IsAdd = true;
            return View("Edit", model);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            long vendorOrderId)
        {
            LogI("GetAll, vendorOrderId=" + vendorOrderId);

            //var searchFilter = new StyleSearchFilterViewModel()
            //{
            //    Barcode = barcode
            //};
            var items = VendorOrderItemViewModel.GetAll(Db, vendorOrderId).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Submit(VendorOrderItemViewModel model)
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
                        SessionHelper.GetUploadedImages(),
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
                    "VendorOrderItem_" + model.VendorOrderId, 
                    null,
                    true));
            } 
            return PartialView(PopupContentView, model);
        }
    }
}
