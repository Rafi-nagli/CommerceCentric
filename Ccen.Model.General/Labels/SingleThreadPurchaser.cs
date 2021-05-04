using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;


namespace Amazon.Model.Implementation.Labels
{
    public class SingleThreadPurchaser : ILabelPurchaser
    {
        public decimal ExpectedCost { get; set; }

        public decimal ActualCost { get; set; }

        private ILogService _log;

        public SingleThreadPurchaser(ILogService log)
        {
            _log = log;
        }

        public void PurshaseLabels(IList<OrderShippingInfoDTO> shippings, Action<OrderShippingInfoDTO> purchaseCallback)
        {
            ExpectedCost = (decimal)shippings.Sum(o => o.StampsShippingCost);
            ActualCost = 0;

            foreach (var info in shippings)
            {
                _log.Info("Purchasing label, orderId=" + info.OrderAmazonId);
                
                purchaseCallback(info);

                if (info.LabelPurchaseResult == (int)LabelPurchaseResultType.Error)
                {
                    ExpectedCost -= info.StampsShippingCost ?? 0;
                }
                else
                {
                    var cost = info.StampsShippingCost.HasValue ? info.StampsShippingCost.Value : 0;
                    ActualCost += cost;
                    _log.Info("Actual cost: " + cost);
                }
            }
        }
    }
}
