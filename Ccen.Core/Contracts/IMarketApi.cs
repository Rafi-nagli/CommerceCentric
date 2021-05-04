using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IMarketApi
    {
        string MarketplaceId { get; }

        MarketType Market { get; }

        bool Connect();
        bool Disconnect();

        IEnumerable<DTOOrder> GetOrders(ILogService log, 
            List<string> orderIds);
        IEnumerable<DTOOrder> GetOrders(ILogService log, 
            DateTime updatedAfter, 
            List<string> orderStatusList);

        IEnumerable<ListingOrderDTO> GetOrderItems(string orderId);

        IEnumerable<ParentItemDTO> GetItems(ILogService log, 
            ITime time,
            MarketItemFilters filters,
            ItemFillMode fillMode,
            out List<string> asinsWithError);

        void FillWithAdditionalInfo(ILogService log, 
            ITime time, 
            IList<ItemDTO> items, 
            IdType idType, 
            ItemFillMode fillMode,
            out List<ItemDTO> itemsWithError);
    }
}
