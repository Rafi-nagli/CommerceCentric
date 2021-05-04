using System;
using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IOpenBoxCountingItemRepository : IRepository<OpenBoxCountingItem>
    {
        IList<OpenBoxCountingItemDto> GetByBoxIdAsDto(long openBoxId);

        void UpdateBoxItemsForBox(long boxId,
            IList<OpenBoxCountingItemDto> boxItems,
            DateTime when,
            long? by);
    }
}
