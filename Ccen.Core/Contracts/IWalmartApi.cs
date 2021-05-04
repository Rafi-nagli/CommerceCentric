using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Feeds;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts
{
    public interface IWalmartApi : IMarketApi
    {
        CallResult<DTOOrder> AcknowledgingOrder(string orderId);

        CallResult<DTOOrder> CancelOrder(string orderId,
            IList<DTOOrderItem> items);

        CallResult<DTOOrder> RefundOrder(string orderId,
            ReturnOrderInput refundInfo);

        DTOOrder GetOrder(ILogService log, string orderId);

        CallResult<DTOOrder> SubmitTrackingInfo(string orderId,
            string trackingNumber,
            string trackinUrl,
            string serviceName,
            ShippingTypeCode shippingType,
            string carrier,
            DateTime shipDate,
            IList<OrderItemDTO> items);

        string GetItemsReport(string outputDirectory);

        CallResult<string> SubmitInventoryFeed(string requetId,
            IList<ItemDTO> items,
            string feedBaseDirectory);
        CallResult<string> SubmitPriceFeed(string requetId,
            IList<ItemDTO> items,
            string feedBaseDirectory);

        IList<ItemDTO> GetAllItems();

        CallResult<IList<WalmartFeedItemDTO>> GetFeedItems(string feedId);
        CallResult<FeedDTO> GetFeed(string feedId);
        CallResult<FeedDTO> SubmitItemsFeed(string requestId,
            IList<ItemExDTO> items,
            IList<ParentItemDTO> allParentItems,
            IList<StyleEntireDto> allStyles,
            IList<StyleImageDTO> allStyleImages,
            IList<FeatureValueDTO> allFeatures,
            string feedBaseDirectory,
            out IList<FeedMessageDTO> messages);

        CallResult<ItemDTO> SendPrice(ItemDTO item);
        CallResult<ItemDTO> SendPromotion(ItemDTO item);
        CallResult<string> RetireItem(string sku);
    }
}
