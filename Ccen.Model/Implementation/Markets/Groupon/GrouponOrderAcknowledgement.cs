using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Groupon.Api;
using Magento.Api.Wrapper;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Markets.Magento
{
    public class GrouponOrderAcknowledgement
    {
        private GrouponApi _api;
        private ILogService _log;
        private ITime _time;

        public GrouponOrderAcknowledgement(GrouponApi api, ILogService log, ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
        }

        public void UpdateOrders(IUnitOfWork db)
        {
            var orderIds = (from o in db.Orders.GetAll()
                            where o.Market == (int)_api.Market
                               && o.OrderStatus == OrderStatusEnumEx.Pending
                            select o.Id).ToList();

            var allOrders = db.Orders.GetAll().Where(o => orderIds.Contains(o.Id)).ToList();
            var allOrderItems = db.OrderItems.GetAllAsDto().Where(oi => orderIds.Contains(oi.OrderId)).ToList();

            foreach (var order in allOrders)
            {
                _log.Info("Begin AcknowledgingOrder, orderId=" + order.MarketOrderId);
                try
                {
                    var orderItems = allOrderItems.Where(oi => oi.OrderId == order.Id).ToList();

                    var ackResult = _api.AcknowledgingOrder(order.AmazonIdentifier, orderItems);

                    if (ackResult.IsSuccess)
                    {
                        _log.Info("Order has been acknowledge");
                    }
                    else
                    {
                        _log.Info("Order already has invoice");
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
