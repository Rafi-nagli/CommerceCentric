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
using Amazon.Core.Models.Extensions;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class OrderDeliveryInquiryEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;
        private IEmailService _emailService;
        private ISystemActionService _actionService;

        public OrderDeliveryInquiryEmailRule(ILogService log, 
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
                var subject = (result.Email.Subject ?? "");
                var today = _time.GetAppNowTime().Date;

                //TASK: 1.	Emails which have these in subjects: "Order delivery inquiry" or " Where's My Stuff?”
                //CHANGE: When client sends an email with subject “Package didn’t arrive..” like 113-7086092-7521857, process it same way as emails with subject “Lost Package”…
                var isWhereMyStaff = subject.StartsWith("Order delivery inquiry", StringComparison.OrdinalIgnoreCase)
                                     || subject.IndexOf("Where's My Stuff?", StringComparison.OrdinalIgnoreCase) >= 0
                                     || subject.IndexOf("Where's My Stuff ?", StringComparison.OrdinalIgnoreCase) >= 0
                                     || subject.IndexOf("Package didn’t arrive:", StringComparison.OrdinalIgnoreCase) >= 0
                                     || subject.IndexOf("Package didn?t arrive", StringComparison.OrdinalIgnoreCase) >= 0
                                     || subject.IndexOf("Shipping inquiry", StringComparison.InvariantCultureIgnoreCase) >= 0; //NOTE: actually came with ? instead of '

                if (isWhereMyStaff)
                {
                    _log.Info("Received 'Where my stuff'");
                    var orderNumber = result.MatchedIdList.FirstOrDefault();
                    if (!String.IsNullOrEmpty(orderNumber))
                    {
                        //TASK: 2.	 Which were successfully delivered to client.
                        var order = db.Orders.GetByOrderNumber(orderNumber);
                        var shippingInfos = db.OrderShippingInfos.GetByOrderIdAsDto(order.Id).Where(sh => sh.IsActive).ToList();
                        //a.	 delivered to client (not returned to us) 
                        var delivered = shippingInfos.Any() && shippingInfos.All(sh => sh.DeliveredStatus == (int)DeliveredStatusEnum.Delivered);
                        var deliveryDate = shippingInfos.Select(sh => sh.ActualDeliveryDate).FirstOrDefault();
                        //b.	Delivered within last 2 month.
                        if (delivered && deliveryDate.HasValue && deliveryDate > today.AddMonths(-2))
                        {
                            _log.Info("Package was delivered, withing last 2 month");

                            var alreadySendLostTemplate = db.OrderEmailNotifies.IsExist(order.AmazonIdentifier,
                                OrderEmailNotifyType.OutputLostPackageEmail);

                            //3.	To which we didn’t answer yet with Lost Package template
                            if (!alreadySendLostTemplate)
                            {
                                _log.Info("Not send package lost template");

                                var orderEmails = db.Emails.GetAllByOrderId(order.AmazonIdentifier)
                                    .Select(m => new
                                    {
                                        m.Id,
                                        m.ReceiveDate,
                                        m.Subject
                                    }).ToList();
                                var isAnyOtherSimilarEmails = orderEmails.Any(e => e.Id != result.Email.Id
                                                                                   && (e.ReceiveDate <= result.Email.ReceiveDate
                                                                                   && e.Subject.Contains("Where's My Stuff?")
                                                                                    || e.Subject.Contains("Order delivery inquiry")));

                                var isAnyOtherEmailAfterDelivery = deliveryDate.HasValue && orderEmails.Any(e => e.Id != result.Email.Id
                                                                                        && e.ReceiveDate >= deliveryDate.Value);

                                //4.	If it’s the first email with that subject ("Order delivery inquiry" or " Where's My Stuff?”)
                                if (!isAnyOtherSimilarEmails && !isAnyOtherEmailAfterDelivery)
                                {
                                    _log.Info("Pass to Order Delivery Inquiry Rule");

                                    OrderShippingInfoDTO shippingInfo = null;
                                    ShippingMethodDTO shippingMethod = null;
                                    if (shippingInfos.Any())
                                    {
                                        shippingInfo = shippingInfos.OrderByDescending(sh => sh.ActualDeliveryDate)
                                                .FirstOrDefault(i => i.IsActive);
                                        if (shippingInfo != null)
                                            shippingMethod = db.ShippingMethods.GetByIdAsDto(shippingInfo.ShippingMethodId);
                                    }
                                    var emailInfo = new LostPackageEmailInfo(_emailService.AddressService,
                                        null,
                                        order.AmazonIdentifier,
                                        (MarketType)order.Market,
                                        shippingInfo != null ? shippingInfo.ActualDeliveryDate : null,
                                        shippingInfo != null ? shippingInfo.TrackingStateEvent : null,
                                        shippingMethod != null ? shippingMethod.CarrierName : null,
                                        shippingInfo != null ? shippingInfo.TrackingNumber : null,
                                        order.GetAddressDto(),
                                        order.BuyerName,
                                        order.BuyerEmail);

                                    _emailService.SendEmail(emailInfo, CallSource.Service);

                                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                                    {
                                        OrderNumber = order.CustomerOrderId,
                                        Type = (int) OrderEmailNotifyType.OutputLostPackageEmail,
                                        Reason = "Email: " + StringHelper.Substring(subject, 40),
                                        CreateDate = _time.GetUtcTime()
                                    });

                                    db.OrderComments.Add(new OrderComment()
                                    {
                                        OrderId = order.Id,
                                        Type = (int) CommentType.OutputEmail,
                                        Message = "[System] \"Lost Package\" email sent",
                                        CreateDate = _time.GetAppNowTime(),
                                    });

                                    db.Commit();

                                    result.WasEmailProcessed = true;
                                }
                                else
                                {
                                    if (isAnyOtherSimilarEmails)
                                        _log.Info("Similar email was found");
                                    if (isAnyOtherEmailAfterDelivery)
                                        _log.Info("Other email was found after delivery");
                                }
                            }
                            else
                            {
                                _log.Info("Already sent Lost Package template");
                            }
                        }
                        else
                        {
                            _log.Info("Package not yet delivered");
                        }
                    }
                }
            }
        }
    }
}
