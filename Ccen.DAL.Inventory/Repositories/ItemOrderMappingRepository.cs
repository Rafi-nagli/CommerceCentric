using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.DbInventory;
using Amazon.Core.EntitiesInventory;
using Amazon.DAL.Inventory.Contracts;
using Amazon.DTO.ScanOrders;

namespace Amazon.DAL.Inventory.Repositories
{
    public class ItemOrderMappingRepository :  Repository<ItemOrderMapping>, IItemOrderMappingRepository
    {
        public ItemOrderMappingRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void AddNewOrder(ScanOrderDTO orderDto, List<ScanItemDTO> items)
        {
            var order = new Order
            {
                IsFBA = orderDto.IsFBA,
                OrderDate = orderDto.OrderDate,
                Description = orderDto.Description,
                FileName = orderDto.FileName,
            };
            unitOfWork.GetSet<Order>().Add(order);
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
                Add(new ItemOrderMapping
                {
                    ItemId = item.Id,
                    OrderId = order.Id,
                    Quantity = dto.Quantity
                });
                dto.Id = item.Id;
            }
            unitOfWork.Commit();
        }
    }
}
