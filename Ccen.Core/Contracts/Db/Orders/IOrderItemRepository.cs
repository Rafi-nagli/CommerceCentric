using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        void StoreOrderItems(long orderId,
            IList<ListingOrderDTO> items,
            DateTime when);

        IQueryable<OrderItemDTO> GetAllAsDto();
        IQueryable<ListingOrderDTO> GetAllAsListingDto();
        IQueryable<OrderItemDTO> GetByOrderIdAsDto(string orderNumber);
        IQueryable<OrderItemDTO> GetByOrderIdAsDto(long orderId);
        IQueryable<OrderItemDTO> GetByShippingInfoIdAsDto(long shippingInfoId);
        IQueryable<OrderItemDTO> GetWithListingInfoByShippingInfoIdAsDto(long shippingInfoId);
        IQueryable<DTOOrderItem> GetWithListingInfo();

        IQueryable<DTOOrderItem> GetOrderPackageSizes(long orderId);
    }        
}
