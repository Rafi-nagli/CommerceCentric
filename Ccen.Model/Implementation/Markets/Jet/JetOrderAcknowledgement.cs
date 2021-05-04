using System;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Jet.Api;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class JetOrderAcknowledgement
    {
        private JetApi _api;
        private ILogService _log;
        private ITime _time;

        public JetOrderAcknowledgement(JetApi api, ILogService log, ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
        }

        public void UpdateOrders(IUnitOfWork db)
        {
            var orders = db.Orders.GetAll()
                .Where(m => m.Market == (int) _api.Market
                        && m.OrderStatus == OrderStatusEnumEx.Pending)
                .ToList();

            foreach (var order in orders)
            {
                _log.Info("Begin AcknowledgingOrder, orderId=" + order.MarketOrderId);
                try
                {
                    var orderItems = db.OrderItems.GetAllAsDto().Where(oi => oi.OrderId == order.Id).ToList();

                    var resultOrder = _api.AcknowledgingOrder(order.MarketOrderId, orderItems.Select(oi => oi.ItemOrderIdentifier).ToList());

                    if (resultOrder.IsSuccess)
                    {
                        _log.Info("Order has been acknowledge");
                    }
                    else
                    {
                        _log.Error("Can't acknowledge order, message=" + resultOrder.Message, resultOrder.Exception);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("AcknowledgingOrder", ex);
                }
            }
        }
    }
}
