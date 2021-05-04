using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IRefundApi
    {
        MarketType Market { get; }
        string MarketplaceId { get; }

        CallResult<DTOOrder> RefundOrder(string orderId, ReturnOrderInput data);
    }
}
