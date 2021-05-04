using System;
using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IOpenBoxItemRepository : IRepository<OpenBoxItem>
    {
        IList<OpenBoxItemDto> GetByBoxIdAsDto(long openBoxId);

        IList<EntityUpdateStatus<long>> UpdateBoxItemsForBox(long boxId,
            IList<OpenBoxItemDto> boxItems,
            DateTime when,
            long? by);
    }
}
