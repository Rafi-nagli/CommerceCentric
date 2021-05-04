using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderEmailNotifyRepository : IRepository<OrderEmailNotify>
    {
        bool IsExist(string orderNumber, OrderEmailNotifyType type);
        IList<OrderEmailNotifyDto> GetForOrders(IList<string> orderNumberList);
    }
}
