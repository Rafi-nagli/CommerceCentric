using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Validation
{
    public class BlackListChecker
    {
        private ITime _time;
        private ILogService _log;

        public BlackListChecker(ILogService log, ITime time)
        {
            _time = time;
            _log = log;
        }


        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Enable SignConfirmation by CheckInBlackList");
                dbOrder.OnHold = true;
                dbOrder.IsSignConfirmation = true;
            }
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order)
        {
            var result = BlackListValidatorCheck(db, order);
            if (result.IsSuccess)
            {
                db.OrderNotifies.Add(
                    ComposeNotify(order.Id,
                        (int)OrderNotifyType.BlackList,
                        1,
                        result.Message,
                        _time.GetAppNowTime()));
                db.Commit();
            }
            return result;
        }

        private CheckResult BlackListValidatorCheck(IUnitOfWork db, DTOMarketOrder order)
        {
            var blackListRecords = db.BuyerBlackLists.GetSimular(order);
            if (blackListRecords.Any())
                return new CheckResult()
                {
                    IsSuccess = true,
                    Message = blackListRecords[0].OrderId
                };
            return new CheckResult()
            {
                IsSuccess = false
            };
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
