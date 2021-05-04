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
using Amazon.Web.ViewModels.Bargains;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class BargainController : BaseController
    {
        public override string TAG
        {
            get { return "CustomBarcodeController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Search()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult Export(BargainSearchFilterViewModel model)
        {
            LogI("Export, model=" + model);

            model.CategoryId = "5438"; //Apparel
            model.StartIndex = 1;
            model.LimitCount = 500;

            var result = BargainViewModel.Export(DbFactory, 
                LogService,
                Time,
                AccessManager.CompanyId.Value,
                model);

            return JsonGet(new ValueResult<string>(result.IsSuccess, result.Message, result.Data));
        }

        public virtual ActionResult GetBargainExportFile(string fileName)
        {
            LogI("GetBargainExportFile, fileName=" + fileName);

            var path = Models.UrlHelper.GetBargainExportFilePath(fileName);
            return File(path, "application/vnd.ms-excel", fileName);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            string keywords,
            decimal minPrice,
            decimal maxPrice)
        {
            LogI("GetAll, barcode=" + keywords + ", minPrice=" + minPrice + ", maxPrice=" + maxPrice);

            var filter = new BargainSearchFilterViewModel()
            {
                Keywords = keywords,
                MaxPrice = maxPrice,
                MinPrice = minPrice,
                CategoryId = "5438", //Apparel

                StartIndex = 1 + (request.Page - 1) * 25,
                LimitCount = 25,
            };
            var result = BargainViewModel.GetAll(DbFactory, LogService, Time, AccessManager.CompanyId.Value, filter);
            var dataSource = new DataSourceResult()
            {
                Data = result.Bargains,
                Total = result.TotalResults
            };
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
