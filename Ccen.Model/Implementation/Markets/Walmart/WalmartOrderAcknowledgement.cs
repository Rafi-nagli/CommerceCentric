using System;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class WalmartOrderAcknowledgement
    {
        private IWalmartApi _api;
        private ILogService _log;
        private ITime _time;

        public WalmartOrderAcknowledgement(IWalmartApi api, ILogService log, ITime time)
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
                    var resultOrder = _api.AcknowledgingOrder(order.MarketOrderId);

                    if (resultOrder.IsSuccess)
                    {
                        _log.Info("Order has been acknowledge");
                    }
                    else
                    {
                        _log.Error("Can't acknowledge order, message=" + resultOrder.Message, resultOrder.Exception);
                    }

                    //NOTE: when do the order sync skipped PENDING->UNSHIPPED validations
                    //if (resultOrder.OrderStatus == OrderStatusEnumEx.Unshipped)
                    //{
                    //    order.OrderStatus = resultOrder.OrderStatus;
                    //    order.SourceOrderStatus = resultOrder.SourceOrderStatus;
                    //    db.Commit();
                    //}
                }
                catch (Exception ex)
                {
                    _log.Error("AcknowledgingOrder", ex);
                }
            }
        }
    }
}
