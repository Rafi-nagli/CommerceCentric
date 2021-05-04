using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderNotifyRepository : IRepository<OrderNotify>
    {
        void StoreGetRateMessage(long orderId, GetRateResultType resultType, string message, DateTime when);
        IList<OrderNotifyDto> GetForOrders(IList<long> orderIds);
        bool IsExist(long orderId, OrderNotifyType notifyType);
        void UpdateForOrder(long orderId,
            OrderNotifyType notifyType,
            IList<OrderNotifyDto> notifies,
            DateTime when);

        IQueryable<OrderNotifyDto> GetAllAsDto();
    }
}
