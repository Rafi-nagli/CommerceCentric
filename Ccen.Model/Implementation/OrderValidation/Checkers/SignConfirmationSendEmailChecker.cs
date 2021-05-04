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
    public class SignConfirmationSendEmailChecker
    {
        private ILogService _log;
        private IUnitOfWork _db;
        private IEmailService _emailService;
        private ITime _time;

        public SignConfirmationSendEmailChecker(ILogService log,
            IUnitOfWork db,
            IEmailService emailService,
            ITime time)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
            _db = db;
        }

        public void ProcessResult(CheckResult result, 
            Order dbOrder,
            DTOOrder order,
            IList<ListingOrderDTO> orderItems)
        {
            if (result.IsSuccess)
            {
                _log.Info("Send sign confirmation request order email, orderId=" + order.Id);

                var alreadySend = _db.OrderEmailNotifies.IsExist(order.OrderId,
                    OrderEmailNotifyType.OutputSignConfirmationEmail);

                if (alreadySend)
                    return;

                var emailInfo = new SignConfirmationRequestEmailInfo(_emailService.AddressService,
                    null,
                    order.OrderId,
                    (MarketType)order.Market,
                    orderItems,
                    //NOTE: make sense only express or not
                    ShippingUtils.IsServiceNextDay(order.InitialServiceType) ? ShippingTypeCode.PriorityExpress : ShippingTypeCode.Standard,
                    order.BuyerName,
                    order.BuyerEmail);

                _emailService.SendEmail(emailInfo, CallSource.Service);

                _db.OrderEmailNotifies.Add(new OrderEmailNotify()
                {
                    OrderNumber = order.OrderId,
                    Reason = "System emailed, request signconfirmation",
                    Type = (int)OrderEmailNotifyType.OutputSignConfirmationEmail,
                    CreateDate = _time.GetUtcTime(),
                });

                _db.OrderComments.Add(new OrderComment()
                {
                    OrderId = order.Id,
                    Message = "[System] Sign Confirmation email sent",
                    Type = (int)CommentType.Address,
                    CreateDate = _time.GetAppNowTime(),
                    UpdateDate = _time.GetAppNowTime()
                });

                _db.Commit();
            }
        }

        public CheckResult Check(DTOOrder order,
            Order dbOrder,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings)
        {
            var sendSignConfirmationEmail = dbOrder.IsSignConfirmation;

            //Please don’t ask for signature confirmation if carrier is Dynamex.
            if (shippings != null
                && shippings.Any(sh => sh.IsActive && sh.ShippingMethodId == ShippingUtils.DynamexPTPSameShippingMethodId))
                sendSignConfirmationEmail = false;

            return new CheckResult() { IsSuccess = sendSignConfirmationEmail };
        }

    }
}
