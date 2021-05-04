using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Users;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Orders;
using Ccen.Web;

namespace Amazon.Web.ViewModels.Emails
{
    public class ComposeEmailViewModel : IEmailInfo
    {
        public long Id { get; set; }

        public string OrderNumber { get; set; }
        public EmailTypes EmailType { get; set; }

        public int Type { get; set; }
        public int FolderType { get; set; }
        public int ResponseStatus { get; set; }
        public string AnswerMessageID { get; set; }
        public long? ReplyToEmailId { get; set; }

        [Display(Name = "From")]
        public string FromEmail { get; set; }
        public string FromName { get; set; }

        
        public string ToName { get; set; }
        [Display(Name = "To")]
        public string ToEmail { get; set; }

        public string ReceiveDateString
        {
            get { return DateHelper.ToDateTimeString(ReceiveDate); }
        }



        public string FolderName
        {
            get
            {
                if (FolderType == (int)EmailFolders.Inbox)
                    return "Inbox";
                if (FolderType == (int)EmailFolders.Sent)
                    return "Sent";
                return "n/a";
            }
        }

        public bool IsSent
        {
            get { return FolderType == (int)EmailFolders.Sent; }
        }

        public bool IsSystem
        {
            get { return Type == (int)IncomeEmailTypes.System; }
        }

        public bool ResponseNeeded
        {
            get
            {
                return EmailHelper.ResponseNeeded(AnswerMessageID, Type, ResponseStatus, FolderType);
            }
        }

        public bool ResponsePromised
        {
            get { return ResponseStatus == (int)EmailResponseStatusEnum.ResponsePromised; }
        }

        public bool ResponseDismissed
        {
            get { return ResponseStatus == (int)EmailResponseStatusEnum.NoResponseNeeded; }
        }

        public IList<ReturnQuantityItemViewModel> ExchangeItems { get; set; }

        public EmailAttachmentViewModel AttachedFile { get; set; }
        public EmailAttachmentViewModel AttachedLabel { get; set; }
        public IList<EmailAttachmentViewModel> AttachedStyleImages { get; set; }

        public IList<Attachment> Attachments
        {
            get
            {
                var attachments = new List<Attachment>();
                if (AttachedFile != null)
                {
                    var filePath = Path.Combine(UrlHelper.GetUploadEmailAttachmentPath(), AttachedFile.ServerFileName);
                    attachments.Add(new Attachment(filePath));
                }

                if (AttachedStyleImages != null && AttachedStyleImages.Any())
                {
                    foreach (var attachment in AttachedStyleImages)
                    {
                        var filePath = Path.Combine(UrlHelper.GetUploadEmailAttachmentPath(), attachment.ServerFileName);
                        attachments.Add(new Attachment(filePath));
                    }
                }

                if (AttachedLabel != null)
                {
                    var filePath = Path.Combine(UrlHelper.GetLabelPath(AttachedLabel.ServerFileName));
                    attachments.Add(new Attachment(filePath));
                }

                return attachments;
            }
        }

        public string Tag
        {
            get { return OrderNumber; }
        }

        public MarketType Market { get; set; }
        public ShipmentProviderType ShipmentProvider { get; set; }

        public MailAddress From
        {
            get
            {
                if (String.IsNullOrEmpty(FromEmail))
                    return null;
                return new MailAddress(FromEmail, FromName);
            }
            set
            {
                if (value != null)
                {
                    FromEmail = value.Address;
                    FromName = value.DisplayName;
                }
                else
                {
                    FromEmail = "";
                    FromName = "";
                }
            }
        }

        public List<MailAddress> ToList
        {
            get
            {
                if (!String.IsNullOrEmpty(ToEmail))
                {
                    var toEmails = ToEmail.Split(";, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var toNames = (ToName ?? "").Split(";,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    var emails = new List<MailAddress>();
                    for (var i = 0; i < toEmails.Length; i++)
                    {
                        emails.Add(new MailAddress(toEmails[i], toNames.Length > i ? toNames[i].Trim() : ""));
                    }

                    return emails;
                }
                return new List<MailAddress>();
            }
        }

        public List<MailAddress> CcList { get; set; }
        public List<MailAddress> BccList { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }


        public string NewComment { get; set; }


        #region Additional Info
        public DateTime ReceiveDate { get; set; }

        public string SubjectHtml { get; set; }

        public long? OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }

        #endregion
        
        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderNumber); }
        }
        
        public static ComposeEmailViewModel BuildFrom(IUnitOfWork db, IEmailInfo emailInfo)
        {
            var model = new ComposeEmailViewModel();
            model.EmailType = emailInfo.EmailType;
            model.OrderNumber = emailInfo.Tag;
            model.Market = emailInfo.Market;

            if (emailInfo.From != null)
            {
                model.FromEmail = emailInfo.From.Address;
                model.FromName = emailInfo.From.DisplayName;
            }

            if (emailInfo.ToList != null && emailInfo.ToList.Count > 0)
            {
                model.ToEmail = emailInfo.ToList[0].Address;
                model.ToName = emailInfo.ToList[0].DisplayName;
            }

            model.Body = emailInfo.Body;
            model.Subject = emailInfo.Subject;

            var order = db.Orders.GetByCustomerOrderNumber(emailInfo.Tag);
            if (order != null)
            {
                var label = db.Labels.GetByOrderIdAsDto(order.Id)
                    .OrderByDescending(l => l.LabelPurchaseDate)
                    .FirstOrDefault(l => !String.IsNullOrEmpty(l.TrackingNumber));
                if (label != null)
                    model.ShipmentProvider = (ShipmentProviderType)label.ShipmentProviderType;
            }
            
            return model;
        }

        public static CallResult<ComposeEmailViewModel> GetTemplateInfo(IUnitOfWork db,
            IEmailService emailService,
            CompanyDTO company,
            string byName,
            EmailTypes type,
            EmailAccountDTO emailAccountInfo,
            string orderNumber,
            long? replyToId)
        {
            var model = new ComposeEmailViewModel();
            var result = CallResult<ComposeEmailViewModel>.Success(model);

            if (!String.IsNullOrEmpty(orderNumber))
            {
                result = ComposeEmailViewModel.FromTemplate(db,
                    emailService,
                    orderNumber,
                    company,
                    byName,
                    (EmailTypes)type,
                    emailAccountInfo);

                if (result.IsSuccess)
                {
                    model = result.Data;
                }
            }

            var useReSubject = false;
            if (replyToId.HasValue)
            {
                var replyToEmail = db.Emails.Get(replyToId.Value);
                if (replyToEmail != null)
                {
                    model.Subject = "RE: " + replyToEmail.Subject;

                    if (String.IsNullOrEmpty(model.Body)) //For Custom template
                        model.Body = "<br/><br/><br/><br/>";
                    model.Body = model.Body + "<hr/><br/>" + replyToEmail.Message;

                    if (replyToEmail.From != "help@walmart.com"
                        && !(StringHelper.ContainsNoCase(replyToEmail.From, "@walmart.com")
                            && StringHelper.ContainsNoCase(replyToEmail.Subject, "Walmart Escalation"))) //NOTE: when reply to walmart support email do not use them
                    {
                        model.ToEmail = replyToEmail.From;
                    }

                    useReSubject = true;
                }
            }

            if ((model.Market == MarketType.Amazon
                || model.Market == MarketType.AmazonEU
                || model.Market == MarketType.AmazonAU))
            {
                if (!useReSubject)
                {
                    if (StringHelper.ContainsNoCase(model.Subject, "[Important]"))
                    {
                        model.Subject = "[Important] ";
                    }
                    else
                    {
                        model.Subject = "";
                    }
                    model.Subject += "Additional Information Required (Order: " + orderNumber + ")";
                    
                }
            }

            model.FromName = emailAccountInfo.DisplayName;
            model.FromEmail = emailAccountInfo.FromEmail;

            model.ReplyToEmailId = replyToId;

            return result;
        }

        public static CallResult<ComposeEmailViewModel> FromTemplate(IUnitOfWork db, 
            IEmailService emailService,
            string orderId, 
            CompanyDTO company,
            string byName,
            EmailTypes type,
            EmailAccountDTO emailAccountInfo)
        {
            try
            {
                ComposeEmailViewModel model = new ComposeEmailViewModel();

                orderId = OrderHelper.RemoveOrderNumberFormat(orderId);

                var order = db.Orders.GetAll().FirstOrDefault(q => q.AmazonIdentifier.ToLower() == orderId || q.CustomerOrderId == orderId);

                if (order != null)
                {
                    IEmailInfo info = emailService.GetEmailInfoByType(db,
                        type,
                        company,
                        byName,
                        order?.AmazonIdentifier,
                        new Dictionary<string, string>(),
                        null,
                        null);

                    model = ComposeEmailViewModel.BuildFrom(db, info);
                }

                model.FromName = emailAccountInfo.DisplayName;
                model.FromEmail = emailAccountInfo.FromEmail;

                if (!StringHelper.ContainsNoCase(model.Subject, "[Important]")
                    && (model.Market == MarketType.Amazon
                    || model.Market == MarketType.AmazonEU
                    || model.Market == MarketType.AmazonAU
                    || model.Market == MarketType.None))
                {
                    var existEmailFromCustomer = db.Emails.GetAll().Any(e => e.From == model.ToEmail);
                    if (!existEmailFromCustomer)
                        model.Subject = "[Important] " + model.Subject;
                }

                return CallResult<ComposeEmailViewModel>.Success(model);
            }
            catch (Exception ex)
            {
                return CallResult<ComposeEmailViewModel>.Fail(ex.Message, ex);
            }
        }
        
        public List<MessageString> Validate(IUnitOfWork db,
            ILogService log,
            DateTime when)
        {
            var messages = new List<MessageString>();
            var emailType = this.EmailType;

            if (ReplyToEmailId.HasValue)
            {
                var replyToEmail = db.Emails.Get(ReplyToEmailId.Value);
                if (replyToEmail.Type == (int) IncomeEmailTypes.CancellationRequest)
                {
                    log.Info("Add message: Cancellation emails usually processed by system automatically. Are you sure you would like to send the response?");
                    messages.Add(new MessageString()
                    {
                        Message = "Cancellation emails usually processed by system automatically. Are you sure you would like to send the response?",
                        Status = MessageStatus.Info,
                    });
                }
            }

            if (!String.IsNullOrEmpty(OrderNumber)
                && emailType == EmailTypes.LostPackage)
            {
                var emailNotify = db.OrderEmailNotifies
                    .GetAll()
                    .OrderByDescending(o => o.CreateDate)
                    .FirstOrDefault(o => o.OrderNumber == OrderNumber
                        && o.Type == (int)OrderEmailNotifyType.OutputLostPackageEmail);

                if (emailNotify != null)
                {
                    log.Info("Add message: Lost template was already sent for this order on " + DateHelper.ToDateString(emailNotify.CreateDate) + ", are you sure you want to send it again?");
                    messages.Add(new MessageString()
                    {
                        Message = "Lost template was already sent for this order on " + DateHelper.ToDateString(emailNotify.CreateDate) + ", are you sure you want to send it again?",
                        Status = MessageStatus.Info,
                    });
                }
            }

            if (!String.IsNullOrEmpty(OrderNumber)
                && emailType == EmailTypes.LostPackage)
            {
                var order = db.Orders.GetByOrderIdAsDto(OrderNumber);
                if (order != null)
                {
                    var labels = db.Labels.GetByOrderIdAsDto(order.Id).ToList();
                    OrderShippingInfoDTO label = null;
                    if (labels.Any())
                    {
                        label = labels.OrderByDescending(sh => sh.LabelPurchaseDate).FirstOrDefault(i => i.IsActive);
                    }

                    if (label == null || label.DeliveredStatus != (int) DeliveredStatusEnum.Delivered)
                    {
                        log.Info("Add message: The order isn't marked yet as delivered, are you sure you want to send this message?");
                        messages.Add(new MessageString()
                        {
                            Message = "The order isn't marked yet as delivered, are you sure you want to send this message?",
                            Status = MessageStatus.Info,
                        });
                    }
                }
            }


            if (!String.IsNullOrEmpty(OrderNumber)
                && emailType == EmailTypes.ReturnInstructions)
            {
                var order = db.Orders.GetByOrderIdAsDto(OrderNumber);
                if (order != null && order.OrderDate.HasValue)
                {
                    var orderShippings = db.OrderShippingInfos
                        .GetByOrderIdAsDto(order.Id)
                        .Where(sh => sh.IsActive)
                        .ToList();

                    var returnRequestToLate = OrderHelper.AcceptReturnRequest(order.OrderDate.Value,
                            order.EarliestDeliveryDate,
                            orderShippings.Max(sh => sh.ActualDeliveryDate),
                            when,
                            when);
                    
                    if (returnRequestToLate)
                    {
                        log.Info("Add message: The order was placed over 30 days ago, are you sure you want to send this message?");
                        messages.Add(new MessageString()
                        {
                            Message = "The order was placed over 30 days ago, are you sure you want to send this message?",
                            Status = MessageStatus.Info,
                        });
                    }
                }
            }
            return messages;
        }

        public void SendEmail(IUnitOfWork db,
            ILogService log,
            IEmailService emailService,
            IQuantityManager quantityManager,
            DateTime when,
            long? by)
        {
            CallResult<Exception> result;
            if (AppSettings.IsDemo)
                result = CallResult<Exception>.Success(null);
            else
                result = emailService.SendEmail(this, CallSource.UI, by);

            CallHelper.ThrowIfFail(result);

            Order order = null;
            switch (this.EmailType)
            {
                case EmailTypes.RequestFeedback:
                    db.Orders.UpdateRequestedFeedback(this.OrderNumber);
                    break;
                case EmailTypes.AddressVerify:
                    db.Orders.UpdateRequestedAddressVerify(this.OrderNumber);
                    break;
                case EmailTypes.LostPackage:
                    break;
                case EmailTypes.LostPackage2:
                    break;
                case EmailTypes.UndeliverableAsAddressed:
                    break;
                case EmailTypes.ExchangeInstructions:
                    if (ExchangeItems != null)
                    {
                        var exchangeToItems = ExchangeItems.Where(i => i.InputQuantity > 0).ToList();
                        if (exchangeToItems.Any())
                        {
                            var model = new QuantityOperationDTO()
                            {
                                Type = (int)QuantityOperationType.ExchangeOnHold,
                                OrderId = this.OrderNumber,
                                Comment ="Sent Exchange Instruction",
                                QuantityChanges = exchangeToItems.Select(i => new QuantityChangeDTO()
                                {
                                    Quantity = i.InputQuantity,
                                    StyleId = i.ExchangeStyleId ?? 0,
                                    StyleItemId = i.ExchangeStyleItemId ?? 0,
                                    StyleString = i.ExchangeStyleString,
                                    ExpiredOn = when.AddDays(14),
                                    Tag = "Exchanged from: " + i.StyleString + ", styleItemId: " + i.StyleItemId,
                                }).ToList(),
                            };
                            quantityManager.AddQuantityOperation(db, model, when, by);
                        }

                        foreach (var item in ExchangeItems)
                        {
                            log.Info("Exchange item, from styleId=" + item.StyleId + " styleItemId=" + item.StyleItemId +
                                     " --- to styleId=" + item.ExchangeStyleId + ", styleItemId=" +
                                     item.ExchangeStyleItemId + ", quantity=" + item.InputQuantity);
                            //TODO: save into DB
                        }
                    }
                    break;
            }

            if (!String.IsNullOrEmpty(NewComment))
            {
                AddOrderComment(db,
                        OrderNumber,
                        NewComment,
                        when,
                        by);
            }

            var emailNotifyType = EmailNotifyHelper.GetEmailNotifyFrom(this.EmailType);
            db.OrderEmailNotifies.Add(new OrderEmailNotify()
            {
                OrderNumber = this.OrderNumber,
                Reason = "User emailed, webpage",
                Type = (int)emailNotifyType,
                CreateDate = when,
                CreatedBy = by
            });
            db.Commit();

            if (ReplyToEmailId.HasValue)
            {
                var email = db.Emails.Get(ReplyToEmailId.Value);
                if (email != null)
                {
                    email.ResponseStatus = (int) EmailResponseStatusEnum.Sent;
                    db.Commit();
                    log.Info("Mark as responsed (as reply), emailId=" + email.Id);
                }
            }
            
            if (!String.IsNullOrEmpty(this.OrderNumber))
            {
                var orderEmails = db.Emails.GetAllByOrderId(this.OrderNumber)
                    .Where(e => e.FolderType == (int)EmailFolders.Inbox
                        && ((String.IsNullOrEmpty(e.AnswerMessageID)
                            && e.ResponseStatus == (int)EmailResponseStatusEnum.None)
                            || e.ResponseStatus == (int)EmailResponseStatusEnum.ResponsePromised))
                    .ToList();
                foreach (var email in orderEmails)
                {
                    var dbEmail = db.Emails.Get(email.Id);
                    dbEmail.ResponseStatus = (int) EmailResponseStatusEnum.Sent;
                    log.Info("Mark as responsed (by orderNumber), emailId=" + email.Id);
                }
                db.Commit();
            }
        }

        private void AddOrderComment(IUnitOfWork db,
            string orderNumber,
            string commentText,
            DateTime when,
            long? by)
        {
            var orders = db.Orders.GetAllByCustomerOrderNumbers(new List<string>() { orderNumber });
            foreach (var order in orders)
            { 
                db.OrderComments.Add(new OrderComment()
                {
                    OrderId = order.Id,
                    Type = (int)CommentType.OutputEmail,
                    Message = commentText,
                    CreateDate = when,
                    CreatedBy = by,
                });
                db.Commit();
            }
        }
    }
}