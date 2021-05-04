using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class EmailController : BaseController
    {
        public override string TAG
        {
            get { return "EmailController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index(string orderId)
        {
            LogI("Index");
            var model = new EmailFilterViewModel()
            {
                OrderNumber = orderId
            };
            if (String.IsNullOrEmpty(orderId))
            {
                model.ResponseStatus = (int) EmailResponseStatusFilterEnum.ResponseNeeded;
            }

            return View(model);
        }

        public virtual ActionResult GetAllEmails([DataSourceRequest]DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo,  
            string buyerName,
            string orderNumber,
            int? market,
            bool onlyIncoming,
            bool onlyWithoutAnswer,
            bool includeSystem,
            int? responseStatus)
        {
            LogI("GetAllEmails");

            request.Sorts = new List<SortDescriptor>()
            {
                new SortDescriptor("ReceiveDate", ListSortDirection.Descending)
            };

            var filter = new EmailFilterViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                BuyerName = buyerName,
                OrderNumber = orderNumber,
                OnlyIncoming = onlyIncoming,
                OnlyWithoutAnswer = onlyWithoutAnswer,
                IncludeSystem = includeSystem,
                ResponseStatus = responseStatus,
                Market = market,
            };

            var items = EmailViewModel.GetAll(Db, AccessManager.Company, filter);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public virtual ActionResult UploadAttachment([DataSourceRequest] DataSourceRequest request,
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
                        var dir = Models.UrlHelper.GetUploadEmailAttachmentPath();
                        var fileName = FileHelper.GetNotExistFileName(dir, sourceFileName);
                        var destinationPath = Path.Combine(dir, fileName);

                        file.SaveAs(destinationPath);

                        savedAttachments.Add(new EmailAttachmentViewModel() {
                            ServerFileName = fileName,
                            SourceFileName = sourceFileName,
                        });
                    }
                }
            }

            return Json(new[] { savedAttachments }.ToDataSourceResult(request));
        }

        public virtual ActionResult AttachStyleImage(string styleId)
        {
            LogI("AttachStyleImage, styleId=" + styleId);
            var attachment = EmailAttachmentViewModel.GetStyleImageAsAttachment(Db, LogService, Time, styleId);
            
            return JsonGet(ValueResult<EmailAttachmentViewModel>.Success("", attachment));
        }

        public virtual ActionResult Validate(ComposeEmailViewModel model)
        {
            LogI("Validate, model=" + model);
            var messages = model.Validate(Db, LogService, Time.GetAppNowTime());
            return JsonGet(new ValueResult<IList<MessageString>>(true, "", messages));
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult SendEmail(ComposeEmailViewModel model)
        {
            try
            {
                var by = AccessManager.UserId;
                var company = AccessManager.Company;
                var emailService = new EmailService(LogService, SettingsBuilder.GetSmtpSettingsFromCompany(company), AddressService);
                
                model.SendEmail(Db,
                    LogService,
                    emailService,
                    QuantityManager,
                    Time.GetAppNowTime(),
                    by);
                
                return Json(new MessagesResult(true).Success("Message has been successfully sent"));
            }
            catch (Exception ex)
            {
                LogE("Reply All", ex);
                return Json(new MessagesResult(false).Error("Can't send email. Details: " + ex.Message));
            }
        }

        public virtual ActionResult TestEmail()
        {
            LogI("TestEmail");
            var company = AccessManager.Company;
            var smtpSettings = CompanyHelper.GetSmtpSettings(company);
            var byName = AccessManager.UserName;

            var result = ComposeEmailViewModel.FromTemplate(Db, 
                EmailService,
                String.Empty, 
                company, 
                byName,
                EmailTypes.System,
                smtpSettings);

            var model = new ComposeEmailViewModel();
            if (result.Status == CallStatus.Success)
            {
                model = result.Data;
            }


            model.Subject = "Check email from web page";
            model.ToEmail = "support@dgtex.com";
            model.ToName = "Support";


            return View("ComposeEmail", model);
        }

        public virtual ActionResult ComposeEmailFromTemplate(string orderNumber, 
            EmailTypes type,
            long? replyToId)
        {
            LogI("ComposeEmailFromTemplate, orderNumber=" + orderNumber + ", type=" + type.ToString() + ", replyToEmailId=" + replyToId);

            var company = AccessManager.Company;
            var smtpSettings = CompanyHelper.GetSmtpSettings(company);
            var byName = AccessManager.UserName;

            var result = ComposeEmailViewModel.GetTemplateInfo(Db,
                EmailService,
                company,
                byName,
                type,
                smtpSettings,
                orderNumber,
                replyToId);

            return View("ComposeEmail", result.Data);
        }

        public virtual ActionResult GetTemplateInfo(string orderNumber,
            EmailTypes type,
            long? replyToId)
        {
            LogI("GetTemplateInfo, orderNumber=" + orderNumber + ", type=" + type + ", replyToId=" + replyToId);

            var company = AccessManager.Company;
            var smtpSettings = CompanyHelper.GetSmtpSettings(company);
            var byName = AccessManager.UserName;

            var result = ComposeEmailViewModel.GetTemplateInfo(Db,
                EmailService,
                company,
                byName,
                (EmailTypes) type,
                smtpSettings,
                orderNumber,
                replyToId);

            return Json(new ValueResult<ComposeEmailViewModel>(result.Status == CallStatus.Success, 
                    result.Message, 
                    result.Data), 
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ReplyTo(long replyToId, 
            string orderNumber,
            EmailTypes? type)
        {
            LogI("ReplyTo, orderNumber=" + orderNumber + ", type=" + type + ", replyToId=" + replyToId);

            var company = AccessManager.Company;
            var smtpSettings = CompanyHelper.GetSmtpSettings(company);
            var byName = AccessManager.UserName;

            var result = ComposeEmailViewModel.GetTemplateInfo(Db,
                EmailService,
                company,
                byName,
                type ?? EmailTypes.CustomEmail,
                smtpSettings,
                orderNumber,
                replyToId);

            return View("ComposeEmail", result.Data);
        }

        public virtual ActionResult ComposeEmail(string orderNumber)
        {
            LogI("ComposeEmail, orderNumber=" + orderNumber);

            var company = AccessManager.Company;
            var smtpSettings = CompanyHelper.GetSmtpSettings(company);
            var byName = AccessManager.UserName;
            
            var result = ComposeEmailViewModel.GetTemplateInfo(Db,
                EmailService,
                company,
                byName,
                EmailTypes.CustomEmail,
                smtpSettings,
                orderNumber,
                null);

            return View("ComposeEmail", result.Data);
        }

        public virtual ActionResult SetNoResponseNeeded(long emailId, string orderNumber)
        {
            LogI("OnNoResponseNeeded, emailId=" + emailId + ", orderNumber=" + orderNumber);
            EmailViewModel.SetResponseStatus(Db, 
                emailId,
                EmailResponseStatusEnum.NoResponseNeeded,
                OrderHistoryService,
                orderNumber,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(MessageResult.Success());
        }

        public virtual ActionResult DismissResponsePromised(long emailId, string orderNumber)
        {
            LogI("DismissResponsePromised, emailId=" + emailId + ", orderNumber=" + orderNumber);
            EmailViewModel.SetResponseStatus(Db,
                emailId,
                EmailResponseStatusEnum.NoResponseNeeded,
                OrderHistoryService,
                orderNumber,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(MessageResult.Success());
        }

        public virtual ActionResult MarkAsReviewed(long emailId, string orderNumber)
        {
            LogI("MarkAsReviewed, emailId=" + emailId + ", orderNumber=" + orderNumber);
            EmailViewModel.SetReviewedStatus(Db,
                emailId,
                true,
                OrderHistoryService,
                orderNumber,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(ValueResult<bool>.Success(null, true));
        }

        public virtual ActionResult ViewEmail(long id, string orderNumber)
        {
            var model = ViewEmailViewModel.BuildFrom(Db, LogService, AccessManager.Company, id, orderNumber);

            return View("ViewEmail", model);
        }

        public virtual ActionResult GetAttachment(long id)
        {
            var file = EmailAttachmentViewModel.GetAttachmentFilepath(Db, id);
            var filename = Path.GetFileName(file);
            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }

        public virtual ActionResult AssignEmailToOrder(long emailId, string orderNumber)
        {
            LogI("AssignEmailToOrder, emailId=" + emailId + ", orderNumber=" + orderNumber);

            var messages = EmailViewModel.AssignToOrder(Db, 
                LogService, 
                emailId, 
                orderNumber, 
                Time.GetAppNowTime(), 
                AccessManager.UserId);

            return JsonGet(new MessagesResult()
            {
                IsSuccess = !messages.Any(),
                Messages = messages
            });
        }

        [HttpPost]
        public virtual ActionResult Escalate(long emailId, string orderNumber, bool escalated)
        {
            EmailViewModel.Escalate(Db, Time, emailId, orderNumber, escalated);
            return Json(new MessagesResult(true).Success($"Email was { (escalated ? "" : "de") }escalated"));
        }
    }
}
