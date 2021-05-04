using System;
using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface ISealedBoxItemRepository : IRepository<SealedBoxItem>
    {
        IList<SealedBoxItemDto> GetByBoxIdAsDto(long sealedBoxId);

        IList<EntityUpdateStatus<long>> UpdateBoxItemsForBox(long boxId,
            IList<SealedBoxItemDto> boxItems,
            DateTime when,
            long? by);
    }
}
