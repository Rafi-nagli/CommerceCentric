using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IItemOrderMappingRepository : IRepository<ItemOrderMapping>
    {
        void StoreShippingItemMappings(long shippingId, 
            IList<ListingOrderDTO> orderItems, 
            DateTime? when);

        void StorePartialShippingItemMappings(long shippingId, 
            IList<RateItemDTO> mappedItems, 
            IList<ListingOrderDTO> allOrderItems, 
            DateTime? when);


        //EmailInfoDTO GetEmailInfo(string orderId);
        IEnumerable<ItemDTO> GetLastShippedDateForItem();
        IEnumerable<ItemDTO> GetLastAnyOrderDateForItem();


        IEnumerable<DTOOrder> GetFilteredOrdersWithItems(IWeightService weightService, OrderSearchFilter filter);
        GridResponse<DTOOrder> GetDisplayOrdersWithItems(IWeightService weightService, OrderSearchFilter filter);
        GridResponse<DTOOrder> GetDisplayOrdersShort(IWeightService weightService, OrderSearchFilter filter);
        IEnumerable<DTOOrder> GetFilteredOrderInfos(IWeightService weightService, OrderSearchFilter filter);
        IEnumerable<DTOOrder> GetSelectedOrdersWithItems(IWeightService weightService, long[] list, bool includeSourceItems);
        IEnumerable<DTOOrder> GetOrdersWithItemsByStatus(IWeightService weightService, string[] statusList, MarketType market, string marketplaceId);

        DTOOrder GetOrderWithItems(IWeightService weightService, string orderId, bool unmaskReferenceStyle, bool includeSourceItems);
        DTOOrder GetOrderWithItems(IWeightService weightService, long orderId, bool withAllShippings, bool includeNotify, bool unmaskReferenceStyles);
        
        IList<SoldSizeInfo> GetPendingAndOtherUnshippedOrderItemQtyes(long? batchId);
    }
}
