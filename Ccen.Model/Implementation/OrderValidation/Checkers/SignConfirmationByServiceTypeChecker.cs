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
    public class SignConfirmationByServiceTypeChecker
    {
        private ILogService _log;
        private IUnitOfWork _db;
        private IEmailService _emailService;
        private ITime _time;

        public SignConfirmationByServiceTypeChecker(ILogService log,
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
                _log.Debug("Enable SignConfirmation by CheckNeedSignConfirmationByServiceType");
                dbOrder.IsSignConfirmation = true;
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
            
            var serviceType = order.InitialServiceType;

            if (ShippingUtils.IsServiceTwoDays(serviceType)
                    || ShippingUtils.IsServiceNextDay(serviceType))
            {
                result = new CheckResult() { IsSuccess = true };
            }

            return result;
        }

    }
}
