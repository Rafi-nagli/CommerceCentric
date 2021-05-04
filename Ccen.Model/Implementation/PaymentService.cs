using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO.Orders;
using Newtonsoft.Json;
using Shopify.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation
{
    public class PaymentService : IPaymentService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public PaymentService(IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        public void ReCheckPaymentStatuses(IPaymentStatusApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIdsToUpdate = db.Orders.GetAll()
                    .Where(o => o.Market == (int)MarketType.Shopify
                        && o.OrderStatus == OrderStatusEnumEx.Unshipped
                        && o.DropShipperId != DSHelper.OverseasId)
                    .Select(o => o.MarketOrderId)
                    .Distinct()
                    .ToList();

                _log.Info("Items to recheck: " + orderIdsToUpdate.Count);
                var sleeper = new StepSleeper(TimeSpan.FromSeconds(1), 3);
                foreach (var orderId in orderIdsToUpdate)
                {
                    var riskStatus = RetryHelper.ActionWithRetries(() => { return api.GetOrderRisk(orderId); }, _log);
                    if (riskStatus?.PaymentValidationStatuses == (int)PaymentValidationStatuses.Red)
                    {
                        var validationInfo = new OrderValidationDTO()
                        {
                            CreateDate = _time.GetAppNowTime(),
                            VerificationStatus = riskStatus.Recommendation,
                            Score = StringHelper.TryGetLong(riskStatus.Score)
                        };

                        var dbOrderList = db.Orders.GetAll().Where(o => o.MarketOrderId == orderId).ToList();
                        foreach (var dbOrder in dbOrderList)
                        {
                            if (dbOrder.OrderStatus == OrderStatusEnumEx.Shipped)
                                continue;

                            if (dbOrder.SignifydStatus == riskStatus.PaymentValidationStatuses)
                                continue;

                            dbOrder.SignifydDesc = JsonConvert.SerializeObject(validationInfo, Formatting.Indented);
                            dbOrder.SignifydStatus = riskStatus.PaymentValidationStatuses;

                            _log.Info("OnHold By PaymentChecker, orderId=" + dbOrder.AmazonIdentifier);
                            dbOrder.OnHold = true;
                            dbOrder.OnHoldUpdateDate = _time.GetAppNowTime();
                        }
                    }
                }

                db.Commit();
            }
        }
    }
}
