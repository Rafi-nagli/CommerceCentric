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

namespace Amazon.Model.Implementation.Validation
{
    public class RestockChecker
    {
        private ITime _time;
        private ILogService _log;

        public RestockChecker(ILogService log, ITime time)
        {
            _time = time;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (!result.IsSuccess)
            {
                _log.Debug("Set OnHold by CheckNotInRestock");
                dbOrder.OnHold = true;
            }
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order, IList<ListingOrderDTO> items)
        {
            var today = _time.GetAppNowTime();
            var futureShipDate = items.Where(l => l.RestockDate.HasValue).Max(l => l.RestockDate);
            if (futureShipDate.HasValue && futureShipDate.Value > today.Date)
            {
                db.OrderNotifies.Add(
                        ComposeNotify(order.Id,
                            (int)OrderNotifyType.FutureShipping,
                            1,
                            DateHelper.ToDateString(futureShipDate.Value.Date),
                            _time.GetAppNowTime()));

                return new CheckResult() { IsSuccess = false };
            }

            return new CheckResult() { IsSuccess = true };
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
