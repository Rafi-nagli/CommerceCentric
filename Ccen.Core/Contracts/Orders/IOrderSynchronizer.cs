using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Core.Models;
using Amazon.Core.Models.Orders;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IOrderSynchronizer
    {
        SyncResult Sync(OrderSyncModes syncMode, CancellationToken? cancel);        
        SyncResult ProcessSpecifiedOrder(IUnitOfWork db, string orderId);

        bool UIUpdate(IUnitOfWork db,
            DTOOrder order,
            bool isForceOverride,
            bool keepActiveShipping,
            bool keepCustomShipping,
            int? switchToMethodId);
    }
}
