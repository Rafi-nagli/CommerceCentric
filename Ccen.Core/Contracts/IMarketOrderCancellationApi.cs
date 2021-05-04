using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts
{
    public interface IMarketOrderCancellationApi
    {
        MarketType Market { get; }
        string MarketplaceId { get; }

        CallResult<DTOOrder> CancelOrder(string orderString,
                   IList<DTOOrderItem> items);
    }
}
