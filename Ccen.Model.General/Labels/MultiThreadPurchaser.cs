using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.Utils;


namespace Amazon.Model.Implementation.Labels
{
    public class MultiThreadPurchaser : ILabelPurchaser
    {
        private decimal _expectedCost;
        public decimal ExpectedCost { get; set; }

        private decimal _actualCost;

        public decimal ActualCost
        {
            get
            {
                return _actualCost;
            }
            set
            {
                _actualCost = value;
            }
        }

        private IList<OrderShippingInfoDTO> _toPrint;
        private object _toPrintSync = new object();
        public OrderShippingInfoDTO GetNextShipping(ShipmentProviderType providerType)
        {
            lock (_toPrintSync)
            {
                var info = _toPrint.FirstOrDefault(sh => sh.ShipmentProviderType == (int)providerType);
                _log.Info("GetNextShipping, providerType=" + providerType + (info != null ? info.OrderAmazonId : "[null]"));
                _toPrint.Remove(info);
                return info;
            }
        }


        private IList<Thread> _threads;
        private ILogService _log;

        public MultiThreadPurchaser(ILogService log)
        {
            _log = log;
        }

        public void PurshaseLabels(IList<OrderShippingInfoDTO> shippings, Action<OrderShippingInfoDTO> purchaseCallback)
        {
            _toPrint = new List<OrderShippingInfoDTO>(shippings);
            ExpectedCost = (decimal)shippings.Sum(o => o.StampsShippingCost);
            ActualCost = 0;

            _threads = new List<Thread>()
            {
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Amazon, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Amazon, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Amazon, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Amazon, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Amazon, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Stamps, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.Dhl, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.DhlECom, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.IBC, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.SkyPostal, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.FIMS, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.FedexOneRate, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.FedexGeneral, purchaseCallback)),
                new Thread(o => PurshaceProviderLabels(ShipmentProviderType.FedexSmartPost, purchaseCallback))

            };
            _threads.ForEach(t => t.Start());
            _threads.ForEach(t => t.Join()); //Wait all
        }

        private void PurshaceProviderLabels(ShipmentProviderType providerType, Action<OrderShippingInfoDTO> purchaseCallback)
        {
            _log.Info("PurchaceProviderLabels. Started, providerType=" + providerType);
            var info = GetNextShipping(providerType);
            while (info != null)
            {
                _log.Info("Purchasing label, orderId=" + info.OrderAmazonId);
                purchaseCallback(info);

                if (info.LabelPurchaseResult == (int) LabelPurchaseResultType.Error)
                {
                    ExpectedCost -= info.StampsShippingCost ?? 0;
                }
                else
                {
                    var cost = info.StampsShippingCost.HasValue ? info.StampsShippingCost.Value : 0;
                    ActualCost += cost;
                    _log.Info("Actual cost: " + cost);
                }

                info = GetNextShipping(providerType);
            }
            _log.Info("PurchaceProviderLabels. End");
        }
    }
}
