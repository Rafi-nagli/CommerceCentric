using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderChangeHistoryRepository : IRepository<OrderChangeHistory>
    {
        IList<OrderChangeHistoryDTO> GetByOrderIdDto(long orderId);
    }
}
