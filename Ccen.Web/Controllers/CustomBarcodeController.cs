using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class CustomBarcodeController : BaseController
    {
        public override string TAG
        {
            get { return "CustomBarcodeController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            string barcode,
            string sku)
        {
            LogI("GetAll, barcode=" + barcode + ", sku=" + sku);

            var filter = new CustomBarcodeFilterViewModel()
            {
                Barcode = barcode,
                SKU = sku
            };
            var items = CustomBarcodeViewModel.GetAll(Db, filter);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        
        public virtual ActionResult RemoveAttachedSKU(long id)
        {
            LogI("RemoveAttachedSKU, id=" + id);

            BarcodeService.RemoveAttachedSKU(id);
            
            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult OnRequestBarcode()
        {
            LogI("OnRequestBarcode");

            return View("RequestBarcodePopupContent");
        }

        public virtual ActionResult RequestBarcode(string skuText)
        {
            LogI("RequestBarcode, skuText=" + skuText);

            var results = CustomBarcodeViewModel.AssociateBarcodes(BarcodeService,
                skuText,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return Json(new ValueResult<IList<CustomBarcodeViewModel>>(true, "", results), JsonRequestBehavior.AllowGet);
        }
    }
}
