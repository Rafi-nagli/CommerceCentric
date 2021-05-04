using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Contracts.Orders;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Implementation.Sorting;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.ViewModels
{
    public class BatchCollection
    {
        public IList<OrderBatchViewModel> Batches { get; set; }
    }

    public class OrderBatchViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? PrintStatus { get; set; }
        public string LabelPrintPackPath { get; set; }
        public long? LabelPrintPackId { get; set; }
        public bool Archive { get; set; }
        public int Type { get; set; }
        public bool IsClosed { get; set; }
        public bool IsLocked { get; set; }
        public bool CanArchive { get; set; }
        public int OrdersCount { get; set; }
        public int ShippedOrdersCount { get; set; }
        
        public bool Selected { get; set; }
        public int Number { get; set; }
        
        public bool UnshippedWarning
        {
            get
            {
                //2. If system failed to generate label, and created a new batch, with 1 order like it happen for batch 954, that new batch should be immediately RED, so user will pay attention.
                return (ShippedOrdersCount > 0 
                    && OrdersCount != ShippedOrdersCount 
                    && CreateDate < DateHelper.GetAppNowTime().AddHours(-24))
                    || (Type == (int)BatchType.PrintLabel 
                        && OrdersCount != ShippedOrdersCount);

            }
        }

        public bool IsPrintInProgress
        {
            get { return PrintStatus == (int) BatchPrintStatuses.InProgress; }
        }

        public override string ToString()
        {
            return "Id=" + Id
                   + ", Name=" + Name
                   + ", CreateDate=" + CreateDate
                   + ", PrintStatus=" + PrintStatus
                   + ", LabelPrintPackPath=" + LabelPrintPackPath
                   + ", LabelPrintPackId=" + LabelPrintPackId
                   + ", Archive=" + Archive
                   + ", Type=" + Type
                   + ", CanArchive=" + CanArchive
                   + ", OrdersCount=" + OrdersCount
                   + ", Selected=" + Selected;
        }

        public bool HasLabel
        {
            get { return !string.IsNullOrEmpty(LabelPrintPackPath); }
        }

        public string PrintPackUrl
        {
            get { return UrlHelper.GetPrintLabelPathById(LabelPrintPackId); }
        }

        public static IList<OrderBatchViewModel> GetAllForTabs(IUnitOfWork db, long? selected = null)
        {
            var activeBatches = GetAll(db.OrderBatches, false, selected)
                .Where(b => b.OrdersCount > 0).ToList();

            return activeBatches;
        }

        public static IList<OrderBatchViewModel> GetAll(IOrderBatchRepository batches, bool withArchive = true, long? selected = null)
        {
            return batches.GetBatchesToDisplay(withArchive, selected)
                .Select(b => new OrderBatchViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Archive = b.Archive,
                    Type = b.Type,
                    IsLocked = b.IsLocked,
                    IsClosed = b.IsClosed,
                    CreateDate = b.CreateDate,
                    PrintStatus = b.PrintStatus,
                    LabelPrintPackPath = b.LabelPath,
                    LabelPrintPackId = b.LabelPrintPackId,
                    OrdersCount = b.OrdersCount,
                    ShippedOrdersCount = b.ShippedOrdersCount,
                    CanArchive = b.CanArchive,
                    Selected = selected != null && b.Id == selected
                })
                .OrderByDescending(b => b.CreateDate)
                .ToList();
        }

        public static MessageResult LockBatch(IUnitOfWork db,
            IBatchManager batchManager,
            long batchId,
            DateTime? when)
        {
            batchManager.LockBatch(db, batchId, when);

            return MessageResult.Success();
        }



        public static MessageResult RemoveFromBatch(IUnitOfWork db,
            ILogService log,
            ISystemActionService systemAction,
            IOrderHistoryService orderHistoryService,
            IBatchManager batchManager,
            long batchId,
            long orderId,
            long? by)
        {
            var batch = db.OrderBatches.GetAsDto(batchId);
            var order = db.Orders.GetAll().FirstOrDefault(o => o.Id == orderId);

            if (batch.IsClosed && !batchManager.CanBeRemovedFromBatch(order))
            {
                if (!AccessManager.IsAdmin)
                    return MessageResult.Error("Batch was closed");
            }
            if (batch.IsLocked)
            {
                if (!AccessManager.IsAdmin)
                    return MessageResult.Error("Batch was locked");
            }
            
            if (order != null)
            {
                var previoudBatchId = order.BatchId;

                order.BatchId = null;

                batchManager.CheckRemovedOrder(db, log, systemAction, order, null, by);

                db.Commit();

                orderHistoryService.AddRecord(order.Id, OrderHistoryHelper.AddToBatchKey, previoudBatchId, batch.Name, null, null, by);

                return MessageResult.Success();
            }
            return MessageResult.Error("No order");
        }

        public static MessageResult RemoveMultipleFromBatch(IUnitOfWork db,
            ILogService log,
            ISystemActionService systemAction,
            IOrderHistoryService orderHistoryService,
            IBatchManager batchManager,
            long batchId,
            string orderIds,
            long? toBatchId,
            bool? removeOnHold)
        {
            var by = AccessManager.UserId;
            var wasChanged = false;
            var hasClosed = false;
            var fromBatch = db.OrderBatches.GetAsDto(batchId);
            OrderBatchDTO toBatch = null;
            if (toBatchId.HasValue)
                toBatch = db.OrderBatches.GetAsDto(toBatchId.Value);

            if (fromBatch.IsClosed || (toBatch != null && toBatch.IsClosed))
            {
                if (!AccessManager.CanEditSystemInfo() && !AccessManager.IsAdmin)
                    hasClosed = true;
            }
            if (fromBatch.IsLocked || (toBatch != null && toBatch.IsLocked))
            {
                if (!AccessManager.IsAdmin)
                {
                    return MessageResult.Error("Source or distination batch was locked");
                }
            }

            var orders = db.Orders.GetFiltered(o => o.BatchId == batchId);
            if (!string.IsNullOrEmpty(orderIds) && !removeOnHold.HasValue)
            {
                var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var orderIdList = stringOrderIdList.Select(long.Parse).ToList();
                foreach (var orderId in orderIdList)
                {
                    var order = orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        if (!hasClosed || batchManager.CanBeRemovedFromBatch(order) || AccessManager.IsAdmin)
                        {
                            order.BatchId = toBatchId;

                            batchManager.CheckRemovedOrder(db, log, systemAction, order, toBatchId, by);

                            orderHistoryService.AddRecord(order.Id, OrderHistoryHelper.AddToBatchKey, fromBatch.Id, fromBatch.Name, toBatch?.Id, toBatch?.Name, by);

                            wasChanged = true;
                        }
                    }
                }
            }
            else if (removeOnHold.HasValue && removeOnHold.Value)
            {
                var onHoldOrders = orders.Where(o => o.OnHold).ToList();
                foreach (var order in onHoldOrders)
                {
                    if (!hasClosed || batchManager.CanBeRemovedFromBatch(order))
                    {
                        order.BatchId = toBatchId;

                        batchManager.CheckRemovedOrder(db, log, systemAction, order, toBatchId, by);

                        orderHistoryService.AddRecord(order.Id, OrderHistoryHelper.AddToBatchKey, fromBatch.Id, fromBatch.Name, toBatch?.Id, toBatch?.Name, by);

                        wasChanged = true;
                    }
                }
            }

            db.Commit();

            if (!wasChanged && hasClosed)
            {
                return MessageResult.Error("Source or distination batch was closed");
            }
            if (!wasChanged)
            {
                return MessageResult.Error("Order list is empty");
            }
            return MessageResult.Success();
        }


        public static MessageResult CheckAddOrdersToBatch(IUnitOfWork db,
            string orderIds)
        {
            if (string.IsNullOrEmpty(orderIds))
                return MessageResult.Success();

            var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var orderIdList = stringOrderIdList.Select(long.Parse).ToList();

            var printedShippings = from sh in db.OrderShippingInfos.GetAllAsDto()
                                   join o in db.Orders.GetAll() on sh.OrderId equals o.Id
                                   where orderIdList.Contains(sh.OrderId)
                                      && !String.IsNullOrEmpty(sh.TrackingNumber)
                                      && o.IsForceVisible != true
                                   select sh;

            var printedMailing = db.MailLabelInfos.GetAllAsDto().Where(sh => orderIdList.Contains(sh.OrderId)
                && !sh.LabelCanceled
                && !sh.CancelLabelRequested).ToList();

            if (printedShippings.Any() || printedMailing.Any())
            {
                return MessageResult.Error();
            }

            return MessageResult.Success();
        }




        public static ValueResult<long> AddOrdersToBatch(IUnitOfWork db,
            IOrderHistoryService orderHistoryService,
            long? batchId,
            string orderIds,
            DateTime when,
            long? by)
        {
            if (!string.IsNullOrEmpty(orderIds) && batchId.HasValue)
            {
                var batch = db.OrderBatches.GetAsDto(batchId.Value);
                if (batch.IsClosed)
                {
                    if (!AccessManager.IsSuperAdmin)
                        return ValueResult<long>.Error("Batch was closed", batchId.Value);
                }
                if (batch.IsLocked)
                {
                    if (!AccessManager.IsAdmin)
                        return ValueResult<long>.Error("Batch was locked", batchId.Value);
                }

                var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var orderIdList = stringOrderIdList.Select(long.Parse).ToList();

                db.OrderBatches.AddOrdersToBatch(batchId.Value, orderIdList, when, by);

                foreach (var orderId in orderIdList)
                {
                    orderHistoryService.AddRecord(orderId, OrderHistoryHelper.AddToBatchKey, null, null, batchId, batch.Name, by);
                }

                return ValueResult<long>.Success("Batch was created", batchId.Value);
            }

            return ValueResult<long>.Error("Order list is empty");
        }

        public static MessageResult CreateBatch(IUnitOfWork db,
            IBatchManager batchManager,
            string orderIds,
            string batchName,
            DateTime when,
            long? by)
        {
            if (!string.IsNullOrEmpty(orderIds))
            {
                var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var orderIdList = stringOrderIdList.Select(long.Parse).ToList();
                
                var batchId = batchManager.CreateBatch(db,
                    BatchType.User,
                    batchName,
                    orderIdList,
                    when,
                    by);

                return MessageResult.Success("", batchId.ToString());
            }

            return MessageResult.Error("Order list is empty");
        }

        public static void UpdateBatchName(IUnitOfWork db,
            long batchId, 
            string newName,
            DateTime when,
            long? by)
        {
            var dbBatch = db.OrderBatches.Get(batchId);
            dbBatch.Name = newName;
            dbBatch.UpdateDate = when;
            dbBatch.UpdatedBy = by;

            db.Commit();
        }

        public static bool ToggleBatchArchive(IUnitOfWork db,
            long batchId)
        {
            var batch = db.OrderBatches.Get(batchId);
            batch.Archive = !batch.Archive;
            db.Commit();

            return batch.Archive;
        }
    }
}