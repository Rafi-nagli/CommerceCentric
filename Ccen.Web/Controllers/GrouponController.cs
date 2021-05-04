using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Emails;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Amazon.Core.Models.Calls;
using Amazon.Web.ViewModels.Groupon;

namespace Amazon.Web.Controllers
{
    [System.Web.Http.Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class GrouponController : BaseController
    {
        public override string TAG
        {
            get { return "GrouponController."; }
        }

        //private const string PopupContentView = "PublishFeedViewModel";

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View("Index");
        }

        public virtual ActionResult GetFeed(string filename)
        {
            LogI("GetFeed, fileName=" + filename);

            var filepath = Path.Combine(Models.UrlHelper.GetActualizeGrouponQtyFeedPath(), filename);
            return File(filepath, FileHelper.GetMimeTypeByExt(Path.GetExtension(filepath)), filename);
        }

        public virtual ActionResult Submit(ActualizeGrouponQtyFeedViewModel model)
        {
            LogI("Submit");
            CallMessagesResult<string> result = new CallMessagesResult<string>();

            var messages = model.Validate();
            if (!messages.Any())
            {
                result = model.UpdateFeed(LogService, Time, DbFactory, AutoCreateListingService);
            }
            else
            {
                result = new CallMessagesResult<string>()
                {
                    Messages = messages,
                    Status = CallStatus.Fail
                };
            }

            return Json(result);
        }

        public virtual ActionResult UploadFeed([DataSourceRequest] DataSourceRequest request,
            IEnumerable<HttpPostedFileBase> files)
        {
            var savedAttachments = new List<EmailAttachmentViewModel>();

            if (files != null)
            {
                foreach (var file in files)
                {
                    var sourceFileName = Path.GetFileName(file.FileName);
                    if (sourceFileName != null)
                    {
                        var dir = Models.UrlHelper.GetActualizeGrouponQtyFeedPath();
                        var fileName = FileHelper.GetNotExistFileName(dir, sourceFileName);
                        var destinationPath = Path.Combine(dir, fileName);

                        file.SaveAs(destinationPath);

                        savedAttachments.Add(new EmailAttachmentViewModel()
                        {
                            ServerFileName = fileName,
                            SourceFileName = sourceFileName,
                        });
                    }
                }
            }

            return Json(new[] { savedAttachments }.ToDataSourceResult(request));
        }
    }
}