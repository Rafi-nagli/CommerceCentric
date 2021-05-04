using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.DropShippers;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.CustomFeeds;
using Amazon.Web.ViewModels.CustomReports;
using Amazon.Web.ViewModels.Messages;
using Ccen.Common.Helpers;
using Ccen.Model.Implementation;
using Ccen.Web.ViewModels.CustomReports;
using Ccen.Web.ViewModels.CustomReportView;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllRoles)]
    public partial class CustomReportViewController : BaseController
    {
        public virtual ActionResult Index(string type)
        {
            LogI("Index");
            ViewBag.ReportId = 0;
            type = type ?? "CustomReportIncomeDisparityViewModel";
            var it = (CustomReportBaseDataViewModel)Activator.CreateInstance("Ccen.Web", "Ccen.Web.ViewModels.CustomReports.Entities." + type).Unwrap();           
            //var report = CustomReportViewModel.GetById(Db, id);
            ViewBag.Title = "";
            

            var model = new CustomReportView_ViewModel();

            //var CustomReportBaseDataViewModel

            model.ReportTitle = "";
            model.ReportId = 0;
            model.Fields = it.GetAllAvailableFields();
            model.Filters = it.GetAllAvailableFilters(Db);            
            model.ReportDataType = type;
            //model.Fil


            return View("Index", model);

            //return View();
        }
        //private static long ReportId;
        /*public virtual ActionResult Index(long id)
        {
            LogI("Index");
            ViewBag.ReportId = id;
            var report = CustomReportViewModel.GetById(Db, id);
            ViewBag.Title = report == null ? "" : report.Name;
            ViewBag.Columns = CustomReportDataItemViewModel.GetColumns(Db, id);

            var model = new CustomReportView_ViewModel();
            model.ReportTitle = report == null ? "" : report.Name;
            model.ReportId = id;
            model.Fields = CustomReportFieldViewModel.GetCustomReportFields(id, Db).ToList();            
            return View("Index", model);
        }*/        

        public virtual ActionResult GetReportItems([DataSourceRequest] DataSourceRequest request, CustomReportView_ViewModel model, [Bind(Prefix = "valuesList[]")] List<string> Filters)
        {
            LogI("GetAll");
            
            model.ReportDataType = model.ReportDataType ?? "CustomReportIncomeDisparityViewModel";
            //model.ReportDataType = model.ReportDataType ?? "CustomReportIncomeDisparityViewModel";
            var it = (CustomReportBaseDataViewModel)Activator.CreateInstance("Ccen.Web", "Ccen.Web.ViewModels.CustomReports.Entities." + model.ReportDataType).Unwrap();

            request.Sorts = BuildFrom(it);
            var filters = GetFilters(model, it);
            var items = it.GetAll(Db, filters.ToList());
            var dataSource = items.ToDataSourceResult(request);            
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private IEnumerable<CustomReportFilterViewModel> GetFilters(CustomReportView_ViewModel model, CustomReportBaseDataViewModel it)
        {
            Regex rg = new Regex(@"\(.*?\)");
            model.ValuesListString = rg.Replace(model.ValuesListString, "").Replace(" GMT+", ".").Replace(" GMT ", ".");
            model.ValuesList = model.ValuesListString.Split(';').ToList();
            model.IdsList = model.IdsListString.Split(';').Select(x => long.Parse(x)).ToList();
            var filters = model.IdsList.Zip(model.ValuesList, (i, v) => new { Id = i, Value = v }).Where(x=>!String.IsNullOrEmpty(x.Value)).ToList();
            var initFilters = it.GetAllAvailableFilters(Db);
            var res = from f in filters
                      join i in initFilters
                      on f.Id equals i.Id
                      select new CustomReportFilterViewModel()
                      {
                          Id = i.Id,

                          Field = i.Field,
                          Operation = i.Operation,

                          ValueString = f.Value,
                          Value = SystemTypeHelper.GetObjectByString(f.Value, i.Field.DataType, i.Multi)
                      };

            return res.Where(x=>!String.IsNullOrEmpty(x.ValueString));
        }
                
        public virtual ActionResult ExportToExcel(CustomReportView_ViewModel model)
        {
            LogI("ExportToExcel, reportId=" + model.ReportDataType);

            model.ReportDataType = model.ReportDataType ?? "CustomReportIncomeDisparityViewModel";
           
            var it = (CustomReportBaseDataViewModel)Activator.CreateInstance("Ccen.Web", "Ccen.Web.ViewModels.CustomReports.Entities." + model.ReportDataType).Unwrap();
            var filters = GetFilters(model, it);
            var items = it.GetTop1000(Db, filters.ToList());

            var ms = ExcelHelper.Export(items, it.GetType(), null);
            var arr = ms.ToArray();
            return File(arr,
                             "application/vnd.ms-excel", //MIME type of Excel files
                            $"CustomReport{ model.ReportDataType }.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user*/
        }

        private IList<SortDescriptor> BuildFrom(CustomReportBaseDataViewModel entity)
        {
            if (this.Request.Params.AllKeys.Contains("sort[0][field]"))
            {
                var dir = ListSortDirection.Ascending;
                if (this.Request.Params.AllKeys.Contains("sort[0][dir]"))
                    dir = this.Request.Params.GetValues("sort[0][dir]")[0] == "desc"
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;

                return new List<SortDescriptor>()
                {
                    new SortDescriptor(this.Request.Params.GetValues("sort[0][field]")[0],
                        dir)
                };
            }
            else
            {
                return new List<SortDescriptor>()
                {
                    new SortDescriptor(entity.SortField,
                        entity.SortOrder)
                };
            }
        }
    }
}