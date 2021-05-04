using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Amazon.Model.Implementation.OrderValidation.Checkers
{
    public class ZipCodeChecker
    {
        private ILogService _log;
        private ITime _time;

        public ZipCodeChecker(ILogService log,
            ITime time)
        {
            _time = time;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Update zipcode, from=" + dbOrder.ShippingZip + ", to=" +
                           result.AdditionalData[0]);

                if (dbOrder.IsManuallyUpdated)
                    dbOrder.ManuallyShippingZip = result.AdditionalData[0];
                else
                    dbOrder.ShippingZip = result.AdditionalData[0];
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

            if (ShippingUtils.IsLatvia(order.FinalShippingCountry))
            {
                if ((order.ShippingZip ?? "").Contains("LV-"))
                {
                    return new CheckResult()
                    {
                        IsSuccess = true,
                        AdditionalData = new[] { (order.ShippingZip ?? "").Replace("LV-", "LV") }
                    };
                }
            }

            return new CheckResult()
            {
                IsSuccess = false
            };
        }
    }
}
