using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Validation
{
    public class DhlChecker
    {
        private ILogService _log;

        public DhlChecker(ILogService log)
        {
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Set OnHold by DhlChecker");
                dbOrder.OnHold = true;
            }
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems)
        {
            if (marketOrder.ShipmentProviderType == (int) ShipmentProviderType.Dhl)
            {
                if (orderItems.All(i => i.Weight > 0)
                    && orderItems.Sum(i => i.Weight) > 48) //3Lb
                {
                    return new CheckResult()
                    {
                        IsSuccess = true
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
