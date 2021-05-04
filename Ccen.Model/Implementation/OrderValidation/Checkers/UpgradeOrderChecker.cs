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
    public class UpgradeOrderChecker
    {
        private ITime _time;
        private ILogService _log;

        public UpgradeOrderChecker(ILogService log, ITime time)
        {
            _time = time;
            _log = log;
        }


        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("UpgradeLevel=1");
                dbOrder.UpgradeLevel = 1;
            }
        }

        public CheckResult Check(IUnitOfWork db, 
            DTOOrder order,
            IList<ListingOrderDTO> orderItems)
        {
            var min = 1.99M;

            if (ShippingUtils.IsInternational(order.FinalShippingCountry)
                || order.InitialServiceType != ShippingUtils.StandardServiceName
                || orderItems.Sum(i => i.ItemPrice) <= 10)
            {
                return new CheckResult()
                {
                    IsSuccess = false
                };
            }

            var avgDeliveryDays = GetAvgDeliveryInfo(db, order);
            if (avgDeliveryDays != null
                && avgDeliveryDays.AverageFirstClassDeliveryDays.HasValue
                && avgDeliveryDays.AverageFirstClassDeliveryDays.Value >= min)
            {
                _log.Info("Zip=" + avgDeliveryDays.Zip + ", Avg FC Days=" + avgDeliveryDays.AverageFirstClassDeliveryDays + ", min=" + min);

                order.UpgradeLevel = 1;
                return new CheckResult()
                {
                    IsSuccess = true
                };
            }

            return new CheckResult()
            {
                IsSuccess = false
            };
        }

        private ZipCode GetAvgDeliveryInfo(IUnitOfWork db, DTOOrder order)
        {
            var zipCode = db.ZipCodes.GetClosestZipInfo(order.FinalShippingZip);
            return zipCode;
        }
    }
}
