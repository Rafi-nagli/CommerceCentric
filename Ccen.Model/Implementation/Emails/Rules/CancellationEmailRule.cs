using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO.Users;
using Amazon.Model.Models.EmailInfos;
using Newtonsoft.Json;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class CancellationEmailRule : IEmailRule
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private ISystemActionService _systemAction;
        private CompanyDTO _company;

        public CancellationEmailRule(ILogService log,
            ITime time,
            IEmailService emailService,
            ISystemActionService systemAction,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _systemAction = systemAction;
            _emailService = emailService;
            _company = company;
        }

        public void Process(IUnitOfWork db,
            EmailReadingResult result)
        {
            if (result.Status == EmailMatchingResultStatus.New
                && result.HasMatches)
            {
                var cancelRequested =
                    (result.Email.Subject ?? "").IndexOf("Order cancellation request from Amazon customer", StringComparison.OrdinalIgnoreCase) >= 0
                    || ((result.Email.Subject ?? "").IndexOf("Walmart.com Customer:", StringComparison.OrdinalIgnoreCase) >= 0
                        && (result.Email.Subject ?? "").IndexOf("Cancel Order", StringComparison.OrdinalIgnoreCase) >= 0) //NOTE: Walmart subject may have differenct spaces count
                    || ((result.Email.Subject ?? "").IndexOf("Walmart.ca Customer:", StringComparison.OrdinalIgnoreCase) >= 0
                        && (result.Email.Subject ?? "").IndexOf("Order cancellation request", StringComparison.OrdinalIgnoreCase) >= 0)
                    || (StringHelper.IsEqualNoCase(result.Email.Subject, "Please contact the customer - Request Cancel"));

                if (cancelRequested)
                {
                    _log.Info("Received OrderCancellation request, orderNumbers=" + String.Join(", ", result.MatchedIdList));

                    var orders = db.Orders.GetAllByCustomerOrderNumbers(result.MatchedIdList);
                    if (orders.Count > 1)
                    {
                        throw new ArgumentException("Multiple orders suite to canceled order Id, orderNumbers=" + String.Join(", ", result.MatchedIdList), "orderId");
                    }

                    if (orders.Any())
                    {
                        foreach (var order in orders)
                        {
                            _log.Info("Received OrderCancellation request, orderNumber=" + order.AmazonIdentifier);
                            var orderNumber = order.AmazonIdentifier;

                            CancelOrder(db, result, orderNumber, order);
                        }
                    }
                    else
                    {
                        var orderNumber = result.MatchedIdList.FirstOrDefault();
                        _log.Info("Received OrderCancellation request, orderNumber=" + orderNumber);
                        _log.Info("Order not found");
                        CancelOrder(db, result, result.MatchedIdList.FirstOrDefault(), null);
                    }
                }
            }
        }

        private void CancelOrder(IUnitOfWork db,
            EmailReadingResult result,
            string orderNumber, 
            Order order)
        {
            if (!String.IsNullOrEmpty(orderNumber))
            {
                var itemId = EmailHelper.ExtractWalmartItemId(result.Email.Message);

                _log.Info("ItemId=" + itemId);

                var existCancellationActions = _systemAction.GetByTypeAndTag(db,
                    SystemActionType.UpdateOnMarketCancelOrder, orderNumber);
                var isExistCancelRequest = existCancellationActions.Any();

                if (existCancellationActions.Any()
                    && order != null
                    && (order.Market == (int)MarketType.Walmart
                    || order.Market == (int)MarketType.WalmartCA)) //NOTE: for Walmart checking by item cancallation
                {
                    var existItemCancallation = false;
                    foreach (var action in existCancellationActions)
                    {
                        var data = JsonConvert.DeserializeObject<CancelOrderInput>(action.InputData);
                        if (data.ItemId == itemId)
                        {
                            existItemCancallation = true;
                        }
                    }
                    isExistCancelRequest = existItemCancallation;
                }

                if (!isExistCancelRequest)
                {
                    result.WasEmailProcessed = true;
                    _log.Info("CancellationEmailRule, WasEmailProcessed=" + result.WasEmailProcessed);

                    var comment = "";

                    //NOTE: "system to cancel that order if it wasn’t shipped and it’s not assigned to Active Batch yet, and send client an email"
                    //NOTE: if no order in system
                    if (order == null ||
                        ((order.OrderStatus == OrderStatusEnumEx.Unshipped
                          || order.OrderStatus == OrderStatusEnumEx.Pending
                          || order.OrderStatus == OrderStatusEnumEx.Canceled)
                         && !order.BatchId.HasValue))
                    {
                        _log.Info("Order status was changed to Canceled, orderNumber=" + orderNumber);

                        SystemActionHelper.AddCancelationActionSequences(db,
                            _systemAction,
                            order.Id,
                            orderNumber,
                            itemId,
                            result.Email.Id,
                            result.Email.From,
                            (order != null && (order.Market == (int)MarketType.Walmart || order.Market == (int)MarketType.WalmartCA)) ? result.Email.Subject : null,
                            EmailHelper.ExtractShortMessageBody(result.Email.Message, 200, true),
                            null,
                            CancelReasonType.PerBuyerRequest);

                        if (order != null && order.Market != (int)MarketType.Walmart && order.Market != (int)MarketType.WalmartCA)
                        //NOTE: Exclude Walmart, cancellation happen only for one item
                        {
                            order.OrderStatus = OrderStatusEnumEx.Canceled;
                        }
                        //if (order != null && (order.Market == (int) MarketType.Walmart || order.Market == (int)MarketType.WalmartCA))
                        //{
                        //    if (!order.BatchId.HasValue) //Only when not in batch
                        //    {
                        //        var orderItemLineCount = db.OrderItems.GetByOrderIdAsDto(order.Id).Count();
                        //        if (orderItemLineCount > 1)
                        //        {
                        //            _log.Info("Walmart Order set OnHold, in case it has more then one item line = " + orderItemLineCount);
                        //            order.OnHold = true;

                        //            db.OrderComments.Add(new OrderComment()
                        //            {
                        //                OrderId = order.Id,
                        //                Message = String.Format("[System] Partial email cancellation request, 1 / {0} order lines", orderItemLineCount),
                        //                Type = (int)CommentType.ReturnExchange,
                        //                LinkedEmailId = result.Email.Id,
                        //                CreateDate = _time.GetAppNowTime(),
                        //                UpdateDate = _time.GetAppNowTime(),
                        //            });
                        //        }
                        //    }
                        //}

                        if (order != null)
                        {
                            comment = "Marked as cancelled + emailed";

                            db.OrderNotifies.Add(new OrderNotify()
                            {
                                OrderId = order.Id,
                                Status = 1,
                                Type = (int)OrderNotifyType.CancellationRequest,
                                Params = itemId,
                                Message = "Email cancelation request in progress",
                                CreateDate = _time.GetAppNowTime(),
                            });
                        }
                    }
                    else
                    {
                        var commentText =
                            "[System] Email cancelation request wasn't processed. Order already shipped.";

                        if (order.OrderStatus != OrderStatusEnumEx.Shipped)
                        {
                            db.OrderNotifies.Add(new OrderNotify()
                            {
                                OrderId = order.Id,
                                Status = 1,
                                Type = (int)OrderNotifyType.CancellationRequest,
                                Params = itemId,
                                Message = "Email cancelation request, order in batch",
                                CreateDate = _time.GetAppNowTime(),
                            });
                        }
                        else
                        {
                            _systemAction.AddAction(db,
                                SystemActionType.SendEmail,
                                orderNumber,
                                new SendEmailInput()
                                {
                                    EmailType = EmailTypes.RejectOrderCancellationToBuyer,
                                    OrderId = orderNumber,
                                    ReplyToEmail = result.Email.From,
                                    ReplyToSubject = result.Email.Subject,
                                },
                                null,
                                null);

                            commentText += " Email was sent to customer.";
                        }

                        db.OrderComments.Add(new OrderComment()
                        {
                            OrderId = order.Id,
                            Message = commentText,
                            Type = (int)CommentType.ReturnExchange,
                            LinkedEmailId = result.Email.Id,
                            CreateDate = _time.GetAppNowTime(),
                            UpdateDate = _time.GetAppNowTime(),
                        });

                        comment = "Cancellation skipped";
                    }

                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                    {
                        OrderNumber = orderNumber,
                        Type = (int)OrderEmailNotifyType.InputOrderCancelledEmail,
                        Reason = comment,
                        CreateDate = _time.GetUtcTime()
                    });

                    db.Commit();
                }
                else
                {
                    _log.Info("Repeated OrderCancellation email, no action");
                }
            }
            else
            {
                _log.Info("Can't OrderCancellation, no order number!");
            }
        }
    }
}
