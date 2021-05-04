using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.FBAPickLists;
using Amazon.Web.ViewModels.Messages;
using Ccen.Core.Models.Enums;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ShipmentController : BaseController
    {
        public override string TAG
        {
            get { return "ShipmentController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index(string shipmenttype = "FBA")
        {
            LogI("Index");
            ViewBag.Shipmenttype = shipmenttype;
            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest] DataSourceRequest request,
            bool showArchived, string shipmenttype)
        {
            LogI("GetAll");

            var filters = new ShipmentPickListFilterViewModel()
            {
                ShowArchived = showArchived,
                type = shipmenttype
            };

            var items = ShipmentPickListViewModel.GetAll(Db, filters);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult SetArchiveStatus(long id, bool status)
        {
            LogI("SetArchiveStatus begin, fbaShipment=" + id);
            var newStatus = ShipmentPickListViewModel.SetArchiveStatus(Db, id, status);

            return JsonGet(ValueResult<bool>.Success("", newStatus));
        }

        public virtual ActionResult SetFinishStatus(long id, bool isFinished)
        {
            LogI("Finish, id=" + id);

            ShipmentPickListViewModel.SetFinishStatus(DbFactory, ActionService, id, isFinished, Time.GetAmazonNowTime(), AccessManager.UserId);

            return JsonGet(ValueResult<bool>.Success());
        }

        public virtual ActionResult OnEdit(long? id, string shipmenttype)
        {
            LogI("OnEdit, pickListId=" + id);

            var model = ShipmentPickListViewModel.Get(Db, id, shipmenttype);
            ViewBag.PartialViewName = "EditPickListPopupContent";
            return View("EditEmpty", model);
        }

        [HttpPost]
        public virtual ActionResult Submit(ShipmentPickListViewModel model)
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

        public virtual ActionResult ExportToExcel(long id)
        {
            LogI("ExportToExcel, id=" + id);

            string filename = null;
            var output = ShipmentPickListViewModel.ExportToExcel(id,
                Db,
                DbFactory,
                AccessManager.Company,
                AmazonCategoryService,
                HtmlScraper,
                LogService,
                Time,
                out filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportToWFSExcel(long id)
        {
            LogI("ExportToWFSExcel, id=" + id);

            var output = ShipmentPickListViewModel.ExportToWFSExcel(id,
                Db,
                DbFactory,
                AccessManager.Company,
                AmazonCategoryService,
                HtmlScraper,
                LogService,
                Time,
                out string filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportToPlanExcel(long id)
        {
            LogI("ExportToPlanExcel, id=" + id);

            string filename = null;
            var output = ShipmentPickListViewModel.ExportToPlanExcel(id,
                Db,
                DbFactory,
                AccessManager.Company,
                LogService,
                Time,
                out filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportToWFSPlanExcel(long id)
        {
            LogI("ExportToWFSPlanExcel, id=" + id);

            var output = ShipmentPickListViewModel.ExportToWFSPlanExcel(id,
                Db,
                DbFactory,
                AccessManager.Company,
                LogService,
                Time,
                out string filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportPickListExcel(long id)
        {
            LogI("ExportPickListExcel, id=" + id);

            var output = ShipmentPickListViewModel.ExportPickList(id,
                Db,
                Time,
                out string filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult GetListingByStyleSize(long styleItemId, long? selectedListingId, ShipmentsTypeEnum shipmenttype)
        {
            LogI("GetListingByStyleSize, styleItemId=" + styleItemId);

            var list = ShipmentPickListViewModel.GetListingByStyleSize(Db, styleItemId, selectedListingId, shipmenttype);

            return JsonGet(ValueResult<IList<SelectListItemTag>>.Success("", list));
        }
    }
}
