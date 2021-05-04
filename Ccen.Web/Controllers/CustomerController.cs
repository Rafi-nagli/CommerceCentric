using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.Mvc;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Customers;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllClients)]
    public partial class CustomerController : BaseController
    {
        public override string TAG
        {
            get { return "CustomerController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View(CustomerFilterViewModel.Empty);
        }


        public virtual ActionResult ExportToExcel()
        {
            LogI("ExportToExcel");

            var filters = new CustomerFilterViewModel();

            string filename = "CustomerReport_" + Time.GetAppNowTime().ToString("ddMMyyyyHHmmss") + ".xls";
            var output = CustomerViewModel.ExportToExcel(LogService,
                Time,
                Db,
                filters);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult GetAll([DataSourceRequest] DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo,
            string buyerName,
            string orderNumber)
        {
            LogI("GetAll");

            var filter = new CustomerFilterViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                BuyerName = buyerName,
                OrderNumber = orderNumber
            };

            request.Sorts = new List<SortDescriptor>()
            {
                new SortDescriptor("CreateDate", ListSortDirection.Descending)
            };
            var items = CustomerViewModel.GetAll(Db, filter);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
