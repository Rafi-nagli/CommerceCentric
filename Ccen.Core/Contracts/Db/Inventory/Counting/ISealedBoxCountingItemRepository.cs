using System;
using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface ISealedBoxCountingItemRepository : IRepository<SealedBoxCountingItem>
    {
        IList<SealedBoxCountingItemDto> GetByBoxIdAsDto(long sealedBoxId);

        void UpdateBoxItemsForBox(long boxId,
            IList<SealedBoxCountingItemDto> boxItems,
            DateTime when,
            long? by);
    }
}
