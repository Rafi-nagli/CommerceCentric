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
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class AddressNotChangedEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;
        private IEmailService _emailService;
        private ISystemActionService _actionService;

        public AddressNotChangedEmailRule(ILogService log, 
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
                var subject = result.Email.Subject;
                
                var isChangeAddressRequest =
                    subject.StartsWith("Change shipping address inquiry", StringComparison.OrdinalIgnoreCase);

                if (isChangeAddressRequest)
                {
                    var orderNumber = result.MatchedIdList.FirstOrDefault();
                    _log.Info("Received change shipping address inquiry request, orderNumber=" + orderNumber);

                    if (!String.IsNullOrEmpty(orderNumber))
                    {
                        var isExistAddressNotChangedResponse = db.OrderEmailNotifies.IsExist(orderNumber,
                            OrderEmailNotifyType.OutputAddressNotChanged);
                        if (!isExistAddressNotChangedResponse) //NOTE: First remove change shipping address
                        {
                            var order = db.Orders.GetByOrderNumber(orderNumber);
                            if (order.OrderStatus == OrderStatusEnumEx.Shipped)
                            {
                                result.WasEmailProcessed = true;
                                _log.Info("AddressNotChangedEmailRule, WasEmailProcessed=" + result.WasEmailProcessed);

                                var emailInfo = new AddressNotChangedEmailInfo(_emailService.AddressService, 
                                    null,
                                    orderNumber,
                                    (MarketType)order.Market,
                                    order.BuyerName,
                                    order.BuyerEmail);

                                emailInfo.ReplyToSubject = result.Email.Subject;

                                _emailService.SendEmail(emailInfo, CallSource.Service);

                                var commentText =
                                    "[System] Change shipping address inquiry came after the order shipped. Email was sent to customer.";

                                db.OrderComments.Add(new OrderComment()
                                {
                                    OrderId = order.Id,
                                    Message = commentText,
                                    Type = (int) CommentType.Address,
                                    CreateDate = _time.GetAppNowTime(),
                                    UpdateDate = _time.GetAppNowTime()
                                });

                                db.Commit();
                            }
                            else
                            {
                                _log.Info("AddressNotChangedEmailRule, orderStatus=" + order.OrderStatus);
                            }
                        }
                        else
                        {
                            _log.Info("Repeated change shipping address inquiry email, no action");
                        }
                    }
                    else
                    {
                        _log.Info("Can't checked change shipping address inquiry, no matching orders!");
                    }
                }
            }
        }
    }
}
