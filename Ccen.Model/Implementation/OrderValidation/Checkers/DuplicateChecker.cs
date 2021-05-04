using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class DuplicateChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;

        public DuplicateChecker(ILogService log,
            IEmailService emailService,
            ITime time)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (!result.IsSuccess)
            {
                _log.Info("Set on hold by duplcate checker");
                dbOrder.OnHold = true;
            }
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order, IList<ListingOrderDTO> items)
        {
            var result = DuplicateValidatorCheck(db, order, items);
            _log.Info("CheckDuplicate, result=" + result.IsSuccess + ", message=" + result.Message);

            if (!result.IsSuccess)
            {
                db.OrderNotifies.Add(
                    ComposeNotify(order.Id,
                        (int)OrderNotifyType.Duplicate,
                        1,
                        result.Message,
                        _time.GetAppNowTime()));

                db.Commit();

                var emailInfo = new DuplicateEmailInfo(_emailService.AddressService,
                    order.CustomerOrderId,
                    (MarketType)order.Market,
                    new DuplicateOrdersDTO
                    {
                        Items = items,
                        OrderNumbers = result.AdditionalData
                    },
                    order.BuyerName,
                    order.BuyerEmail);

                _emailService.SendEmail(emailInfo, CallSource.Service);
                _log.Info("Send duplicated order email, orderId=" + order.Id);

                db.OrderEmailNotifies.Add(new OrderEmailNotify()
                {
                    OrderNumber = order.OrderId,
                    Reason = "System emailed, found duplicate",
                    Type = (int)OrderEmailNotifyType.OutputDuplicateAlertEmail,
                    CreateDate = _time.GetUtcTime(),
                });

                db.OrderComments.Add(new OrderComment()
                {
                    OrderId = order.Id,
                    Message = "[System] Duplicate order alert email sent",
                    Type = (int)CommentType.OutputEmail,
                    CreateDate = _time.GetAppNowTime(),
                    UpdateDate = _time.GetAppNowTime()
                });

                db.Commit();
            }
            return result;
        }

        private CheckResult DuplicateValidatorCheck(IUnitOfWork db, DTOMarketOrder marketOrder, IList<ListingOrderDTO> orderItems)
        {
            var duplicateOrders = new List<string>();
            var orderWithSameBuyer = db.Orders.GetOrdersWithSimilarDateAndBuyerAndAddress(marketOrder);
            if (orderWithSameBuyer.Any())
            {
                _log.Info("Similar orders, count=" + orderWithSameBuyer.Count);
                foreach (var order in orderWithSameBuyer)
                {
                    var items = db.Listings.GetOrderItems(order.Id);

                    if (items.Any() && items.Count == orderItems.Count)
                    {
                        if (orderItems.All(oItem => items.Any(i => i.ListingId == oItem.ListingId)))
                        {
                            duplicateOrders.Add(order.OrderId);
                            break;
                        }
                        if (orderItems.All(oItem => items.Any(i => i.StyleItemId == oItem.StyleItemId)))
                        {
                            duplicateOrders.Add(order.OrderId);
                            break;
                        }
                    }
                }
            }

            if (duplicateOrders.Any())
            {
                _log.Info("Possible duplicates for order: " + marketOrder.OrderId + "; duplicates: " + string.Join(", ", duplicateOrders));
            }

            return new CheckResult()
            {
                IsSuccess = !duplicateOrders.Any(),
                Message = duplicateOrders.Any() ? duplicateOrders[0] : String.Empty,
                AdditionalData = duplicateOrders
            };
        }

        private OrderNotify ComposeNotify(
            long orderId,
            int type,
            int status,
            string message,
            DateTime when)
        {
            return new OrderNotify()
            {
                OrderId = orderId,
                Status = status,
                Type = type,
                Message = message,
                CreateDate = when
            };
        }
    }
}
