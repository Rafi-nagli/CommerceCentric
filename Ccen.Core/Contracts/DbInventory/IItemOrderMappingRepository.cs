using System.Collections.Generic;
using Amazon.Core.EntitiesInventory;
using Amazon.DTO.ScanOrders;

namespace Amazon.Core.Contracts.DbInventory
{
    public interface IItemOrderMappingRepository : IInventoryRepository<ItemOrderMapping>
    {
        void AddNewOrder(ScanOrderDTO order, List<ScanItemDTO> items);
    }
}
