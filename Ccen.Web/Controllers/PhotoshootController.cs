using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.FBAPickLists;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class PhotoshootController : BaseController
    {
        public override string TAG
        {
            get { return "PhotoshootController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            bool showArchived)
        {
            LogI("GetAll");

            var filters = new PhotoshootPickListFilterViewModel()
            {
                ShowArchived = showArchived
            };
            var items = PhotoshootPickListViewModel.GetAll(Db, filters);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult SetArchiveStatus(long id, bool status)
        {
            LogI("SetArchiveStatus begin, fbaShipment=" + id);
            var newStatus = PhotoshootPickListViewModel.SetArchiveStatus(Db, id, status);

            return JsonGet(ValueResult<bool>.Success("", newStatus));
        }

        public virtual ActionResult OnEdit(long? id)
        {
            LogI("OnEdit, pickListId=" + id);

            var model = PhotoshootPickListViewModel.Get(Db, id);

            ViewBag.PartialViewName = "EditPickListPopupContent";
            return View("EditEmpty", model);
        }

        [HttpPost]
        public virtual ActionResult Submit(PhotoshootPickListViewModel model)
        {
            LogI("Submit, model=" + model);

            model.Apply(Db,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(new UpdateRowViewModel(model,
                    "grid",
                    null,
                    true));
        }
    }
}
