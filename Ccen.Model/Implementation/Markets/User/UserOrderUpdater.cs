using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.User
{
    public class UserOrderUpdater
    {
        private ILogService _log;
        private ITime _time;
        private UserOrderApi _api;
        private IOrderHistoryService _orderHistory;

        public UserOrderUpdater(UserOrderApi api,
            IOrderHistoryService orderHistory,
            ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
            _api = api;
            _orderHistory = orderHistory;
        }

        public void UpdateOrders(IUnitOfWork db)
        {
            var shippingLabels = db.OrderShippingInfos.GetOrdersToFulfillAsDTO(_api.Market, _api.MarketplaceId).ToList();
            var shippingInfoIds = shippingLabels.Select(i => i.Id).ToList();
            var dbShippings = db.OrderShippingInfos.GetFiltered(sh => shippingInfoIds.Contains(sh.Id)).ToList();

            var mailLabels = db.MailLabelInfos.GetInfosToFulfillAsDTO(_api.Market, _api.MarketplaceId).ToList();
            var mailInfoIds = mailLabels.Select(i => i.Id).ToList();
            var dbMailInfoes = db.MailLabelInfos.GetFiltered(m => mailInfoIds.Contains(m.Id)).ToList();

            var combinedList = shippingLabels;
            combinedList.AddRange(mailLabels);

            _log.Info("Orders to update, count=" + combinedList.Count);

            if (combinedList.Any())
            {
                foreach (var shipping in combinedList)
                {
                    if (UpdateOrder(db, shipping))
                    {
                        if (shipping.IsFromMailPage)
                        {
                            var dbMailShipping = dbMailInfoes.FirstOrDefault(m => m.Id == shipping.Id);
                            if (dbMailShipping != null)
                                dbMailShipping.IsFulfilled = true;
                        }
                        else
                        {
                            var dbOrderShipping = dbShippings.FirstOrDefault(sh => sh.Id == shipping.Id);
                            if (dbOrderShipping != null)
                                dbOrderShipping.IsFulfilled = true;
                        }
                    }
                }

                db.Commit();
            }
        }

        public bool UpdateOrder(IUnitOfWork db, ShippingDTO shipping)
        {
            _log.Info("Update order: id=" + shipping.OrderId + ", orderId=" + shipping.AmazonIdentifier + ", marketId=" + shipping.MarketOrderId);

            var dbOrder = db.Orders.Get(shipping.OrderId);
            if (dbOrder.OrderStatus == OrderStatusEnumEx.Unshipped)
            {
                _log.Info("Order status changed: " + dbOrder.OrderStatus + " => " + OrderStatusEnumEx.Shipped);
                _orderHistory.AddRecord(dbOrder.Id, OrderHistoryHelper.StatusChangedKey, dbOrder.OrderStatus, OrderStatusEnumEx.Shipped, null);

                dbOrder.OrderStatus = OrderStatusEnumEx.Shipped;
            }
            db.Commit();

            return true;
        }

        public void ProcessCancellation(IUnitOfWork db)
        {
            var dbOrdersToCancel = (from o in db.Orders.GetAll()
                                    join n in db.OrderNotifies.GetAll() on o.Id equals n.OrderId
                                    where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                     && n.Type == (int)OrderNotifyType.CancellationRequest
                                    select o)
                                .ToList();

            if (dbOrdersToCancel.Any())
            {
                foreach (var dbOrder in dbOrdersToCancel)
                {
                    _log.Info("Order status changed: " + dbOrder.OrderStatus + " => " + OrderStatusEnumEx.Canceled);
                    _orderHistory.AddRecord(dbOrder.Id, OrderHistoryHelper.StatusChangedKey, dbOrder.OrderStatus, OrderStatusEnumEx.Canceled, null);
                    dbOrder.SourceOrderStatus = OrderStatusEnumEx.Canceled;
                    dbOrder.OrderStatus = OrderStatusEnumEx.Canceled;
                }
                db.Commit();
            }
        }

    }
}
