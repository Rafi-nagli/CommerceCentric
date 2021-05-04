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
    public partial class MarketBarcodeController : BaseController
    {
        public override string TAG
        {
            get { return "MarketBarcodeController."; }
        }

        public virtual ActionResult OnRequestBarcode()
        {
            LogI("OnRequestBarcode");

            return View("RequestBarcodePopupContent");
        }

        public virtual ActionResult RequestBarcode(string productUrl)
        {
            LogI("RequestBarcode, productUrl=" + productUrl);

            var results = MarketBarcodeViewModel.SearchBarcodes(LogService,
                StringHelper.TrimWhitespace(productUrl));

            return Json(new ValueResult<IList<MarketBarcodeViewModel>>(true, "", results), JsonRequestBehavior.AllowGet);
        }
    }
}
