using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.OrderValidation.Checkers
{
    public class HasCancellationChecker
    {
        private ILogService _log;
        private ISystemActionService _systemAction;
        private ITime _time;

        public HasCancellationChecker(ILogService log,
            ISystemActionService systemAction,
            ITime time)
        {
            _time = time;
            _systemAction = systemAction;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {

        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order, IList<ListingOrderDTO> items)
        {
            if (order.OrderStatus == OrderStatusEnumEx.Shipped)
            {
                var existHasCancellation = db.OrderNotifies.IsExist(order.Id, OrderNotifyType.CancellationRequest);
                if (existHasCancellation)
                {
                    if (!db.OrderEmailNotifies.IsExist(order.OrderId,
                            OrderEmailNotifyType.OutputRejectedOrderCancelledEmailToBuyer))
                    {
                        _systemAction.AddAction(db,
                            SystemActionType.SendEmail,
                            order.OrderId,
                            new SendEmailInput()
                            {
                                EmailType = EmailTypes.RejectOrderCancellationToBuyer,
                                OrderId = order.OrderId,
                            },
                            null,
                            null);
                    }
                }
                return new CheckResult() { IsSuccess = false };
            }
            
            return new CheckResult() { IsSuccess = true };
        }
    }
}
