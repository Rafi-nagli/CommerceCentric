using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
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
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Emails;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Orders;

namespace Amazon.Web.ViewModels.Emails
{
    public class ViewEmailViewModel
    {
        public long Id { get; set; }

        public string OrderNumber { get; set; }
        public EmailTypes EmailType { get; set; }

        public int Type { get; set; }
        public int FolderType { get; set; }
        public int ResponseStatus { get; set; }
        public string AnswerMessageID { get; set; }
        public long? ReplyToEmailId { get; set; }
        public bool IsReviewed { get; set; }

        [Display(Name = "From")]
        public string FromEmail { get; set; }
        public string FromName { get; set; }

        
        public string ToName { get; set; }
        [Display(Name = "To")]
        public string ToEmail { get; set; }
        public bool IsEscalated { get; set; }


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
            get { return ResponseStatus == (int) EmailResponseStatusEnum.ResponsePromised; }
        }

        public bool ResponseDismissed
        {
            get { return ResponseStatus == (int)EmailResponseStatusEnum.NoResponseNeeded; }
        }

        public bool CanReply
        {
            get
            {
                return !((Subject ?? "").StartsWith("Can you answer this question about")
                            && (StringHelper.ContainsNoCase(FromEmail, "seller-answers@amazon.com") || StringHelper.ContainsNoCase(FromEmail, "seller-answers@amazon.co.uk")));
            }
        }

        public bool CanAssign
        {
            get
            {
                return !((Subject ?? "").StartsWith("Can you answer this question about")
                            && (StringHelper.ContainsNoCase(FromEmail, "seller-answers@amazon.com") || StringHelper.ContainsNoCase(FromEmail, "seller-answers@amazon.co.uk")));
            }
        }


        public IList<EmailAttachmentViewModel> Attachments { get; set; }

        public string Tag
        {
            get { return OrderNumber; }
        }

        public int EmailMarket
        {
            get
            {
                if (Market == MarketType.None)
                {
                    var marketInfo = MarketHelper.GetMarketNameByEmailAddress(FolderType == (int)EmailFolders.Inbox ? FromEmail : ToEmail);
                    return (int)marketInfo.Market;
                }
                return (int)Market;
            }
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
                    return new List<MailAddress>()
                    {
                        new MailAddress(ToEmail, ToName ?? String.Empty)
                    };
                }
                return new List<MailAddress>();
            }
        }

        public List<MailAddress> CcList { get; set; }


        public string Subject { get; set; }
        public string Body { get; set; }

        
        
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


        public static ViewEmailViewModel BuildFrom(IUnitOfWork db,
            ILogService log,
            CompanyDTO company,
            long emailId, 
            string orderNumber)
        {
            var email = db.Emails.Get(emailId);
            var attachments = db.EmailAttachments.GetAllAsDto()
                .Where(e => e.EmailId == emailId)
                .OrderBy(e => e.Id)
                .ToList();

            var model = new ViewEmailViewModel();
            model.Id = email.Id;
            model.FromEmail = email.From;

            model.AnswerMessageID = email.AnswerMessageID;
            model.ResponseStatus = email.ResponseStatus;
            model.Type = email.Type;
            model.FolderType = email.FolderType;
            model.IsReviewed = email.IsReviewed;

            model.ToEmail = email.To;

            model.ReceiveDate = email.ReceiveDate;
            model.Body = email.Message;
            model.Subject = email.Subject;
            model.SubjectHtml = email.Subject;

            model.OrderNumber = orderNumber;

            var preparedOrderNumber = OrderHelper.RemoveOrderNumberFormat(orderNumber);
            var order = db.Orders.GetByOrderIdAsDto(preparedOrderNumber);
            if (order != null)
            {
                model.FromName = model.IsSent ? company.CompanyName : order.BuyerName;
                model.Market = (MarketType) order.Market;
                model.SubjectHtml = BuildHtmlSubject(email.Subject, new string[] {order.CustomerOrderId, order.OrderId},
                    model.Market, model.OrderUrl);
            }

            model.Attachments = attachments.Select(a => new EmailAttachmentViewModel(a)).ToList();
            var orderIds = db.Orders.GetFiltered(x => x.CustomerOrderId == orderNumber).Select(x => x.Id).Distinct();

            var notifies = db.OrderNotifies.GetFiltered(x => orderIds.Contains(x.OrderId) && x.Type == (int)OrderNotifyType.Escalated);

            model.IsEscalated = notifies.Any();

            return model;
        }

        private static string BuildHtmlSubject(string subject, string[] orderStrings, MarketType market, string orderUrl)
        {
            var html = subject;

            var matches = Regex.Matches(subject, EmailParserHelper.ID_UNIVERSAL_REGEX, RegexOptions.IgnoreCase);

            foreach (var orderNumber in orderStrings.Select(o => (o ?? "").Replace("-", "")).Distinct())
            {
                foreach (Match match in matches)
                {
                    if (match.Value.Replace("-", "").Replace(" ", "") == orderNumber)
                    {
                        html = html.Replace(match.Value.Trim(),
                            String.Format("<a target='_blank' href='{0}'>" + match.Value.Trim() + "</a>", orderUrl));
                        break;
                    }
                }
            }

            return html;
        }
    }
}