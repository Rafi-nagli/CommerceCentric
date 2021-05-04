using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderShippingInfoRepository : IRepository<OrderShippingInfo>
    {
        IEnumerable<ShippingDTO> GetOrdersAsShippingDTO(MarketType market, string marketplaceId);
        IEnumerable<ShippingDTO> GetOrdersToFulfillAsDTO(MarketType market, string marketplaceId);
        IQueryable<OrderShippingInfoDTO> GetAllAsDto();
        IList<OrderShippingInfoDTO> GetOrderInfoWithItems(IWeightService weightService, 
            IList<long> orderIds, 
            SortMode sort, 
            bool unmaskReferenceStyle, 
            bool includeSourceItems,
            bool onlyIsActive = true);
        IEnumerable<PackingListDTO> GetPackingSlipOrders(long[] orderIds, SortMode sort, bool unmaskReferenceStyle);

        OrderShippingInfo CreateShippingInfo(RateDTO rate, 
            long orderId, 
            int shippingNumber, 
            int methodId);
        
        IEnumerable<OrderShippingInfo> GetByOrderId(long id);
        IEnumerable<OrderShippingInfo> GetByOrderId(IList<long> idList);

        IList<OrderShippingInfoDTO> GetByOrderIdAsDto(long orderId);
    }
}
