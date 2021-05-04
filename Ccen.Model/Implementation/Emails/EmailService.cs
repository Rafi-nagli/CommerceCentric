using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;


namespace Amazon.Model.Implementation
{
    public class EmailService : IEmailService
    {
        private string _systemEmailPrefix;

        private string _host;
        private int _port;
        private string _username;
        private string _password;

        private string _fromEmail;
        private string _fromDisplayName;

        private bool _isDebug;
        private ILogService _log;
        private IAddressService _addressService;

        public IAddressService AddressService
        {
            get { return _addressService; }
        }

        public EmailService(ILogService log, 
            IEmailSmtpSettings settings,
            IAddressService addressService)
        {
            _log = log;
            _addressService = addressService;

            _systemEmailPrefix = settings.SystemEmailPrefix;

            _host = settings.SmtpHost;
            _port = settings.SmtpPort;
            _username = settings.SmtpUsername;
            _password = settings.SmtpPassword;

            _fromEmail = settings.SmtpFromEmail;
            _fromDisplayName = settings.SmtpFromDisplayName;

            _isDebug = settings.IsDebug;
        }

        private string GetFrom(MarketType market)
        {
            if (market == MarketType.Walmart
                || market == MarketType.WalmartCA)
                return _fromEmail.Replace("support@pre", "walmart@pre");
            return _fromEmail;
        }

        private class MarketEmailStatus
        {
            public string Title { get; set; }
            public IList<MarketType> Markets { get; set; }
            public int EmailCount { get; set; }

            public MarketEmailStatus(string title, int count, IList<MarketType> markets)
            {
                Title = title;
                EmailCount = count;
                Markets = markets;
            }
        }

        public CallResult<Exception> SendEmailStatusNotification(IList<EmailOrderDTO> emails)
        {
            try
            {
                var responseNeededEmails = emails.OrderByDescending(e => e.ReceiveDate).ToList();

                if (responseNeededEmails.Any())
                {
                    var marketList = new List<MarketEmailStatus>()
                    {
                        new MarketEmailStatus("Amazon", 0, new List<MarketType>() {MarketType.Amazon, MarketType.AmazonEU, MarketType.AmazonAU}),
                        new MarketEmailStatus("Walmart", 0, new List<MarketType>() {MarketType.Walmart, MarketType.WalmartCA}),
                        new MarketEmailStatus("eBay", 0, new List<MarketType>() {MarketType.eBay})
                    };
                    
                    foreach (var market in marketList)
                    {
                        market.EmailCount = responseNeededEmails.Count(e => e.Market.HasValue && market.Markets.Contains((MarketType)e.Market));
                    }

                    if (marketList.All(m => m.EmailCount == 0))
                    {
                        return new CallResult<Exception>()
                        {
                            Status = CallStatus.Success,
                        };
                    }

                    var marketInfoString = String.Join(", ", marketList.Select(m => m.Title + " - " + m.EmailCount));
                    var subject = String.Format("Unanswered messages: {0}", marketInfoString);

                    var body = "";
                    foreach (var market in marketList)
                    {
                        if (market.EmailCount > 0)
                        {
                            body += String.Format("The following {0} messages without answers:", market.Title);
                            body += "<ul>";
                            foreach (var email in responseNeededEmails.Where(e => e.Market.HasValue
                                && market.Markets.Contains((MarketType)e.Market)))
                            {
                                body += "<li> [" + DateHelper.ToDateTimeString(email.ReceiveDate) + "] " + email.Subject +
                                        "</li>";
                            }
                            body += "</ul>";
                        }
                    }

                    return SendSystemEmail(subject, body,
                        EmailHelper.RafiEmail,
                        EmailHelper.SupportDgtexEmail,
                        null);
                }
                else
                {
                    _log.Info("There are no messages required the response");
                }
            }
            catch (Exception ex)
            {
                _log.Error("SendEmailStatusNotification", ex);
                return new CallResult<Exception>()
                {
                    Status = CallStatus.Fail,
                    Data = ex,
                };
            }
            return new CallResult<Exception>()
            {
                Status = CallStatus.Success,
            };
        }

        public CallResult<Exception> SendSystemEmailToAdmin(string subject,
            string body)
        {
            return SendSystemEmail(subject, body, "support@dgtex.com", null, null);
        }

        public CallResult<Exception> SendSmsEmail(string message,
            string to)
        {
            try
            {
                var info = new SmsEmailInfo(_addressService,
                    message,
                    to,
                    "");

                return SendEmail(info, CallSource.Service);
            }
            catch (Exception ex)
            {
                _log.Error("Can't send email", ex);
                return new CallResult<Exception>()
                {
                    Status = CallStatus.Fail,
                    Data = ex,
                };
            }
        }

        public CallResult<Exception> SendSystemEmail(string subject,
            string body,
            string to,
            string cc,
            string bcc)
        {
            return SendSystemEmailWithAttachments(subject, body, new string[] { }, to, cc, bcc);
        }

        public CallResult<Exception> SendSystemEmailWithStreamAttachment(string subject,
                    string body,
                    Stream fileStream,
                    string fileName,
                    string to,
                    string cc,
                    string bcc = "")
        {
            try
            {
                subject = "[" + _systemEmailPrefix + ".commercentric.com] " + subject;

                body += @"<br/>
                          <p>System Notification</p>";

                body = "<div style='font-size: 11pt; font-family: Calibri'>" + body + "</div>";

                var info = new RawEmailInfo(_addressService,
                    subject,
                    body,
                    (fileStream != null && fileName != null) ? new Attachment[] { new Attachment(fileStream, fileName) } : null,
                    "",
                    to,
                    "",
                    cc,
                    "",
                    bcc);

                return SendEmail(info, CallSource.Service);
            }
            catch (Exception ex)
            {
                _log.Error("Can't send email", ex);
                return new CallResult<Exception>()
                {
                    Status = CallStatus.Fail,
                    Data = ex,
                };
            }
        }

        public CallResult<Exception> SendSystemEmailWithAttachments(string subject,
            string body,
            string[] attachmentFilenames,
            string to,
            string cc,
            string bcc)
        {
            Attachment[] attachments = null;
            if (attachmentFilenames != null && attachmentFilenames.Any())
                attachments = attachmentFilenames.Select(a => new Attachment(a)).ToArray();

            return SendSystemEmailWithAttachments(subject,
                body,
                attachments,
                to,
                cc,
                bcc);
        }

        public CallResult<Exception> SendSystemEmailWithAttachments(string subject,
            string body,
            Attachment[] attachments,
            string to,
            string cc,
            string bcc)
        {
            try
            {
                subject = "[" + _systemEmailPrefix + ".commercentric.com] " + subject;

                body += @"<br/>
                          <p>System Notification</p>";

                body = "<div style='font-size: 11pt; font-family: Calibri'>" + body + "</div>";

                var info = new RawEmailInfo(_addressService,
                    subject,
                    body,
                    attachments,
                    "",
                    to,
                    "",
                    cc,
                    "",
                    bcc);

                return SendEmail(info, CallSource.Service);
            }
            catch (Exception ex)
            {
                _log.Error("Can't send email", ex);
                return new CallResult<Exception>()
                {
                    Status = CallStatus.Fail,
                    Data = ex,
                };
            }
        }

        public CallResult<Exception> SendEmail(IEmailInfo emailInfo, CallSource callSource)
        {
            return SendEmail(emailInfo, callSource, null);
        }

        public CallResult<Exception> SendEmail(IEmailInfo emailInfo, CallSource callSource, long? by)
        {
            try
            {
                _log.Info("Send email, type=" + emailInfo.EmailType + ", orderNumber=" + emailInfo.Tag);
                var email = ComposeEmail(emailInfo, callSource, by);
                return SendEmail(email);
            }
            catch (Exception ex)
            {
                _log.Error("Can't send email", ex);
                return new CallResult<Exception>()
                {
                    Status = CallStatus.Fail,
                    Data = ex,
                };
            }
        }

        private CallResult<Exception> SendEmail(MailMessage message)
        {
            var accessor = new SmtpAccessor(_log, _host, _port, _username, _password);

            accessor.SendEmail(message);

            return new CallResult<Exception>()
            {
                Status = CallStatus.Success
            };
        }

        public MailMessage ComposeEmail(IEmailInfo model, CallSource callSource, long? by)
        {
            model.From = new MailAddress(GetFrom(model.Market), _fromDisplayName);

            var toAddressList = model.ToList;
            var ccAddressList = model.CcList;

            if (_isDebug)
            {
                toAddressList = new List<MailAddress>() {new MailAddress("support@dgtex.com", "Support")};
                ccAddressList = new List<MailAddress>();
            }

            var subject = model.Subject;
            if (callSource == CallSource.Service
                && (model.Market == MarketType.Amazon
                || model.Market == MarketType.AmazonEU
                || model.Market == MarketType.AmazonAU))
            {
                var isImportant = (subject ?? "").Contains("[Important]");
                if (!(subject ?? "").Contains("commercentric.com]"))
                {
                    if (!(subject ?? "").StartsWith("RE:", StringComparison.InvariantCultureIgnoreCase))
                        subject = (isImportant ? "[Important] " : "") + "Additional Information Required (Order: " + model.Tag + ")";
                }
            }
            subject = (_isDebug ? "[Test] " : "") + subject;


            var toCollection = new MailAddressCollection();
            toAddressList.ForEach(a => toCollection.Add(a));

            var ccCollection = new MailAddressCollection();
            if (ccAddressList != null && ccAddressList.Count > 0)
            {
                ccAddressList.ForEach(a => ccCollection.Add(a));
            }

            var mailMessage = new MailMessage(model.From, toCollection.First())
            {
                Subject = subject,
                Body = model.Body,
                IsBodyHtml = true,
            };

            if (by.HasValue)
                mailMessage.Headers.Add(EmailHelper.UserNameHeadersKey, by.ToString());

            toCollection.Remove(toCollection.First());
            foreach (var mailAddress in toCollection)
            {
                mailMessage.To.Add(mailAddress);
            }

            foreach (var mailAddress in ccCollection)
            {
                mailMessage.CC.Add(mailAddress);
            }

            foreach (var attachment in model.Attachments)
            {
                mailMessage.Attachments.Add(attachment);
            }

            return mailMessage;
        }


        public void AutoAnswerEmails(IDbFactory dbFactory,
            ITime time,
            CompanyDTO company)
        {
            AutoResponseAmazon(dbFactory, time, company);
            AutoResponseWalmart(dbFactory, time, company);
        }

        private void AutoResponseWalmart(IDbFactory dbFactory,
            ITime time,
            CompanyDTO company)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var emailsToAnswer = db.Emails.GetAllWithOrder(new EmailSearchFilter()
                {
                    ResponseStatus = (int)EmailResponseStatusFilterEnum.ResponseNeeded,
                })
                    .ToList()
                    .Where(e => e.ResponseStatus == (int)EmailResponseStatusEnum.None
                        && (e.Market == (int)MarketType.Walmart
                            || e.Market == (int)MarketType.WalmartCA
                            || !e.Market.HasValue)
                        && e.To != e.From)
                    .ToList();

                var subjectKey = "Please contact the customer to resolve an issue with";
                var bodyKey = "** THIS IS AN ESCALATION **";
                emailsToAnswer = emailsToAnswer.Where(e => (e.Market == (int)MarketType.Walmart
                                              || e.Market == (int)MarketType.WalmartCA
                                              || MarketHelper.GetMarketNameByEmailAddress(e.From).Market == MarketType.Walmart
                                              || MarketHelper.GetMarketNameByEmailAddress(e.From).Market == MarketType.WalmartCA)
                                              && e.EmailType != (int)IncomeEmailTypes.System
                                              && (e.Subject ?? "").StartsWith(subjectKey)
                                              //&& (e.Message ?? "").StartsWith(bodyKey)
                                              )
                                .ToList();

                emailsToAnswer = emailsToAnswer
                    .Where(e => (e.Message ?? "").IndexOf(bodyKey, StringComparison.InvariantCultureIgnoreCase) < 40)
                    .ToList();

                _log.Info("Response needed: " + emailsToAnswer.Count);

                foreach (var email in emailsToAnswer)
                {
                    //if (!String.IsNullOrEmpty(email.OrderIdString))
                    {
                        var order = db.Orders.GetByOrderIdAsDto(email.OrderIdString);

                        if (//order != null && 
                            email.ReceiveDate < time.GetAppNowTime().AddMinutes(-5))
                        {
                            _log.Info("Send autoresponse");
                            SendSystemEmailToAdmin("Send auto response, Id=" + email.Id,
                                "<div>To email: " + email.Subject + "</div><div>Receive Date: " + email.ReceiveDate);

                            var model = GetEmailInfoByType(db,
                                EmailTypes.AutoResponseWalmart,
                                company,
                                null,
                                order?.OrderId,
                                null,
                                email.Subject,
                                email.From);

                            var result = SendEmail(model, CallSource.Service);
                            if (result.IsSuccess)
                            {
                                var dbEmail = db.Emails.Get(email.Id);
                                dbEmail.ResponseStatus = (int)EmailResponseStatusEnum.Sent;
                                db.Commit();
                            }
                        }
                    }
                }
            }
        }

        private void AutoResponseAmazon(IDbFactory dbFactory,
            ITime time,
            CompanyDTO company)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var emailsToAnswer = db.Emails.GetAllWithOrder(new EmailSearchFilter()
                {
                    ResponseStatus = (int)EmailResponseStatusFilterEnum.ResponseNeeded,
                })
                    .ToList()
                    .Where(e => e.ResponseStatus == (int)EmailResponseStatusEnum.None
                        && (e.Market == (int)MarketType.AmazonEU
                            || e.Market == (int)MarketType.AmazonAU
                            || e.Market == (int)MarketType.Amazon
                            || !e.Market.HasValue)
                        && e.To != e.From)
                    .ToList();

                emailsToAnswer = emailsToAnswer.Where(e => (e.Market == (int)MarketType.Amazon
                                              || e.Market == (int)MarketType.AmazonEU
                                              || e.Market == (int)MarketType.AmazonAU
                                              || MarketHelper.GetMarketNameByEmailAddress(e.From).Market == MarketType.Amazon
                                              || MarketHelper.GetMarketNameByEmailAddress(e.From).Market == MarketType.AmazonEU
                                              || MarketHelper.GetMarketNameByEmailAddress(e.From).Market == MarketType.AmazonAU)
                                              && e.EmailType != (int)IncomeEmailTypes.System
                                              && !e.Subject.StartsWith("Your Immediate Response Required:"))
                                .ToList();

                _log.Info("Response needed: " + emailsToAnswer.Count);

                foreach (var email in emailsToAnswer)
                {
                    //if (!String.IsNullOrEmpty(email.OrderIdString))
                    {
                        var order = db.Orders.GetByOrderIdAsDto(email.OrderIdString);

                        if (//order != null && 
                            email.ReceiveDate < time.GetAppNowTime().AddHours(-23).AddMinutes(-30))
                        {
                            _log.Info("Send autoresponse");
                            SendSystemEmailToAdmin("Send auto response, Id=" + email.Id,
                                "<div>To email: " + email.Subject + "</div><div>Receive Date: " + email.ReceiveDate);

                            var model = GetEmailInfoByType(db,
                                EmailTypes.AutoResponseAmazon,
                                company,
                                null,
                                order?.OrderId,
                                null,
                                email.Subject,
                                email.From);

                            var result = SendEmail(model, CallSource.Service);
                            if (result.IsSuccess)
                            {
                                var dbEmail = db.Emails.Get(email.Id);
                                dbEmail.ResponseStatus = (int)EmailResponseStatusEnum.ResponsePromised;
                                db.Commit();
                            }
                        }
                    }
                }
            }
        }

        public void ProcessEmailActions(IDbFactory dbFactory, 
            ITime time,
            CompanyDTO company,
            ISystemActionService actionService)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var actionToProcess = actionService.GetUnprocessedByType(db, SystemActionType.SendEmail, null, null);
                foreach (var action in actionToProcess)
                {
                    var isSuccess = false;
                    var message = String.Empty;
                    SendEmailInput inputData = null;
                    try
                    {
                        inputData = SystemActionHelper.FromStr<SendEmailInput>(action.InputData);

                        var emailInfo = GetEmailInfoByType(db,
                            inputData.EmailType,
                            company,
                            null,
                            inputData.OrderId,
                            inputData.Args,
                            inputData.ReplyToSubject,
                            inputData.ReplyToEmail);

                        var result = SendEmail(emailInfo, CallSource.Service);
                        isSuccess = result.Status == CallStatus.Success;
                        message = result.Message;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("ProcessEmailActions, actionId=" + action.Id, ex);
                        isSuccess = false;
                        message = ex.Message;
                    }

                    var outputDate = SystemActionHelper.FromStr<SendEmailOutput>(action.OutputData);
                    if (outputDate == null)
                        outputDate = new SendEmailOutput();

                    var actionStatus = SystemActionStatus.None;
                    if (isSuccess)
                    {
                        outputDate.IsSuccess = true;
                        outputDate.SendDate = time.GetAppNowTime();
                        actionStatus = SystemActionStatus.Done;
                    }
                    else
                    {
                        outputDate.AttemptNumber++;
                        outputDate.Message = message;
                        if (outputDate.AttemptNumber > 3)
                            actionStatus = SystemActionStatus.Fail;
                        else
                            actionStatus = SystemActionStatus.None;
                    }

                    actionService.SetResult(db, action.Id,
                        actionStatus,
                        outputDate,
                        null);
                    db.Commit();

                    if (isSuccess)
                    {
                        var notifyType = EmailNotifyHelper.GetEmailNotifyFrom(inputData.EmailType);

                        db.OrderEmailNotifies.Add(new OrderEmailNotify()
                        {
                            OrderNumber = inputData.OrderId,
                            Type = (int)notifyType,
                            Reason = "",
                            CreateDate = time.GetUtcTime()
                        });
                        db.Commit();
                    }
                }
            }
        }

        public IEmailInfo GetEmailInfoByType(IUnitOfWork db,
            EmailTypes emailType,
            CompanyDTO company,
            string byName,
            string orderId,
            Dictionary<string, string> args,
            string replyToSubject,
            string replyToEmail)
        {
            IEmailInfo info = null;
            DTOOrder order;
            IList<OrderShippingInfoDTO> labels = new List<OrderShippingInfoDTO>();
            OrderShippingInfoDTO label = null;
            string buyerEmail;

            //Trying to get shipping info
            string labelType = (args != null && args.ContainsKey("LabelType")) ? args["LabelType"] : LabelFromType.Batch.ToString();
            var shippingInfoId = (args != null && args.ContainsKey("ShippingOrderId")) ? StringHelper.ToInt(args["ShippingOrderId"]) : null;
            
            if (shippingInfoId != null)
            {
                if (labelType == LabelFromType.Batch.ToString())
                {
                    label = db.OrderShippingInfos.GetAllAsDto().FirstOrDefault(sh => sh.Id == shippingInfoId);
                }
                else
                {
                    label = db.MailLabelInfos.GetAllAsDto().FirstOrDefault(m => m.Id == shippingInfoId);
                }
            }


            switch (emailType)
            {
                case EmailTypes.RequestFeedback:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle:false, includeSourceItems: false);
                    info = new FeedbackEmailInfo(_addressService,
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail,
                        order.Items);
                    break;

                case EmailTypes.SignConfirmationRequest:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);

                    info = new SignConfirmationRequestEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.Items,
                        label != null ? ShippingUtils.GetShippingType(label.ShippingMethodId) : ShippingTypeCode.None,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.AcceptOrderCancellationToBuyer:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    buyerEmail = String.IsNullOrEmpty(replyToEmail) ? order.BuyerEmail : replyToEmail;
                    
                    info = new AcceptOrderCancellationToBuyerEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        buyerEmail,
                        replyToSubject);
                    break;

                case EmailTypes.RejectOrderCancellationToBuyer:
                    order = db.Orders.GetByOrderIdAsDto(orderId);

                    info = new RejectOrderCancellationToBuyerEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail,
                        replyToSubject,
                        replyToEmail);
                    break;

                case EmailTypes.AcceptOrderCancellationToSeller:
                    order = db.Orders.GetByOrderIdAsDto(orderId);

                    info = new AcceptOrderCancellationToSellerEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.MarketplaceId,
                        args.ContainsKey("SourceMessage") ? args["SourceMessage"] : "",
                        company.SellerName,
                        company.SellerEmail);
                    break;

                case EmailTypes.AcceptReturnRequest:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new AcceptReturnRequestEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        true,
                        args.ContainsKey("Size") ? args["Size"] : "-",
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.AcceptRemoveSignConfirmation:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    info = new AcceptRemoveSignConfirmationEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.Items,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.RejectRemoveSignConfirmation:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new RejectRemoveSignConfirmationEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.AddressVerify:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new UnverifiedAddressEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market, 
                        order.GetAddressDto(),
                        order.BuyerName,
                        order.BuyerEmail,
                        order.EarliestShipDate ?? (order.OrderDate ?? DateTime.Today).AddDays(1));
                    break;

                case EmailTypes.AddressNotServedByUSPS:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new AddressNotServedByUSPSEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market, 
                        order.GetAddressDto(),
                        order.BuyerName,
                        order.BuyerEmail,
                        order.EarliestShipDate ?? (order.OrderDate ?? DateTime.Today).AddDays(1));
                    break;
                    
                case EmailTypes.AddressChanged:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new AddressChangedEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.AddressNotChanged:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new AddressNotChangedEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    ((AddressNotChangedEmailInfo) info).ReplyToSubject = replyToSubject;
                    break;

                case EmailTypes.IncompleteName:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    info = new IncompleteNameEmailInfo(_addressService,
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market, 
                        order.GetAddressDto(),
                        order.Items,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.PhoneMissing:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    info = new PhoneMissingEmailInfo(_addressService,
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market, 
                        order.Items,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.LostPackage:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    labels = db.Labels.GetByOrderIdAsDto(order.Id).ToList();

                    if (labels.Any())
                    {
                        label = labels.OrderByDescending(sh => sh.LabelPurchaseDate).FirstOrDefault(i => i.IsActive);
                    }
                    info = new LostPackageEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        label != null ? label.ActualDeliveryDate : null,
                        label != null ? label.TrackingStateEvent : null,
                        label != null ? label.ShippingMethod.CarrierName : null,
                        label != null ? label.TrackingNumber : null,
                        label != null ? label.ToAddress : order.GetAddressDto(),
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.LostPackage2:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    
                    info = new LostPackage2EmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.UndeliverableInquiry:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    labels = db.Labels.GetByOrderIdAsDto(order.Id).ToList();
                    label = null;
                    if (labels.Any())
                    {
                        label = labels.OrderByDescending(sh => sh.LabelPurchaseDate).FirstOrDefault(i => i.IsActive);
                    }
                    info = new UndeliverableInquiryEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        label != null ? label.ShippingMethod.CarrierName : null,
                        label != null ? label.TrackingNumber : null,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.UndeliverableAsAddressed:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    labels = db.Labels.GetByOrderIdAsDto(order.Id).ToList();
                    if (labels.Any())
                    {
                        label = labels.OrderByDescending(sh => sh.LabelPurchaseDate).FirstOrDefault(i => i.IsActive);
                    }
                    info = new UndeliverableAsAddressedRequestEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market, 
                        order.Items,
                        label != null && label.ShippingMethod != null ? label.ShippingMethod.CarrierName : null,
                        label != null ? label.TrackingNumber : null,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.AlreadyShipped:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    labels = db.Labels.GetByOrderIdAsDto(order.Id).ToList();
                    if (labels.Any())
                    {
                        label = labels.OrderByDescending(sh => sh.LabelPurchaseDate).FirstOrDefault(i => i.IsActive);
                    }
                    info = new AlreadyShippedEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.Items,
                        label != null && label.ShippingMethod != null ? label.ShippingMethod.CarrierName : null,
                        label != null ? label.TrackingNumber : null,
                        label != null ? label.ToAddress : order.GetAddressDto(),
                        order.BuyerName,
                        order.BuyerEmail);
                    break;


                case EmailTypes.GiftReceipt:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    
                    info = new GiftEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;


                case EmailTypes.Oversold:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    var firstStyleId = order.Items.FirstOrDefault()?.StyleId;
                    var subLicense = ""; 
                    if (firstStyleId.HasValue)
                    {
                        subLicense = db.FeatureValues.GetValueByStyleAndFeatureId(firstStyleId.Value,
                                StyleFeatureHelper.SUB_LICENSE1)?.Value;
                    }
                    info = new OversoldEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.Items,
                        subLicense,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.NoticeLeft:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);
                    if (label == null)
                        label = db.Labels.GetByOrderIdAsDto(order.Id).OrderByDescending(sh => sh.LabelPurchaseDate).FirstOrDefault(i => i.IsActive);

                    info = new NoticeLeftEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market, 
                        order.Items,
                        label != null ? ShippingUtils.GetShippingType(label.ShippingMethodId) : ShippingTypeCode.None,
                        label != null ? label.ShippingMethod.CarrierName : null,
                        label != null ? label.TrackingStateDate : null,
                        label != null ? label.TrackingNumber : null,
                        label != null ? label.ToAddress : order.GetAddressDto(),
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.ExchangeInstructions:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new ExchangeInstructionsEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.MarketplaceId,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.ReturnInstructions:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new ReturnInstructionsEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.MarketplaceId,
                        order.BuyerName,
                        order.BuyerEmail,
                        order.FinalShippingCountry);
                    break;

                case EmailTypes.ReturnPeriodExpired:
                    order = db.Orders.GetByOrderIdAsDto(orderId);

                    info = new ReturnPeriodExpiredEmailInfo(_addressService,
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.DamagedItem:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    var isOnePieces = IsOnePieces(db, order.OrderId);

                    info = new DamagedItemEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        isOnePieces,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.NotOurs:
                    order = db.Orders.GetByOrderIdAsDto(orderId);

                    info = new NotOursEmailInfo(_addressService,
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.ReturnWrongDamagedItem:
                    order = db.Orders.GetByOrderIdAsDto(orderId);

                    info = new ReturnWrongDamagedItemEmailInfo(_addressService, 
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                case EmailTypes.AutoResponseAmazon:
                    order = db.Orders.GetByOrderIdAsDto(orderId);

                    info = new AutoResonseAmazonEmailInfo(_addressService, 
                        order?.BuyerName,
                        replyToSubject,
                        replyToEmail);
                    break;

                case EmailTypes.AutoResponseWalmart:
                    order = db.ItemOrderMappings.GetOrderWithItems(null, orderId, unmaskReferenceStyle: false, includeSourceItems: false);

                    info = new AutoResonseWalmartEmailInfo(_addressService,
                        order?.OrderId,
                        order?.Items,
                        (MarketType)(order?.Market ?? (int)MarketType.None),
                        order?.BuyerName,
                        order?.BuyerEmail,
                        replyToSubject,
                        replyToEmail);
                    break;

                case EmailTypes.RawEmail:
                    var subject = args.ContainsKey("Subject") ? args["Subject"] : "";
                    var body = args.ContainsKey("Body") ? args["Body"] : "";
                    var toName = args.ContainsKey("ToName") ? args["ToName"] : "";
                    var toEmail = args.ContainsKey("ToEmail") ? args["ToEmail"] : "";
                    var ccName = args.ContainsKey("CcName") ? args["CcName"] : "";
                    var ccEmail = args.ContainsKey("CcEmail") ? args["CcEmail"] : "";
                    info = new RawEmailInfo(_addressService,
                        subject,
                        body,
                        new string[] { },
                        toName,
                        toEmail,
                        ccName,
                        ccEmail,
                        null,
                        null);
                    break;

                case EmailTypes.CustomEmail:
                    order = db.Orders.GetByOrderIdAsDto(orderId);
                    info = new CustomEmailInfo(_addressService,
                        byName,
                        order.CustomerOrderId,
                        (MarketType)order.Market,
                        order.BuyerName,
                        order.BuyerEmail);
                    break;

                default:
                    throw new NotSupportedException("Not supported email type");
                    break;
            }

            return info;
        }

        private bool IsOnePieces(IUnitOfWork db,
            string orderNumber)
        {
            var items = db.OrderItems.GetByOrderIdAsDto(orderNumber);
            var onePieces = true;

            long? styleId = null;
            if (items.Count() == 1)
            {
                styleId = items.FirstOrDefault().StyleId;
            }
            else
            {
                var returnRequest = db.ReturnRequests
                    .GetAll()
                    .OrderByDescending(r => r.CreateDate)
                    .FirstOrDefault(r => r.OrderNumber == orderNumber);

                if (returnRequest != null)
                {
                    styleId = returnRequest.StyleId;
                }
            }

            if (styleId.HasValue)
            {
                var itemType = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(styleId.Value, StyleFeatureHelper.ITEMSTYLE);
                if (itemType != null)
                {
                    onePieces = !StringHelper.ContrainOneOfKeywords((itemType.Value ?? ""), new string[]
                                {
                                    "2pc",
                                    "3pc",
                                    "4pc"
                                });
                }
            }

            return onePieces;
        }

        public string GetMainNotifierStr()
        {
            return EmailHelper.RafiEmail;
        }

        public string GetMainCoWorkerNotifierStr()
        {
            return EmailHelper.RaananEmail;
        }

        public string GetWarehouseNotifierStr()
        {
            return "";
        }

        public string GetSupportNotifierStr()
        {
            return EmailHelper.SupportDgtexEmail;
        }
    }
}
