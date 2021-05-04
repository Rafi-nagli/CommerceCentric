using System;
using System.Collections.Generic;
using Amazon.DTO.Orders;

namespace Amazon.DTO
{
    public class ShippingDTO
    {
        public long Id { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public bool IsFromMailPage { get; set; }
        public bool IsFulfilled { get; set; }
        public bool LabelCanceled { get; set; }

        public long OrderId { get; set; }
        public string AmazonIdentifier { get; set; }
        public string MarketOrderId { get; set; }
        public string TrackingNumber { get; set; }
        public int MessageIdentifier { get; set; }
        public int ShippingMethodId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public DateTime ShippingDate { get; set; }

        public string ShippingService { get; set; }

        public string SourceShippingService { get; set; }
        public string ShippingCountry { get; set; }
        public string CustomCurrier { get; set; }
        public string CustomShippingMethodName { get; set; }

        public ShippingMethodDTO ShippingMethod { get; set; }

        public IList<OrderItemDTO> Items { get; set; }

        public ShippingDTO Clone()
        {
            return new ShippingDTO()
            {
                Id = Id,
                Market = Market,
                MarketplaceId = MarketplaceId,

                IsFromMailPage = IsFromMailPage,
                OrderId = OrderId,
                AmazonIdentifier = AmazonIdentifier,
                MarketOrderId = MarketOrderId,
                TrackingNumber = TrackingNumber,
                MessageIdentifier = MessageIdentifier,
                ShippingMethodId = ShippingMethodId,
                OrderDate = OrderDate,
                OrderStatus = OrderStatus,
                ShippingDate = ShippingDate,

                ShippingService = ShippingService,

                ShippingMethod = ShippingMethod,

                Items = Items,
            };
        }
    }
}
