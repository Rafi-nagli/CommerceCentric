using System.Collections.Generic;
using Amazon.Core.EntitiesInventory;
using Amazon.DTO.Inventory;
using Amazon.DTO.ScanOrders;

namespace Amazon.Core.Contracts.DbInventory
{
    public interface IItemInventoryMappingRepository : IInventoryRepository<ItemInventoryMapping>
    {
        void AddNewInventory(InventoryDTO inventory, List<ScanItemDTO> items);
    }
}
