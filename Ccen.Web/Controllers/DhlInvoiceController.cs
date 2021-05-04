using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.DTO.Shippings;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Orders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.IO;
using Amazon.Core.Models.Settings;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class DhlInvoiceController : BaseController
    {
        public override string TAG
        {
            get { return "DhlInvoiceController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            var model = new DhlInvoiceFilterViewModel();
            return View(model);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            string orderNumber,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? status)
        {
            LogI("GetAll, orderNumber=" + orderNumber
                + ", status=" + status
                + ", dateFrom=" + dateFrom
                + ", dateTo=" + dateTo);

            var searchFilter = new DhlInvoiceFilterViewModel()
            {
                OrderNumber = orderNumber,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
            };
            var items = DhlInvoiceViewModel.GetAll(Db, Time, searchFilter);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult SetStatus(int id, int newStatus)
        {
            LogI("SetStatus: id=" + id + ", newStatus=" + newStatus);
            DhlInvoiceViewModel.SetStatus(Db, id, newStatus);

            return JsonGet(MessageResult.Success());
        }

        [HttpPost]
        public virtual ActionResult UploadInvoice(HttpPostedFileBase invoiceFile)
        {
            LogI("UploadInvoice");
            try
            {
                if (invoiceFile != null)
                {
                    LogI("fileLength=" + invoiceFile.ContentLength + ", fileName=" + invoiceFile.FileName);
                    if (invoiceFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(invoiceFile.FileName);
                        var folderPath = Models.UrlHelper.GetUploadDhlInvoicePath();
                        var filePath = Path.Combine(folderPath, fileName);
                        invoiceFile.SaveAs(filePath);

                        var dhlService = new DhlInvoiceService(LogService, Time, DbFactory);
                        var records = dhlService.GetRecordsFromFile(filePath);
                        dhlService.ProcessRecords(records, ShipmentProviderType.Dhl);

                        return Json(MessageResult.Success("Invoice file has been processed"));
                    }
                }

                return Json(MessageResult.Error("Empty file"));
            }
            catch (Exception ex)
            {
                LogE("UploadInvoice, file=" + invoiceFile.FileName, ex);
                return Json(MessageResult.Error(ex.Message));
            }
        }

        public virtual ActionResult ExportToExcel(string orderNumber,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? status)
        {
            LogI("ExportToExcel, orderNumber=" + orderNumber
                + ", status=" + status
                + ", dateFrom=" + dateFrom
                + ", dateTo=" + dateTo);

            var searchFilter = new DhlInvoiceFilterViewModel()
            {
                OrderNumber = orderNumber,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
            };
            var invoices = DhlInvoiceViewModel.GetAll(Db, Time, searchFilter).ToList();

            return File(DhlInvoiceViewModel.BuildReport(invoices).ToArray(),   //The binary data of the XLS file
                            "application/vnd.ms-excel", //MIME type of Excel files
                            "DhlInvoiceReport.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

    }
}
