using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Orders;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Sorting;
using Amazon.Model.Models;


namespace Amazon.Model.Implementation.Labels
{
    public class BatchManager : IBatchManager
    {
        private IOrderHistoryService _orderHistoryService;
        private ILogService _log;
        private ITime _time;
        private IWeightService _weightService;

        public BatchManager(ILogService log,
            ITime time,
            IOrderHistoryService orderHistoryService,
            IWeightService weightService)
        {
            _log = log;
            _time = time;
            _weightService = weightService;
            _orderHistoryService = orderHistoryService;
        }

        public long CreateBatch(IUnitOfWork db,
            BatchType batchType,
            string batchName,
            IList<long> orderIdList,
            DateTime when,
            long? by)
        {
            _log.Info("CreateBatch, batchName=" + batchName + ", type=" + batchType);

            var batchId = db.OrderBatches.CreateBatch(batchName,
                batchType,
                orderIdList,
                when,
                by);

            foreach (var orderId in orderIdList)
            {
                _orderHistoryService.AddRecord(orderId, OrderHistoryHelper.AddToBatchKey, null, null, batchId, batchName, by);
            }

            return batchId;
        }

        public void LockBatch(IUnitOfWork db,
            long batchId,
            DateTime? when)
        {
            _log.Info("Lock batch, batchId=" + batchId);

            var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                    batchId,
                    OrderStatusEnumEx.AllUnshippedWithShipped);

            var shippingList = db.OrderShippingInfos
                .GetOrderInfoWithItems(_weightService,
                    orderIds.ToList(), 
                    SortMode.ByLocation,
                    unmaskReferenceStyle: false,
                    includeSourceItems: false)
                .ToList();

            shippingList = SortHelper.Sort(shippingList, SortMode.ByShippingMethodThenLocation).ToList();

            for (int i = 0; i < shippingList.Count; i++)
            {
                var mapping = new OrderToBatch()
                {
                    BatchId = batchId,
                    ShippingInfoId = shippingList[i].Id,
                    SortIndex1 = ShippingUtils.GetShippingMethodIndex(shippingList[i].ShippingMethodId),
                    SortIndex2 = LocationHelper.GetLocationIndex(shippingList[i].SortIsle, shippingList[i].SortSection, shippingList[i].SortShelf),
                    SortIndex3 = SortHelper.GetStringIndex(shippingList[i].SortStyleString),
                    SortIndex4 = (decimal)SizeHelper.GetSizeIndex(shippingList[i].SortSize),
                    SortIndex5 = SortHelper.GetStringIndex(shippingList[i].SortColor),
                    //SortIndex6 = SortHelper.GetStringIndex(shippingList[i].FirstItemName),
                    CreateDate = when,
                };
                db.OrderToBatches.Add(mapping);
            }
            db.Commit();

            db.OrderBatches.LockBatch(batchId, when);
        }


        public bool CanBeRemovedFromBatch(Order order)
        {
            if (order.OnHold && order.OrderStatus != OrderStatusEnumEx.Shipped)
                return true;
            return false;
        }


        public void CheckRemovedOrder(IUnitOfWork db,
            ILogService log,
            ISystemActionService systemAction,
            Order order,
            long? toBatchId,
            long? by)
        {
            log.Info("CheckRemovedOrder");

            var orderNumber = order.AmazonIdentifier;
            var customerOrderId = order.CustomerOrderId;
            var buyerEmail = order.BuyerEmail;

            //NOTE: when remove from batch if has cancellation request process them
            var cancelationRequests = db.OrderNotifies.GetAll().Where(o => o.OrderId == order.Id && o.Type == (int)OrderNotifyType.CancellationRequest).ToList();
            if (cancelationRequests.Any()
                && toBatchId == null) //Move to Order Page
            {
                log.Info("Order status was changed to Canceled, orderNumber=" + orderNumber);

                var itemIdList = cancelationRequests
                    .Where(r => !String.IsNullOrEmpty(r.Params))
                    .Select(r => r.Params)
                    .Distinct();
                SystemActionHelper.AddCancelationActionSequences(db,
                    systemAction,
                    order.Id,
                    orderNumber,
                    String.Join(";", itemIdList),
                    null,
                    null,
                    null,
                    null,
                    by,
                    CancelReasonType.PerBuyerRequest);

                order.OrderStatus = OrderStatusEnumEx.Canceled;

                db.Commit();
            }
        }
    }
}
