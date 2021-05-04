using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderBatchRepository : IRepository<OrderBatch>
    {
        long[] GetOrderIdsForBatch(long batchId, 
            string[] ordersStatuses);
        long CreateBatch(string batchName, 
            BatchType type,
            IList<long> orderIds, 
            DateTime? when, 
            long? by);
        IQueryable<OrderBatchDTO> GetBatchesToDisplay(bool withArchive = true, 
            long? selectedId = null);
        void AddOrdersToBatch(long batchId, 
            IList<long> orderIds, 
            DateTime? when, 
            long? by);

        void SetPrintStatus(long batchId, 
            BatchPrintStatuses printStatus);

        void CloseBatch(long batchId);
        void LockBatch(long batchId, DateTime? when);
        OrderBatchDTO GetAsDto(long batchId);
    }
}
