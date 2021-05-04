using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.User
{
    public class UserOrderApi : IMarketApi
    {
        public string MarketplaceId => MarketplaceKeeper.ManuallyCreated;

        public MarketType Market => MarketType.OfflineOrders;

        public IList<DTOOrder> _orders = new List<DTOOrder>();

        public UserOrderApi(IList<DTOOrder> createdOrders)
        {
            _orders = createdOrders;
        }

        public bool Connect()
        {
            return true;
        }

        public bool Disconnect()
        {
            return true;
        }

        public void FillWithAdditionalInfo(ILogService log, ITime time, IList<ItemDTO> items, IdType idType, ItemFillMode fillMode, out List<ItemDTO> itemsWithError)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ParentItemDTO> GetItems(ILogService log, ITime time, MarketItemFilters filters, ItemFillMode fillMode, out List<string> asinsWithError)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ListingOrderDTO> GetOrderItems(string orderId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DTOOrder> GetOrders(ILogService log, List<string> orderIds)
        {
            return _orders.Where(o => orderIds.Contains(o.OrderId)).ToList();
        }

        public IEnumerable<DTOOrder> GetOrders(ILogService log, DateTime updatedAfter, List<string> orderStatusList)
        {
            return _orders;
        }
    }
}
