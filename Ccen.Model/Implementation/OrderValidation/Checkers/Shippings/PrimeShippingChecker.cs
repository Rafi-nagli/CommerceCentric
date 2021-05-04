using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Model.Implementation.Validation
{
    public class PrimeShippingChecker
    {
        private ILogService _log;
        private ITime _time;
        private IEmailService _emailService;

        public PrimeShippingChecker(ILogService log,
            IEmailService emailService,
            ITime time)
        {
            _log = log;
            _time = time;
            _emailService = emailService;
        }

        public void ProcessResult(CheckResult result, IUnitOfWork db, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                if (!dbOrder.OnHold)
                {
                    _log.Debug("Set OnHold by PrimeShippingChecker");
                    dbOrder.OnHold = true;
                    db.Commit();
                }
            }
        }

        public CheckResult Check(IUnitOfWork db,
            long orderId,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings,
            DTOMarketOrder marketOrder)
        {
            if (!orderItems.Any() || !shippings.Any())
                return new CheckResult() { IsSuccess = false };

            if (marketOrder.OrderType != (int)OrderTypeEnum.Prime)
                return new CheckResult() { IsSuccess = false };

            var serviceType = marketOrder.InitialServiceType;
            if (ShippingUtils.IsServiceTwoDays(serviceType)
                || ShippingUtils.IsServiceNextDay(serviceType))
                return new CheckResult() { IsSuccess = false };

            if (shippings != null)
            {
                //NOTE: send notification when Prime order has only express service optionas (when it is has originally priority service) 
                var activeShipping = shippings.FirstOrDefault(sh => sh.IsActive);
                if (activeShipping != null
                    && activeShipping.ShippingMethod != null
                    && (activeShipping.ShippingMethod.Id == ShippingUtils.AmazonExpressFlatShippingMethodId
                        || activeShipping.ShippingMethod.Id == ShippingUtils.AmazonExpressRegularShippingMethodId))
                {
                    _emailService.SendSystemEmail("Prime order issue #" + marketOrder.OrderId,
                        "Prime order hasn't priority rates",
                        EmailHelper.RafiEmail,
                        EmailHelper.SupportDgtexEmail);
                    _log.Info("Send prime notification email, orderId=" + marketOrder.Id);

                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = marketOrder.Id,
                        Message = "[System] Prime order hasn't priority rates",
                        Type = (int)CommentType.OutputEmail,
                        CreateDate = _time.GetAppNowTime(),
                        UpdateDate = _time.GetAppNowTime()
                    });
                    db.Commit();

                    return new CheckResult() { IsSuccess = true };
                }
            }

            return new CheckResult() { IsSuccess = false };
        }
        
    }
}
