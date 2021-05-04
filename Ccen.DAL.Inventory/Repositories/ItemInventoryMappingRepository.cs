using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.DbInventory;
using Amazon.Core.EntitiesInventory;
using Amazon.DAL.Inventory.Contracts;
using Amazon.DTO.Inventory;
using Amazon.DTO.ScanOrders;

namespace Amazon.DAL.Inventory.Repositories
{
    public class ItemInventoryMappingRepository : Repository<ItemInventoryMapping>, IItemInventoryMappingRepository
    {
        public ItemInventoryMappingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public void AddNewInventory(InventoryDTO inventoryDto, List<ScanItemDTO> items)
        {
            var inventory = new Core.EntitiesInventory.Inventory
            {
                Description = inventoryDto.Description,
                InventoryDate = inventoryDto.InventoryDate
            };
            unitOfWork.GetSet<Core.EntitiesInventory.Inventory>().Add(inventory);
            unitOfWork.Commit();
            foreach (var dto in items)
            {
                var item = unitOfWork.GetSet<Item>().FirstOrDefault(i => i.Barcode == dto.Barcode);
                if (item == null)
                {
                    item = new Item
                    {
                        Barcode = dto.Barcode
                    };
                    unitOfWork.GetSet<Item>().Add(item);
                    unitOfWork.Commit();
                }
                Add(new ItemInventoryMapping
                {
                    ItemId = item.Id,
                    InventoryId = inventory.Id,
                    Quantity = dto.Quantity
                });
            }
            unitOfWork.Commit();
        }
    }
}
