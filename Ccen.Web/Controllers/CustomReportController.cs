using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
using Ccen.Model.Implementation;
using Ccen.Web.ViewModels.CustomReports;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllRoles)]
    public partial class CustomReportController : BaseController
    {
        private const string PopupContentView = "CustomReportViewModel";

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAll");

            var items = CustomReportViewModel.GetAll(DbFactory);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetAvailableFilters(CustomReportViewModel model)
        {
            LogI("GetSourceFields");
            var fields = model.UsedFields;
            var customFeedSevice = new CustomFeedService(LogService, Time, DbFactory);
            var sourceFields = CustomReportFilterViewModel.GetAvailableFiltersForReport(Db, fields.Select(x => x.CustomReportPredefinedFieldId).ToList());
            return JsonGet(CallResult<IList<CustomReportFilterViewModel>>.Success(sourceFields));
        }

        public virtual ActionResult EditFeed(long? id)
        {
            LogI("EditFeed, id=" + id);            
            var model = new CustomReportViewModel(Db, id);

            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [HttpPost]
        public virtual ActionResult DeleteFeed(long id)
        {
            LogI("DeleteFeed, id=" + id);

            CustomReportViewModel.Delete(DbFactory, id);

            return JsonGet(ValueResult<long>.Success("", id));
        }

        public virtual ActionResult Submit(CustomReportViewModel model)
        {
            LogI("Submit");
            CallMessagesResultVoid result = new CallMessagesResultVoid();

            var messages = model.Validate();
            if (!messages.Any())
            {
                result = CustomReportViewModel.Apply(Db, model, Time.GetAmazonNowTime(), AccessManager.UserId);
            }
            else
            {
                result = new CallMessagesResultVoid()
                {
                    Messages = messages,
                    Status = CallStatus.Fail
                };
            }

            return Json(result);
        }

        public virtual ActionResult CheckConnection(CustomIncomingFeedViewModel model)
        {
            LogI("CheckConnection");
            var result = model.CheckConnection();
            return Json(result);
        }

        public virtual ActionResult ExportToExcel(long id)
        {
            LogI("ExportToExcel, reportId=" + id);

            return null;
            /*return File(CustomReportDataItemViewModel.BuildReport(Db, id).ToArray(),   //The binary data of the XLS file
                            "application/vnd.ms-excel", //MIME type of Excel files
                            $"CustomReport{ id }.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user*/
        }
    }
}