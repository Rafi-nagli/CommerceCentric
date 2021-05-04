using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.OrderValidation.Checkers
{
    public class SignConfirmationRemoveBuyerAskedBeforeChecker
    {
        private ILogService _log;
        private IUnitOfWork _db;
        private IEmailService _emailService;
        private ITime _time;

        public SignConfirmationRemoveBuyerAskedBeforeChecker(ILogService log,
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
            if (result.IsSuccess
                && dbOrder.IsSignConfirmation)
            {
                _log.Debug("Disable SignConfirmation by SignConfirmationRemoveBuyerAskedBeforeChecker");

                _db.OrderComments.Add(new OrderComment()
                {
                    OrderId = order.Id,
                    Message = "[System] Sign Confirmation was deleted by the system. The buyer asked removal before.",
                    Type = (int)CommentType.Address,
                    CreateDate = _time.GetAppNowTime(),
                    UpdateDate = _time.GetAppNowTime()
                });

                dbOrder.IsSignConfirmation = false;
            }
        }

        public CheckResult Check(DTOOrder order,
            IList<ListingOrderDTO> orderItems)
        {
            if (order.Id == 0)
                throw new ArgumentOutOfRangeException("order.Id", "Should be non zero");

            if (order.OrderStatus == OrderStatusEnumEx.Pending)
                throw new ArgumentException("order.OrderStatus", "Not supported status Pending");

            CheckResult result = new CheckResult() { IsSuccess = false };

            var removeSignConfirmationByBuyer = false;
            var buyer = _db.Buyers.GetByEmailAsDto(order.BuyerEmail);
            if (buyer != null)
            {
                removeSignConfirmationByBuyer = buyer.RemoveSignConfirmation;
            }

            result = new CheckResult() { IsSuccess = removeSignConfirmationByBuyer };
            
            return result;
        }

    }
}
