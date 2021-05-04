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
    public interface IMarketOrderUpdaterApi
    {
        MarketType Market { get; }
        string MarketplaceId { get; }

        CallResult<ShippingDTO> SubmitTrackingInfo(string orderId,
                   string trackingNumber,
                   string trackinUrl,
                   ShippingMethodDTO shippingMethod,
                   string serviceName,
                   ShippingTypeCode shippingType,
                   string carrier,
                   DateTime shipDate,
                   IList<OrderItemDTO> items,
                   string tag);
    }
}
