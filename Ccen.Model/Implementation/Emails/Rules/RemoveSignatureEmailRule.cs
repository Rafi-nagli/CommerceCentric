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
using Amazon.Model.Models.EmailInfos;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class RemoveSignatureEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;
        private IEmailService _emailService;
        private ISystemActionService _actionService;

        public RemoveSignatureEmailRule(ILogService log, 
            ITime time,
            IEmailService emailService,
            ISystemActionService actionService)
        {
            _log = log;
            _time = time;
            _emailService = emailService;
            _actionService = actionService;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Status == EmailMatchingResultStatus.New
                && result.HasMatches)
            {
                var noHtml = StringHelper.TrimTags(result.Email.Message ?? "");
                var message = StringHelper.Substring(noHtml, 70).ToLower();
                //to remove the signature confirmation
                var removeSignConfirmation =
                    message.IndexOf("remove signature confirmation", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("remove signature", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("remove the signature", StringComparison.OrdinalIgnoreCase) >= 0;

                if (removeSignConfirmation)
                {
                    var orderNumber = result.MatchedIdList.FirstOrDefault();
                    _log.Info("Received RemoveSignatureConfirmation request, orderNumber=" + orderNumber);

                    if (!String.IsNullOrEmpty(orderNumber))
                    {
                        var isExistRemoveSignConfirmation = db.OrderEmailNotifies.IsExist(orderNumber,
                            OrderEmailNotifyType.InputRemoveSignConfirmationEmail);
                        if (!isExistRemoveSignConfirmation) //NOTE: First remove sign confirmation request
                        {
                            result.WasEmailProcessed = true;
                            _log.Info("RemoveSignatureEmailRule, WasEmailProcessed=" + result.WasEmailProcessed);

                            var comment = String.Empty;
                            var order = db.Orders.GetByOrderNumber(orderNumber);
                            //var now = _time.GetAppNowTime();
                            //var isWeekday = db.Dates.IsWorkday(_time.GetAppNowTime());
                            
                            if (order.OrderStatus == OrderStatusEnumEx.Shipped
                                //|| (isWeekday && now.Hour > 15)
                                )
                            {
                                //comment = "Skipped. Order already shipped or request came after 3PM on weekday";
                                var commentText = "";
                                if (order.OrderStatus == OrderStatusEnumEx.Shipped)
                                {
                                    commentText = "[System] Remove signature confirmation request came after the order shipped";
                                    comment = "Skipped. Order already shipped";
                                }
                                //if (isWeekday && now.Hour > 15)
                                //{
                                //    commentText = "[System] Remove signature confirmation request came after 3PM on weekday";
                                //    comment = "Skipped. Request came after 3PM on weekday";
                                //}
                                db.OrderComments.Add(new OrderComment()
                                {
                                    OrderId = order.Id,
                                    Message = commentText,
                                    Type = (int)CommentType.Address,
                                    CreateDate = _time.GetAppNowTime(),
                                    UpdateDate = _time.GetAppNowTime()
                                });

                                var emailInfo = new RejectRemoveSignConfirmationEmailInfo(_emailService.AddressService, 
                                            null,
                                            orderNumber,
                                            (MarketType)order.Market,
                                            order.BuyerName,
                                            order.BuyerEmail);

                                _emailService.SendEmail(emailInfo, CallSource.Service);
                            }
                            else
                            {
                                //Tring to remove signature

                                if (order.OrderStatus == OrderStatusEnumEx.Unshipped
                                    || order.OrderStatus == OrderStatusEnumEx.PartiallyShipped
                                    || order.OrderStatus == OrderStatusEnumEx.Pending)
                                {
                                    if (order.IsSignConfirmation)
                                    {
                                        order.IsSignConfirmation = false;
                                        _log.Info("Updated IsSignConfirmation=false, orderNumber=" + order.AmazonIdentifier);

                                        db.OrderComments.Add(new OrderComment()
                                        {
                                            OrderId = order.Id,
                                            Message = "[System] Signature confirmation was removed per buyer request",
                                            Type = (int)CommentType.Address,
                                            CreateDate = _time.GetAppNowTime(),
                                            UpdateDate = _time.GetAppNowTime()
                                        });

                                        _actionService.AddAction(db,
                                            SystemActionType.UpdateRates,
                                            order.AmazonIdentifier,
                                            new UpdateRatesInput()
                                            {
                                                OrderId = order.Id
                                            },
                                            null,
                                            null);

                                        var orderItems = db.Listings.GetOrderItems(order.Id);
                                        var emailInfo = new AcceptRemoveSignConfirmationEmailInfo(_emailService.AddressService, 
                                            null,
                                            orderNumber,
                                            (MarketType)order.Market, 
                                            orderItems,
                                            order.BuyerName,
                                            order.BuyerEmail);

                                        _emailService.SendEmail(emailInfo, CallSource.Service);

                                        comment = "Sign confirmation removed + emailed";
                                    }
                                    else
                                    {
                                        db.OrderComments.Add(new OrderComment()
                                        {
                                            OrderId = order.Id,
                                            Message = "[System] Remove signature confirmation request skipped. Signature confirmation was already removed",
                                            Type = (int)CommentType.Address,
                                            CreateDate = _time.GetAppNowTime(),
                                            UpdateDate = _time.GetAppNowTime()
                                        });

                                        comment = "Skipped. Already removed";
                                    }
                                }
                                else
                                {
                                    //NOTE: If OrderStatus=Canceled
                                    comment = "Skipped. Order status=" + order.OrderStatus;
                                }
                            }

                            var dbBuyer = db.Buyers.GetFiltered(b => b.Email == result.Email.From).FirstOrDefault();
                            if (dbBuyer != null)
                            {
                                dbBuyer.RemoveSignConfirmation = true;
                                dbBuyer.RemoveSignConfirmationDate = _time.GetUtcTime();
                                _log.Info("Set RemoveSignConfirmation=true, buyerEmail=" + result.Email.From);
                            }
                            else
                            {
                                _log.Info("Can't find buyerEmail=" + result.Email.From);
                            }

                            db.OrderEmailNotifies.Add(new OrderEmailNotify()
                            {
                                OrderNumber = orderNumber,
                                Type = (int)OrderEmailNotifyType.InputRemoveSignConfirmationEmail,
                                Reason = comment,
                                CreateDate = _time.GetUtcTime()
                            });

                            db.Commit();
                        }
                        else
                        {
                            _log.Info("Repeated RemoveSignConfirmation email, no action");
                        }
                    }
                    else
                    {
                        _log.Info("Can't RemoveSignConfirmation, no matching orders!");
                    }
                }
            }
        }
    }
}
