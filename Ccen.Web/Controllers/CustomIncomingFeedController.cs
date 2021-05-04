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
using Amazon.Web.ViewModels.Messages;
using Ccen.Model.Implementation;
using Ccen.Web.ViewModels.CustomFeeds;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllRoles)]
    public partial class CustomIncomingFeedController : BaseController
    {
        private const string PopupContentView = "CustomFeedViewModel";

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAll");

            var items = CustomIncomingFeedViewModel.GetAll(DbFactory);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetSourceFields(int dsFeedType, int dsProductType)
        {
            LogI("GetSourceFields");

            var customFeedSevice = new CustomFeedService(LogService, Time, DbFactory);
            var sourceFields = CustomIncomingFeedViewModel.GetFeedFields(customFeedSevice, (DSFileTypes)dsFeedType, (DSProductType)dsProductType);

            return JsonGet(CallResult<IList<CustomFeedFieldViewModel>>.Success(sourceFields));
        }

        public virtual ActionResult UploadSampleFeed([DataSourceRequest] DataSourceRequest request,
                IEnumerable<HttpPostedFileBase> files)
        {
            var columns = new List<CustomFeedSourceFieldViewModel>();

            if (files != null)
            {
                foreach (var file in files)
                {
                    var sourceFileName = Path.GetFileName(file.FileName);
                    if (sourceFileName != null)
                    {
                        var dir = Models.UrlHelper.GetUploadSampleIncomingFeedPath();
                        var fileName = FileHelper.GetNotExistFileName(dir, sourceFileName);
                        var destinationPath = Path.Combine(dir, fileName);

                        file.SaveAs(destinationPath);

                        columns = CustomFeedSourceFieldViewModel.BuildFromFile(destinationPath);
                    }
                }
            }

            return JsonGet(CallResult<IList<CustomFeedSourceFieldViewModel>>.Success(columns));
        }

        public virtual ActionResult EditFeed(long? id)
        {
            LogI("EditFeed, id=" + id);

            var customFeedSevice = new CustomFeedService(LogService, Time, DbFactory);
            var model = new CustomIncomingFeedViewModel(Db, customFeedSevice, id);

            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [HttpPost]
        public virtual ActionResult DeleteFeed(long id)
        {
            LogI("DeleteFeed, id=" + id);

            CustomIncomingFeedViewModel.Delete(DbFactory, id);

            return JsonGet(ValueResult<long>.Success("", id));
        }

        public virtual ActionResult Submit(CustomIncomingFeedViewModel model)
        {
            LogI("Submit");
            CallMessagesResultVoid result = new CallMessagesResultVoid();

            var messages = model.Validate();
            if (!messages.Any())
            {
                result = CustomIncomingFeedViewModel.Apply(Db, model, Time.GetAmazonNowTime(), AccessManager.UserId);
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
    }
}