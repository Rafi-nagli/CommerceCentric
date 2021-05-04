using System;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.DAL.Repositories
{
    public class VendorOrderItemRepository : Repository<VendorOrderItem>, IVendorOrderItemRepository
    {
        public VendorOrderItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<VendorOrderItemDTO> GetAllAsDto()
        {
            return GetAll().Select(i => new VendorOrderItemDTO()
            {
                Id = i.Id,
                VendorOrderId = i.VendorOrderId,
                StyleString = i.StyleString,
                Name = i.Name,

                Quantity = i.Quantity,
                Price = i.Price,
                QuantityDate1 = i.QuantityDate1,
                //QuantityDate2 = i.QuantityDate2,
                //SubtotalDate1 = i.SubtotalDate1,
                //LineTotal = i.LineTotal,
                TargetSaleDate = i.TargetSaleDate,
                
                Comment = i.Comment,
                AvailableQuantity = i.AvailableQuantity,
                RelatedStyle = i.RelatedStyle,
                Reason = i.Reason,
                Picture = i.Picture,
                IsDeleted = i.IsDeleted,

                CreateDate = i.CreateDate
            });
        }
    }
}
