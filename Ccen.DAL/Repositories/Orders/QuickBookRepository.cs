using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Quickbooks;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories.Orders
{
    public class QuickBookRepository : Repository<QuickbooksInventory>, IQuickBookRepository
    {
        public QuickBookRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IQueryable<QuickBookExportDTO> GetAllExportAsDto()
        {
            var query = from o in unitOfWork.GetSet<Order>()
                        join oi in unitOfWork.GetSet<OrderItem>() on o.Id equals oi.OrderId
                        join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id into withListing
                        from l in withListing.DefaultIfEmpty()
                        where o.OrderStatus == OrderStatusEnumEx.Shipped
                        orderby o.OrderDate descending
                        select new QuickBookExportDTO()
                        {
                            BuyerName = o.BuyerName,
                            ItemName = l.Title,
                            ItemSku = l.SKU,
                            ItemPrice = oi.ItemPrice,
                            Quantity = oi.QuantityOrdered,
                            OrderDate = o.OrderDate.Value,
                            OrderId = o.Id,
                            OrderNumber = o.AmazonIdentifier,
                        };

            return query;
        }

        public IQueryable<ProductList> GetAllProductList()
        {
            return unitOfWork.GetSet<ProductList>();
        }

        public IQueryable<ItemDTO> GetInventoryAsDto()
        {
            var query = from o in unitOfWork.GetSet<QuickbooksInventory>()
                        select new ItemDTO()
                        {
                            SKU = o.Item,
                            Name = o.Quantity,
                        };

            return query;
        }
    }
}
