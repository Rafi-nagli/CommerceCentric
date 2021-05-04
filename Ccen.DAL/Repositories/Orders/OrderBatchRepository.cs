using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class OrderBatchRepository : Repository<OrderBatch>, IOrderBatchRepository
    {
        public OrderBatchRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public OrderBatchDTO GetAsDto(long batchId)
        {
            return AsDto(GetAll().Where(b => b.Id == batchId)).FirstOrDefault();
        }

        private IQueryable<OrderBatchDTO> AsDto(IQueryable<OrderBatch> query)
        {
            return query.Select(b => new OrderBatchDTO()
            {
                Id = b.Id,
                Name = b.Name,
                Archive = b.Archive,
                IsClosed = b.IsClosed,
                IsLocked = b.IsLocked,
                LockDate = b.LockDate,
                Type = b.Type,
                LabelPrintPackId = b.LablePrintPackId,
                CreateDate = b.CreateDate,
                ScanFormPath = b.ScanFormPath,
                ScanFormId = b.ScanFormId,
            });
        }

        public void CloseBatch(long batchId)
        {
            var batch = Get(batchId);
            batch.IsClosed = true;
            unitOfWork.Commit();
        }

        public void LockBatch(long batchId, DateTime? when)
        {
            var batch = Get(batchId);
            batch.IsLocked = true;
            batch.LockDate = when;
            unitOfWork.Commit();
        }

        public void SetPrintStatus(long batchId, BatchPrintStatuses printStatus)
        {
            var batch = Get(batchId);
            batch.PrintStatus = (int) printStatus;
            unitOfWork.Commit();
        }

        public long[] GetOrderIdsForBatch(long batchId, string[] orderStatuses)
        {
            var query = unitOfWork.Orders
                .GetFiltered(o => o.BatchId == batchId);
            if (orderStatuses != null && orderStatuses.Any())
                query = query.Where(o => orderStatuses.Contains(o.OrderStatus));

            return query.Select(b => b.Id).ToArray();
        }

        public long CreateBatch(string batchName, 
            BatchType batchType,
            IList<long> orderIds, 
            DateTime? when, 
            long? by)
        {
            var batch = new OrderBatch
            {
                Name = batchName,
                Type = (int)batchType,
                CreateDate = when,
                CreatedBy = by
            };
            Add(batch);
            unitOfWork.Commit();
            if (orderIds != null)
            {
                var orders = unitOfWork.Orders.GetFiltered(o => orderIds.Contains(o.Id));
                foreach (var order in orders)
                {
                    order.BatchId = batch.Id;
                }
            }
            unitOfWork.Commit();
            return batch.Id;            
        }

        public void AddOrdersToBatch(long batchId, IList<long> orderIds, DateTime? when, long? by)
        {
            var batch = Get(batchId);
            batch.UpdateDate = when;
            batch.UpdatedBy = by;

            var orders = unitOfWork.Orders.GetFiltered(o => orderIds.Contains(o.Id));
            foreach (var order in orders)
            {
                order.BatchId = batch.Id;
                order.UpdateDate = when;
                order.UpdatedBy = by;
            }
            unitOfWork.Commit();
        }

        public IQueryable<OrderBatchDTO> GetBatchesToDisplay(bool withArchive = true, long? selectedId = null)
        {
            var batches = unitOfWork.GetSet<ViewBatch>().Select(v => new OrderBatchDTO
            {
                Id = v.Id,
                Name = v.Name,
                Archive = v.Archive,
                Type = v.Type,
                IsLocked = v.IsLocked,
                IsClosed = v.IsClosed,
                CanArchive = !v.AllPrinted.HasValue || v.AllPrinted == 1,
                LabelPath = v.FileName,
                PrintStatus = v.PrintStatus,
                LabelPrintPackId = v.LablePrintPackId,
                OrdersCount = v.Count ?? 0,
                ShippedOrdersCount = v.ShippedCount ?? 0,
                CreateDate = v.CreateDate
            });

            if (!withArchive)
            {
                if (selectedId.HasValue)
                    batches = batches.Where(b => !b.Archive || b.Id == selectedId.Value);
                else
                    batches = batches.Where(b => !b.Archive);
            }
            return batches;
        }
    }
}
