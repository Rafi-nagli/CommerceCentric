using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IItemChangeHistoryRepository : IRepository<ItemChangeHistory>
    {
        IList<ItemChangeHistoryDTO> GetByItemIdDto(long itemId);
    }
}
