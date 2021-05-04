using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Emails;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class AddMatchIdsEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;

        public AddMatchIdsEmailRule(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Status != EmailMatchingResultStatus.New)
                return;

            var fromAddress = result.Email.From;
            var toAddress = result.Email.To;

            var orderIdList = new List<string>();

            var marketFromInfo = MarketHelper.GetMarketNameByEmailAddress(fromAddress);
            var marketToInfo = MarketHelper.GetMarketNameByEmailAddress(toAddress);

            //At first process only specified marketplaces
            if (marketFromInfo.Market == MarketType.eBay
                || marketToInfo.Market == MarketType.eBay)
            {
                if (!orderIdList.Any())
                    orderIdList.AddRange(EmailParserHelper.GetEBayAssociatedOrderIds(result.Email.Message));
            }

            if (marketFromInfo.Market == MarketType.Walmart
                || marketToInfo.Market == MarketType.Walmart)
            {
                if (!orderIdList.Any())
                    orderIdList.AddRange(EmailParserHelper.GetWalmartAssociatedOrderIds(result.Email.Subject).ToList());

                if (!orderIdList.Any())
                    orderIdList.AddRange(EmailParserHelper.GetWalmartAssociatedOrderIds(result.Email.Message));
            }

            if (marketFromInfo.Market == MarketType.WalmartCA
                || marketToInfo.Market == MarketType.WalmartCA
                || marketFromInfo.Market == MarketType.Walmart
                || marketToInfo.Market == MarketType.Walmart)
            {
                if (!orderIdList.Any())
                    orderIdList.AddRange(EmailParserHelper.GetWalmartCAAssociatedOrderIds(result.Email.Subject).ToList());

                if (!orderIdList.Any())
                    orderIdList.AddRange(EmailParserHelper.GetWalmartCAAssociatedOrderIds(result.Email.Message));
            }

            //Amazon
            //NOTE: Default amazon body processing
            if (!orderIdList.Any())
                orderIdList.AddRange(EmailParserHelper.GetAmazonAssociatedOrderIds(result.Email.Subject).ToList());
            if (!orderIdList.Any())
                orderIdList.AddRange(EmailParserHelper.GetAmazonAssociatedOrderIds(result.Email.Message)); //Use body only when subject hasn't any orderId (Amazon orders get a lot of trash numbers)
            
            var emailOrderIds = new List<string>();
            if (result.Email.FolderType == (int)EmailFolders.Inbox)
            {
                var orderByEmail = db.Orders.GetAll()
                        .OrderByDescending(o => o.OrderDate)
                        .FirstOrDefault(o => o.BuyerEmail == fromAddress);

                if (orderByEmail != null 
                    && !orderIdList.Contains(orderByEmail.CustomerOrderId)) //NOTE: ignore OrderId if already exist in Subject/Bodu
                {
                    emailOrderIds.Add(orderByEmail.CustomerOrderId);
                }
            }

            if (result.Email.FolderType == (int)EmailFolders.Sent)
            {
                var orderByEmail = db.Orders.GetAll()
                        .OrderByDescending(o => o.OrderDate)
                        .FirstOrDefault(o => o.BuyerEmail == toAddress);

                if (orderByEmail != null
                    && !orderIdList.Contains(orderByEmail.CustomerOrderId)) //NOTE: ignore OrderId if already exist in Subject/Bodu
                {
                    emailOrderIds.Add(orderByEmail.CustomerOrderId);
                }
            }

            if (!orderIdList.Any())
            {
                if (result.Email.FolderType == (int)EmailFolders.Inbox)
                {
                    //NOTE: checking other emails from that address, if we have any assigned recently (not more than 3 months) and haven't multiple options assign it
                    var checkFromDate = _time.GetAppNowTime().AddMonths(-3);
                    var otherAssignedEmails = (from e in db.Emails.GetAll()
                                               join eToO in db.EmailToOrders.GetAll() on e.Id equals eToO.EmailId
                                               where e.From == fromAddress
                                                && e.ReceiveDate > checkFromDate
                                               select eToO.OrderId).Distinct().ToList();
                    if (otherAssignedEmails.Count() == 1)
                        emailOrderIds.Add(otherAssignedEmails.First());
                }
            }

            orderIdList.AddRange(emailOrderIds);
            orderIdList = orderIdList.Distinct().ToList();

            _log.Info("Id: " + result.Email.Id + ", order matches: " + String.Join(", ", orderIdList));
            if (orderIdList.Any())
            {
                var orders = db.Orders.GetAllByCustomerOrderNumbers(orderIdList);
                if (orders.Any())
                {
                    var newOrderIdList = new List<string>();
                    foreach (var order in orders)
                    {
                        if (!newOrderIdList.Contains(order.CustomerOrderId))
                        {
                            _log.Info("Add CustomerOrderId matchId: " + order.CustomerOrderId);
                            newOrderIdList.Add(order.CustomerOrderId);
                        }
                    }
                    if (newOrderIdList.Any(o => !emailOrderIds.Contains(o)))
                        newOrderIdList = newOrderIdList.Where(o => !emailOrderIds.Contains(o)).ToList(); //NOTE: remove from orderIdList by email orderId when exist other orderIds, email orderId is optional, uses when no other order mappings

                    orderIdList = newOrderIdList; //NOTE: Replace OrderIds with real orderIds (ex. Walmart Purchase Order # with Customer Order #)
                }

                foreach (var orderId in orderIdList)
                {
                    _log.Info("Add new Order match, orderId=" + orderId);
                    db.EmailToOrders.Add(new EmailToOrder()
                    {
                        EmailId = result.Email.Id,
                        OrderId = orderId,
                        CreateDate = _time.GetAppNowTime()
                    });
                }
                db.Commit();
            }
            else
            {
                _log.Info("No order matches");
            }

            result.MatchedIdList = orderIdList.ToArray();
        }
    }
}
