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
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class PhoneNumberChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;

        public PhoneNumberChecker(ILogService log,
            IEmailService emailService,
            ITime time)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {

            }
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder order,
            IList<ListingOrderDTO> items,
            AddressValidationStatus addressValidationStatus)
        {
            if (order.Id == 0)
                throw new ArgumentOutOfRangeException("order.Id", "Should be non zero");

            if (order.OrderStatus == OrderStatusEnumEx.Pending)
                throw new ArgumentException("order.OrderStatus", "Not supported status Pending");

            //International order has issue with PersonName
            if (ShippingUtils.IsInternational(order.FinalShippingCountry))
            {
                if (String.IsNullOrEmpty(order.FinalShippingPhone))
                {
                    var emailInfo = new PhoneMissingEmailInfo(_emailService.AddressService, 
                        null,
                        order.OrderId,
                        (MarketType)order.Market, 
                        items,
                        order.BuyerName,
                        order.BuyerEmail);

                    _emailService.SendEmail(emailInfo, CallSource.Service);
                    _log.Info("Send phone missing email, orderId=" + order.Id);

                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                    {
                        OrderNumber = order.OrderId,
                        Reason = "System emailed, missing phone number",
                        Type = (int)OrderEmailNotifyType.OutputPhoneMissingEmail,
                        CreateDate = _time.GetUtcTime(),
                    });

                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        Message = "[System] Missing phone email sent",
                        Type = (int)CommentType.Address,
                        CreateDate = _time.GetAppNowTime(),
                        UpdateDate = _time.GetAppNowTime()
                    });

                    db.Commit();

                    return new CheckResult()
                    {
                        IsSuccess = false
                    };
                }
            }

            return new CheckResult()
            {
                IsSuccess = true
            };
        }
    }
}
