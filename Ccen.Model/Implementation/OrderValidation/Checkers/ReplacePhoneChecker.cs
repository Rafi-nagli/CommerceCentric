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
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class ReplacePhoneChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private CompanyDTO _company;

        public ReplacePhoneChecker(ILogService log,
            IEmailService emailService,
            ITime time,
            CompanyDTO company)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
            _company = company;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Info("ReplacePhoneChecker. Phone was changed: " + dbOrder.ManuallyShippingPhone + "=>" + result.AdditionalData[0]);
                if (dbOrder.IsManuallyUpdated)
                    dbOrder.ManuallyShippingPhone = result.AdditionalData[0];
                else
                    dbOrder.ShippingPhone = result.AdditionalData[0];
            }
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder order,
            IList<ListingOrderDTO> items)
        {
            if (order.Id == 0)
                throw new ArgumentOutOfRangeException("order.Id", "Should be non zero");

            if (order.OrderStatus != OrderStatusEnumEx.Pending)
            {
                if (order.ShipmentProviderType == (int)ShipmentProviderType.FedexOneRate)
                {
                    if (String.IsNullOrEmpty(order.FinalShippingPhone))
                    {
                        if (order.IsManuallyUpdated)
                            order.ManuallyShippingPhone = _company.Phone;
                        else
                            order.ShippingPhone = _company.Phone;

                        return new CheckResult()
                        {
                            IsSuccess = true,
                            AdditionalData = new List<string>() { _company.Phone }
                        };
                    }
                }
            }
            return new CheckResult()
            {
                IsSuccess = false
            };            
        }
    }
}
