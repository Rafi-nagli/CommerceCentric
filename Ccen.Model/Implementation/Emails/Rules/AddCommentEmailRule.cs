using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;


namespace Amazon.Model.Implementation.Emails.Rules
{
    public class AddCommentEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;

        public AddCommentEmailRule(ILogService log, 
            ISystemActionService actionService,
            ITime time)
        {
            _log = log;
            _time = time;
            _actionService = actionService;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Status == EmailMatchingResultStatus.New
                && result.HasMatches)
            {
                var subject = result.Email.Subject ?? "";
                if (EmailHelper.IsAutoCommunicationAddress(result.Email.From)
                    && subject.StartsWith("Your e-mail to"))
                {
                    return; //NOTE: Skip processing our emails to customer
                }
                
                _log.Info("AddCommentEmailRule, WasEmailProcessed=" + result.WasEmailProcessed);
                if (!result.WasEmailProcessed)
                {
                    //var orderNumber = result.MatchedIdList.FirstOrDefault();
                    var orders = db.Orders.GetAllByCustomerOrderNumbers(result.MatchedIdList);

                    var message = " > " + EmailHelper.ExtractShortSubject(result.Email.Subject);
                    message += " > " + EmailHelper.ExtractShortMessageBody(result.Email.Message, 200, true);
                    var commentText = "Customers Email" + message;
                    commentText = StringHelper.Substring(commentText, 110);

                    if (orders.Any())
                    {
                        foreach (var order in orders)
                        {
                            if ((order.OrderStatus == OrderStatusEnumEx.Unshipped ||
                                 order.OrderStatus == OrderStatusEnumEx.Pending))
                            {
                                db.OrderComments.Add(new OrderComment()
                                {
                                    OrderId = order.Id,
                                    Message = commentText,
                                    Type = (int) CommentType.IncomingEmail,
                                    LinkedEmailId = result.Email.Id,
                                    CreateDate = _time.GetAppNowTime(),
                                    UpdateDate = _time.GetAppNowTime()
                                });

                                OrderBatchDTO batch = null;
                                if (order.BatchId.HasValue)
                                    batch = db.OrderBatches.GetAsDto(order.BatchId.Value);

                                if (batch == null || !batch.IsLocked) //Exclude set onHold to orders in locked batch
                                {
                                    order.OnHold = true;
                                    order.OnHoldUpdateDate = _time.GetAppNowTime();
                                }

                                db.Commit();

                                _log.Info("Comment was added, orderId=" + order.AmazonIdentifier
                                          + ", customerOrderId=" + order.CustomerOrderId
                                          + ", comment=" + commentText);
                            }
                        }
                    }
                    else
                    {
                        _log.Info("No exist orders");
                        foreach (var orderNumber in result.MatchedIdList)
                        {
                            _log.Info("Action add comment was added, orderId=" + orderNumber);
                            _actionService.AddAction(db,
                                SystemActionType.AddComment,
                                orderNumber,
                                new AddCommentInput()
                                {
                                    OrderNumber = orderNumber,
                                    Message = message,
                                    Type = (int)CommentType.IncomingEmail,
                                    LinkedEmailId = result.Email.Id,
                                },
                                null,
                                null);
                        }
                    }
                }
            }
        }
    }
}
