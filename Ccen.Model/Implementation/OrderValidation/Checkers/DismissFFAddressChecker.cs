using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class DismissFFAddressChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private IUnitOfWork _db;

        public DismissFFAddressChecker(ILogService log,
            IEmailService emailService,
            IUnitOfWork db,
            ITime time)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
            _db = db;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Dismiss FF address");
                dbOrder.IsDismissAddressValidation = true;
                dbOrder.DismissAddressValidationDate = _time.GetAppNowTime();

                _db.OrderComments.Add(new OrderComment()
                {
                    OrderId = dbOrder.Id,
                    Message = "System Dismissed Address Issue-FF",
                    Type = (int)CommentType.Address,
                    CreateDate = _time.GetAppNowTime(),
                    UpdateDate = _time.GetAppNowTime()
                });

                _db.Commit();
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


            if (addressValidationStatus >= AddressValidationStatus.Invalid)
            {
                var address = order.GetAddressDto();
                var isFFAddress = AddressHelper.IsFFAddress(address);
                
                if (isFFAddress) 
                {
                    return new CheckResult() {IsSuccess = true};
                }
            }
            
            return new CheckResult()
            {
                IsSuccess = false
            };
        }
    }
}
