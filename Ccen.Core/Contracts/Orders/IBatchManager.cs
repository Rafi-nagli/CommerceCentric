using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts.Orders
{
    public interface IBatchManager
    {
        long CreateBatch(IUnitOfWork db,
            BatchType batchType,
            string batchName,
            IList<long> orderIdList,
            DateTime when,
            long? by);

        void LockBatch(IUnitOfWork db,
            long batchId,
            DateTime? when);

        void CheckRemovedOrder(IUnitOfWork db,
            ILogService log,
            ISystemActionService systemAction,
            Order order,
            long? toBatchId,
            long? by);

        bool CanBeRemovedFromBatch(Order order);
    }
}
