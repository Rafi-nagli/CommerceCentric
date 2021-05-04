using Amazon.Common.Helpers;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.UploadOrders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllClients)]
    public partial class UploadOrdersController : BaseController
    {
        public override string TAG
        {
            get { return "UploadOrdersController."; }
        }

        private const string PopupContentView = "UploadOrdersFeedViewModel";

        public virtual ActionResult Index()
        {
            LogI("Index");

            var model = UploadOrderFeedFilterViewModel.Default;
            return View("Index", model);
        }

        public virtual ActionResult GetAll([DataSourceRequest] DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? status)
        {
            LogI("GetAll, "
                + ", dateFrom=" + dateFrom
                + ", dateTo=" + dateTo
                + ", status=" + status);

            var searchFilter = new UploadOrderFeedFilterViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
            };
            var items = UploadOrderFeedViewModel.GetAll(Db, Time, searchFilter);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }


        public virtual ActionResult AddFeed()
        {
            LogI("AddFeed");

            var model = new UploadOrderFeedViewModel();
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        public virtual ActionResult GetFeed(long id)
        {
            LogI("GetLabelPrintFile, id=" + id);

            var uploadAction = Db.SystemActions.Get(id);
            if (uploadAction != null)
            {
                var inputModel = SystemActionHelper.FromStr<PublishFeedInput>(uploadAction.InputData);
                var filepath = Path.Combine(Models.UrlHelper.GetUploadOrderFeedPath(), inputModel.FileName);
                var filename = inputModel.FileName;
                return File(filepath, FileHelper.GetMimeTypeByExt(Path.GetExtension(filepath)), filename);
            }

            return new HttpNotFoundResult();
        }

        //public virtual ActionResult Validate(PublishFeedViewModel model)
        //{
        //    LogI("Submit");
        //    CallMessagesResultVoid result = new CallMessagesResultVoid();

        //    var messages = model.Validate();
        //    if (!messages.Any())
        //    {
        //        result = PublishFeedViewModel.GetPreviewMessages(Db, ActionService, model, AccessManager.UserId);
        //    }
        //    else
        //    {
        //        result = new CallMessagesResultVoid()
        //        {
        //            Messages = messages,
        //            Status = CallStatus.Fail
        //        };
        //    }

        //    return Json(result);
        //}

        public virtual ActionResult Submit(UploadOrderFeedViewModel model)
        {
            LogI("Submit");
            CallMessagesResultVoid result = new CallMessagesResultVoid();

            var messages = model.Validate();
            if (!messages.Any())
            {
                result = UploadOrderFeedViewModel.Add(Db, ActionService, model, AccessManager.UserId);
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
                        var dir = Models.UrlHelper.GetUploadOrderFeedPath();
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