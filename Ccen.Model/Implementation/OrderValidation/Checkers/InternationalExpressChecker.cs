using System;
using System.Collections.Generic;
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

namespace Amazon.Model.Implementation.Validation.Checkers
{
    public class InternationalExpressChecker
    {
        private ILogService _log;
        private ITime _time;

        public InternationalExpressChecker(ILogService log, ITime time)
        {
            _log = log;
            _time = time;
        }

        public void ProcessResult(IUnitOfWork db, CheckResult result, Order order)
        {
            if (result.IsSuccess)
            {
                db.OrderNotifies.Add(new OrderNotify()
                {
                    OrderId = order.Id,
                    Status = (int) OrderNotifyType.InternationalExpress,
                    Type = 1,
                    Message = result.Message,
                    CreateDate = _time.GetAppNowTime()
                });

                _log.Debug("Set OnHold by InternationalExpressChecker");
                order.OnHold = true;

                db.Commit();
            }
        }

        public CheckResult Check(DTOMarketOrder order)
        {
            CheckResult result = new CheckResult() { IsSuccess = false };
            
            var serviceType = order.InitialServiceType;

            if ((ShippingUtils.IsServiceTwoDays(serviceType)
                || ShippingUtils.IsServiceNextDay(serviceType))
                && ShippingUtils.IsInternational(order.ShippingCountry))
            {
                return new CheckResult() { IsSuccess = true };
            }

            return result;
        }
    }
}
