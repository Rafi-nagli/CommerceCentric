using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Validation
{
    public class PrimeChecker
    {
        private ILogService _log;

        public PrimeChecker(ILogService log)
        {
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                if (ShippingUtils.IsCanada(dbOrder.ShippingCountry))
                {
                    _log.Debug("IsPrime = false because of Canada");
                    dbOrder.OrderType = (int)OrderTypeEnum.Default;
                }
            }
            //TASK: Remove automatic hold
            //if (result.IsSuccess)
            //{
            //    _log.Debug("Set OnHold by CheckPrime");
            //    dbOrder.OnHold = true;
            //}
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder marketOrder)
        {
            return new CheckResult()
            {
                IsSuccess = marketOrder.OrderType == (int)OrderTypeEnum.Prime
            };
        }
    }
}
