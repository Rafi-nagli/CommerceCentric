using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Script.Serialization;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Emails;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Orders;
using Newtonsoft.Json;

namespace Amazon.Web.ViewModels.Emails
{
    public class EmailViewModel
    {
        public long Id { get; set; }

        public string OrderNumber { get; set; }
        public int EmailType { get; set; }
        public int FolderType { get; set; }
        public bool IsReviewed { get; set; }

        public string ToEmail { get; set; }
        public string ToName { get; set; }

        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public bool IsEscalated { get; set; }



        public string Subject { get; set; }
        [ScriptIgnore]
        [JsonIgnore]
        public string Body { get; set; }

        public int ResponseStatus { get; set; }
        public string AnswerMessageID { get; set; }

        public int? DueHours
        {
            get
            {
                if (ResponseNeeded)
                {
                    if (EmailType != (int)IncomeEmailTypes.System
                        && (OrderMarket == (int) MarketType.Amazon
                        || OrderMarket == (int) MarketType.AmazonEU
                        || OrderMarket == (int) MarketType.AmazonAU
                        || EmailMarket == (int) MarketType.Amazon
                        || EmailMarket == (int) MarketType.AmazonEU
                        || EmailMarket == (int) MarketType.AmazonAU))
                    {
                        var hours = 24 - (int) Math.Ceiling((DateTime.Now - ReceiveDate).TotalHours);
                        return hours;
                    }
                }
                return null;
            }
        }

        public bool HasDueDate
        {
            get { return DueHours.HasValue; }
        }

        public bool CanReply
        {
            get
            {
                return !((Subject ?? "").StartsWith("Can you answer this question about")
                            && StringHelper.ContainsNoCase(FromEmail, "seller-answers@amazon.com"));
            }
        }


        #region Additional Info
        public DateTime ReceiveDate { get; set; }

        public long? OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }
        public int? OrderMarket { get; set; }
        public string OrderMarketplaceId { get; set; }

        public bool IsSent
        {
            get { return FolderType == (int) EmailFolders.Sent; }
        }

        public bool IsSystem
        {
            get { return EmailType == (int) IncomeEmailTypes.System; }
        }

        public bool HasAttachments { get; set; }

        public bool ResponseNeeded
        {
            get
            {
                return EmailHelper.ResponseNeeded(AnswerMessageID, EmailType, ResponseStatus, FolderType);
            }
        }

        public bool ResponsePromised
        {
            get { return ResponseStatus == (int)EmailResponseStatusEnum.ResponsePromised; }
        }

        public bool ResponseDismissed
        {
            get { return ResponseStatus == (int) EmailResponseStatusEnum.NoResponseNeeded; }
        }

        #endregion

        public int EmailMarket
        {
            get
            {
                if (OrderMarket.HasValue)
                {
                    return OrderMarket.Value;
                }
                else
                {
                    var market = MarketHelper.GetMarketNameByEmailAddress(FolderType == (int)EmailFolders.Inbox ? FromEmail : ToEmail);
                    return (int)market.Market;
                }
            }
        }

        public string EmailMarketShortName
        {
            get
            {
                if (OrderMarket.HasValue)
                {
                    return MarketHelper.GetShortName(OrderMarket.Value, OrderMarketplaceId, true);
                }
                else
                {
                    var market = MarketHelper.GetMarketNameByEmailAddress(FolderType == (int) EmailFolders.Inbox ? FromEmail : ToEmail);
                    return MarketHelper.GetShortName((int)market.Market, market.MarketplaceId, true);
                }
            }
        }

        public string DisplayName
        {
            get { return IsSent ? ToEmail : FromName; }
        }

        public string DisplayEmail
        {
            get { return IsSent ? ToEmail : FromEmail; }
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

        public string ShortBody
        {
            get
            {
                if (String.IsNullOrEmpty(Body))
                    return String.Empty;

                return EmailHelper.ExtractShortMessageBody(Body, 255, true);
            }
        }

        public string ReceiveDateString
        {
            get { return DateHelper.ToDateTimeString(ReceiveDate); }
        }

        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderNumber); }
        }

        public string ViewEmailUrl
        {
            get { return UrlHelper.GetViewEmailUrl(Id, OrderNumber); }
        }

        public string ReplyEmailUrl
        {
            get { return UrlHelper.GetReplyToUrl(Id, OrderNumber); }
        }
        
        public LabelViewModel Label { get; set; }
        
        public static IEnumerable<EmailViewModel> GetAll(IUnitOfWork db, 
            CompanyDTO company,
            EmailFilterViewModel filter)
          {
            return db.Emails.GetAllWithOrder(filter.GetModel()).Select(e =>
                new EmailViewModel()
                {
                    Id = e.Id,
                    Subject = e.Subject,
                    FromEmail = e.From,
                    ToEmail = e.To,
                    ReceiveDate = e.ReceiveDate,
                    FolderType = e.FolderType,
                    EmailType = e.EmailType,
                    IsReviewed = e.IsReviewed,

                    FromName = e.FolderType == (int)EmailFolders.Sent ? company.CompanyName : e.BuyerName,
                    ToName = e.FolderType == (int)EmailFolders.Sent ? e.BuyerName : company.CompanyName,
                    OrderNumber = e.CustomerOrderId,
                    OrderDate = e.OrderDate,
                    OrderMarket = e.Market,
                    OrderMarketplaceId = e.MarketplaceId,

                    AnswerMessageID = e.AnswerMessageID,
                    ResponseStatus = e.ResponseStatus,
                    HasAttachments = e.HasAttachments,
                    IsEscalated = e.IsEscalated,

                    Label = new LabelViewModel() {
                        Carrier = e.Label.Carrier,
                        TrackingNumber = e.Label.TrackingNumber,
                        PurchaseDate = e.Label.LabelPurchaseDate,
                        ActualDeliveryDate = e.Label.ActualDeliveryDate,
                        ShippingCountry = e.Label.ShippingCountry,
                        TrackingStatusSource = e.Label.TrackingStateSource,
                        TrackingStateDate = e.Label.TrackingStateDate,
                        EstDeliveryDate = e.Label.EstimatedDeliveryDate,
                        DeliveredStatus = e.Label.DeliveredStatus
                    }
                });
        }

        public static void SetResponseStatus(IUnitOfWork db, 
            long emailId,
            EmailResponseStatusEnum newResponseStatus,
            IOrderHistoryService orderHistorySerivce,
            string orderNumber,
            DateTime when,
            long? by)
        {
            var email = db.Emails.Get(emailId);

            var fromResponseStatus = email.ResponseStatus;
            email.ResponseStatus = (int)newResponseStatus;
            db.Commit();

            DTOOrder order = null;
            if (!String.IsNullOrEmpty(orderNumber))
            {
                order = db.Orders.GetByOrderIdAsDto(orderNumber);
            }

            orderHistorySerivce.AddRecord(order?.Id ?? 0,
                OrderHistoryHelper.EmailStatusChangedKey,
                fromResponseStatus,
                email.Id.ToString(),
                email.ResponseStatus,
                null,
                by);
        }

        public static void SetReviewedStatus(IUnitOfWork db,
            long emailId,
            bool newReviewedStatus,
            IOrderHistoryService orderHistorySerivce,
            string orderNumber,
            DateTime when,
            long? by)
        {
            var email = db.Emails.Get(emailId);

            var fromReviwedStatus = email.IsReviewed;
            email.IsReviewed = newReviewedStatus;
            db.Commit();

            DTOOrder order = null;
            if (!String.IsNullOrEmpty(orderNumber))
            {
                order = db.Orders.GetByOrderIdAsDto(orderNumber);
            }

            orderHistorySerivce.AddRecord(order?.Id ?? 0,
                OrderHistoryHelper.EmailReviewedStatusChangedKey,
                fromReviwedStatus,
                email.Id.ToString(),
                email.IsReviewed,
                null,
                by);
        }

        public static IList<MessageString> AssignToOrder(IUnitOfWork db, 
            ILogService log,
            long emailId, 
            string orderNumber,
            DateTime when,
            long? by)
        {
            var messages = new List<MessageString>();

            var email = db.Emails.GetAll().FirstOrDefault(e => e.Id == emailId);
            var fromDate = when.AddDays(-30);
            var allNotAssignedEmailIds = (from e in db.Emails.GetAll()
                                               join eToO in db.EmailToOrders.GetAll() on e.Id equals eToO.EmailId into assigned
                                               from eToO in assigned.DefaultIfEmpty()
                                               where eToO == null
                                                && e.ReceiveDate > fromDate
                                                && e.From == email.From
                                               select e.Id).ToList();
            allNotAssignedEmailIds.Add(emailId);
            allNotAssignedEmailIds = allNotAssignedEmailIds.Distinct().ToList();
            
            orderNumber = OrderHelper.RemoveOrderNumberFormat(orderNumber);
            var order = db.Orders.GetByCustomerOrderNumber(orderNumber);

            if (order != null)
            {
                var bindings = db.EmailToOrders.GetAll().Where(e => e.EmailId == emailId).ToList();
                foreach (var binding in bindings)
                {
                    log.Info("Remove, orderId=" + binding.OrderId + ", emailId=" + binding.EmailId);
                    db.EmailToOrders.Remove(binding);
                }
                var orderIds = new List<string>() { order.CustomerOrderId}.Distinct(); //order.AmazonIdentifier,

                foreach (var orderId in orderIds)
                {
                    foreach (var toAssignEmailId in allNotAssignedEmailIds)
                    {
                        db.EmailToOrders.Add(new EmailToOrder()
                        {
                            EmailId = toAssignEmailId,
                            OrderId = orderId,
                            CreateDate = when,
                            CreatedBy = by,
                        });
                    }
                }
                db.Commit();
            }
            else
            {
                messages.Add(MessageString.Error("Unable to find a matching order"));
            }

            return messages;
        }

        public static void Escalate(IUnitOfWork db, ITime time, long emailId, string orderNumber, bool escalated)
        {
            if (orderNumber == null)
            {
                throw new ArgumentException("Value cann not be null", "orderNumber");
            }
            var order = db.Orders.GetFiltered(x => x.CustomerOrderId == orderNumber).FirstOrDefault();
            if (order == null)
            {
                throw new ArgumentException("Value cann not be null", "orderNumber");
            }
            var oldNotifies = db.OrderNotifies.GetFiltered(x => x.OrderId == order.Id && x.Type == (int)OrderNotifyType.Escalated).ToList();
            foreach (var o in oldNotifies)
            {
                db.OrderNotifies.Remove(o);
            }
            if (escalated)
            {
                db.OrderNotifies.Add(new Core.Entities.Orders.OrderNotify()
                {
                    OrderId = order.Id,
                    Type = (int)OrderNotifyType.Escalated,
                    CreateDate = time.GetAppNowTime(),
                    Status = 1
                });
            }
            db.Commit();
        }
    }
}