using System;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Vendors;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class VendorOrderController : BaseController
    {
        public override string TAG
        {
            get { return "VendorOrderController."; }
        }

        private const string PopupContentView = "VendorOrderPopupContent";

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View();
        }

        public virtual ActionResult Update(long id)
        {
            LogI("Update, Id=" + id);

            var model = VendorOrderViewModel.GetById(Db, id);

            ViewBag.PartialViewName = PopupContentView;
            return View("EditNew", model);
        }

        public virtual ActionResult Create()
        {
            LogI("Create");

            var model = VendorOrderViewModel.Create(Db);
            ViewBag.PartialViewName = PopupContentView;
            ViewBag.IsAdd = true;
            return View("EditNew", model);
        }

        public virtual ActionResult Delete(long id)
        {
            LogI("Delete, Id=" + id);

            VendorOrderViewModel.Delete(Db, id);

            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Export(long id)
        {
            LogI("Export, id=" + id);

            string filename;
            var output = VendorExport.ExportToExcelVendorOrder(Db, 
                Time, 
                id, 
                out filename);

            return File(output.ToArray(),
                "application/vnd.ms-excel",
                filename);
        }


        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAll");

            //var searchFilter = new StyleSearchFilterViewModel()
            //{
            //    Barcode = barcode
            //};
            var items = VendorOrderViewModel.GetAll(Db).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Submit(VendorOrderViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var errors = model.Validate(Db);
                if (!errors.Any())
                {
                    model.Save(Db, 
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
                    "VendorOrders", 
                    null,
                    true));
            } 
            return PartialView(PopupContentView, model);
        }
    }
}
