using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db.Emails;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Emails;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Models.Search;
using Amazon.DTO;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories.Emails
{
    public class EmailRepository : Repository<Email>, IEmailRepository
    {
        public EmailRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public int GetUnansweredEmailCount()
        {
            var unansweredQuery = from e in unitOfWork.GetSet<Email>()
                where e.AnswerMessageID == null
                      && e.ResponseStatus == (int) EmailResponseStatusEnum.None
                      && e.FolderType == (int) EmailFolders.Inbox
                      && (e.Type != (int)IncomeEmailTypes.ReturnRequest
                            && e.Type != (int)IncomeEmailTypes.AZClaim
                            && e.Type != (int)IncomeEmailTypes.Test
                            && e.Type != (int)IncomeEmailTypes.DhlInvoice
                            && e.Type != (int)IncomeEmailTypes.SystemNotification
                            && e.Type != (int)IncomeEmailTypes.SystemAutoCopy)
                      && e.ReceiveDate > new DateTime(2017, 3, 15)
                select e;

            return unansweredQuery.Count();
        }

        public IQueryable<EmailOrderDTO> GetAllByOrderId(string orderId)
        {
            var query = from m in GetAll()
                join mTo in unitOfWork.GetSet<EmailToOrder>() on m.Id equals mTo.EmailId
                where mTo.OrderId == orderId
                    && m.Type != (int)IncomeEmailTypes.ReturnRequest
                    && m.Type != (int)IncomeEmailTypes.AZClaim
                    && m.Type != (int)IncomeEmailTypes.Test
                    && m.Type != (int)IncomeEmailTypes.DhlInvoice
                    && m.Type != (int)IncomeEmailTypes.SystemNotification
                select new EmailOrderDTO()
                {
                    Id = m.Id,
                    To = m.To,
                    From = m.From,
                    Subject = m.Subject,
                    Message = m.Message,
                    FolderType = m.FolderType,
                    ReceiveDate = m.ReceiveDate,
                    AnswerMessageID = m.AnswerMessageID,
                    ResponseStatus = m.ResponseStatus,
                    MessageID = m.MessageID,
                    UID = m.UID,
                    CreateDate = m.CreateDate,
                };

            return query;
        }

        public IQueryable<EmailOrderDTO> GetAllWithOrder(EmailSearchFilter filters)
        {
            var escalatedOrderIds = unitOfWork.OrderNotifies.GetFiltered(x => x.Type == (int)OrderNotifyType.Escalated).Select(x => x.OrderId);
            var escalatedCustomerOrdersIds = unitOfWork.Orders.GetFiltered(x => escalatedOrderIds.Contains(x.Id)).Select(x => x.CustomerOrderId);
            var query = from m in unitOfWork.GetSet<ViewEmails>()                        
                        join e in escalatedCustomerOrdersIds on m.CustomerOrderId equals e
                        into joinedEscalated
                        from je in joinedEscalated.DefaultIfEmpty()
                        where m.Type != (int)IncomeEmailTypes.ReturnRequest
                    && m.Type != (int)IncomeEmailTypes.AZClaim
                    && m.Type != (int)IncomeEmailTypes.Test
                    && m.Type != (int)IncomeEmailTypes.DhlInvoice
                    && m.Type != (int)IncomeEmailTypes.SystemNotification
                select new EmailOrderDTO()
                {
                    Id = m.Id,
                    To = m.To,
                    From = m.From,
                    Subject = m.Subject,
                    Message = m.Message,
                    ReceiveDate = m.ReceiveDate,
                    MessageID = m.MessageID,
                    UID = m.UID,
                    CreateDate = m.CreateDate,
                    FolderType = m.FolderType,
                    EmailType = m.Type,
                    IsReviewed = m.IsReviewed,

                    ResponseStatus = m.ResponseStatus,
                    AnswerMessageID = m.AnswerMessageID,

                    Market = m.Market,
                    MarketplaceId = m.MarketplaceId,
                    OrderIdString = m.AmazonIdentifier,
                    CustomerOrderId = m.CustomerOrderId,
                    //OrderId = o.Id, //NOTE: issue with declimator
                    OrderDate = m.OrderDate,
                    
                    OrderShipDate = m.SourceShippedDate,

                    BuyerName = m.BuyerName,

                    HasAttachments = m.HasAttachments,

                    Label = new LabelDTO() {
                        Carrier = m.CarrierName,
                        TrackingNumber = m.TrackingNumber,
                        LabelPurchaseDate = m.LabelPurchaseDate,
                        ActualDeliveryDate = m.ActualDeliveryDate,
                        ShippingCountry = m.ShippingCountry,

                        TrackingStateSource = m.TrackingStateSource,
                        TrackingStateDate = m.TrackingStateDate,
                        DeliveredStatus = m.DeliveredStatus,
                        ShippingDate = m.ShippingDate,
                        EstimatedDeliveryDate = m.EstimatedDeliveryDate
                    },
                    IsEscalated = je!=null
                };
            var q1 = query.Where(x => x.OrderIdString != null);
            var q2 = query.Where(x => x.CustomerOrderId != null);

            if (!String.IsNullOrEmpty(filters.OrderId))
            {
                var digitsOnlyNumber = new string(filters.OrderId.Where(char.IsDigit).ToArray());
                query = query.Where(o => o.OrderIdString == filters.OrderId
                    || o.CustomerOrderId == filters.OrderId
                    || o.OrderIdString == digitsOnlyNumber
                    || o.CustomerOrderId == digitsOnlyNumber);
            }

            if (filters.ResponseStatus == (int)EmailResponseStatusFilterEnum.ResponseNeeded)
            {
                query = query.Where(o => ((o.AnswerMessageID == null
                                            && o.ResponseStatus == (int)EmailResponseStatusEnum.None)
                                           || o.ResponseStatus == (int)EmailResponseStatusEnum.ResponsePromised)
                                         && o.FolderType == (int) EmailFolders.Inbox
                                         && o.EmailType != (int)IncomeEmailTypes.SystemAutoCopy
                                         && o.ReceiveDate > new DateTime(2016, 12, 13) //TEMP:
                                         );
            }

            if (filters.ResponseStatus == (int)EmailResponseStatusFilterEnum.Escalated)
            {
                query = query.Where(o => o.CustomerOrderId != null && o.IsEscalated);
            }

            //NOTE: recent dismissed
            if (filters.ResponseStatus == (int) EmailResponseStatusFilterEnum.ResponseNeededDismissed)
            {
                var from = DateHelper.GetAppNowTime().AddDays(-4);
                query = query.Where(o => o.ResponseStatus == (int) EmailResponseStatusEnum.NoResponseNeeded
                                         && o.ReceiveDate >= from);
            }

            if (filters.Market.HasValue)
            {
                if (filters.Market == (int) MarketType.Walmart
                    || filters.Market == (int)MarketType.WalmartCA)
                {
                    query = query.Where(o => o.Market == filters.Market.Value
                                             || o.To == EmailHelper.WalmartSupportEmail);
                }
                else
                {
                    query = query.Where(o => o.Market == filters.Market.Value);
                }
            }

            if (!String.IsNullOrEmpty(filters.BuyerName))
                query = query.Where(o => o.BuyerName.Contains(filters.BuyerName));
            if (filters.From.HasValue)
                query = query.Where(o => o.ReceiveDate >= filters.From.Value);
            if (filters.To.HasValue)
                query = query.Where(o => o.ReceiveDate <= filters.To.Value);
            if (filters.OnlyIncoming)
                query = query.Where(o => o.EmailType != (int)IncomeEmailTypes.SystemAutoCopy);
            if (!filters.IncludeSystem)
                query = query.Where(o => o.EmailType != (int)IncomeEmailTypes.System);

            return query;
        }


        public EmailDTO GetByUid(uint uid, EmailFolders folder)
        {
            return AsDto(GetFiltered(e => e.UID == uid && e.FolderType == (int)folder)).FirstOrDefault();
        }

        public EmailDTO GetByMessageID(string messageId)
        {
            return AsDto(GetFiltered(e => e.MessageID == messageId)).FirstOrDefault();
        }

        public void UpdateUID(long emailId, uint newUID)
        {
            var item = GetLocalOrAttach(e => e.Id == emailId, () => new Email() { Id = emailId });
            item.UID = newUID;

            unitOfWork.Commit();
        }

        public void Insert(EmailDTO email)
        {
            var item = new Email()
            {
                To = email.To,
                From = email.From,
                CopyTo = email.CopyTo,
                BCopyTo = email.BCopyTo,
                Subject = email.Subject,
                Message = email.Message,
                ReceiveDate = email.ReceiveDate,
                MessageID = email.MessageID,
                UID = email.UID,
                Type = email.Type,
                FolderType = email.FolderType,
                CreateDate = email.CreateDate 
            };
            Add(item);
            unitOfWork.Commit();

            email.Id = item.Id;
        }

        private IQueryable<EmailDTO> AsDto(IQueryable<Email> query)
        {
            return query.Select(e => new EmailDTO()
            {
                Id = e.Id,
                To = e.To,
                From = e.From,
                CopyTo = e.CopyTo,
                BCopyTo = e.BCopyTo,
                Subject = e.Subject,
                Message = e.Message,
                ReceiveDate = e.ReceiveDate,
                MessageID = e.MessageID,
                UID = e.UID,
                CreateDate = e.CreateDate
            });
        }
    }
}
