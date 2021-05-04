using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Api;
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
    public class UpgradeShippingServiceChecker
    {
        private ITime _time;
        private ILogService _log;

        public UpgradeShippingServiceChecker(ILogService log, ITime time)
        {
            _time = time;
            _log = log;
        }


        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                if (dbOrder.InitialServiceType == ShippingUtils.StandardServiceName)
                    dbOrder.InitialServiceType = ShippingUtils.ExpeditedServiceName;
            }
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder order,
            IList<ListingOrderDTO> orderItems)
        {
            var canUpgrade = false;

            if (orderItems != null && orderItems.Any(oi => oi.OnMarketTemplateName == AmazonTemplateHelper.OversizeTemplate)
                && order.InitialServiceType == ShippingUtils.StandardServiceName
                && (order.Market == (int)MarketType.Amazon
                    || order.Market == (int)MarketType.AmazonAU
                    || order.Market == (int)MarketType.AmazonEU))
            {
                order.InitialServiceType = ShippingUtils.ExpeditedServiceName;
                canUpgrade = true;
            }

            return new CheckResult()
            {
                IsSuccess = canUpgrade
            };
        }
    }
}
