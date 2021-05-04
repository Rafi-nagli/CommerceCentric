using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderItemSourceRepository : IRepository<OrderItemSource>
    {
        void StoreOrderItems(long orderId,
            IList<ListingOrderDTO> items,
            DateTime when);

        IQueryable<OrderItemDTO> GetAllAsDto();
        IQueryable<OrderItemDTO> GetByOrderIdAsDto(string orderNumber);
        IQueryable<OrderItemDTO> GetByOrderIdAsDto(long orderId);
        IQueryable<ListingOrderDTO> GetByOrderIdsAsDto(IList<long> orderIds);

        IQueryable<DTOOrderItem> GetWithListingInfo();

        OrderItem CreateItemFromSourceDto(OrderItemDTO source);
    }
}
