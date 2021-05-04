using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderItemEditViewModel
    {
        public string ItemOrderId { get; set; }
        public string SourceItemOrderId { get; set; }
        public int Quantity { get; set; }

        public long ListingId { get; set; }
        public string ASIN { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string StyleColor { get; set; }
        public string StyleSize { get; set; }
        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public int AvailableQuantity { get; set; }

        public string SourceStyleColor { get; set; }
        public string SourceStyleSize { get; set; }
        public string SourceStyleString { get; set; }
        public long? SourceStyleItemId { get; set; }

        public long NewListingId { get; set; }

        public OrderItemEditViewModel()
        {
            
        }

        public OrderItemEditViewModel(ListingOrderDTO item)
        {
            ItemOrderId = item.ItemOrderId;
            SourceItemOrderId = item.SourceItemOrderIdentifier;

            Quantity = item.QuantityOrdered;

            ListingId = item.Id;
            ASIN = item.ASIN;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;
            StyleColor = item.StyleColor;
            StyleSize = item.StyleSize;
            AvailableQuantity = item.AvailableQuantity ?? 0;
            StyleString = item.StyleID;
            StyleId = item.StyleId;
            StyleItemId = item.StyleItemId;

            SourceStyleString = item.SourceStyleString;
            SourceStyleItemId = item.SourceStyleItemId;
            SourceStyleSize = item.SourceStyleSize;
            SourceStyleColor = item.SourceStyleColor;

            NewListingId = item.Id;
        }

        public OrderItemEditViewModel Clone()
        {
            var clone = new OrderItemEditViewModel();
            clone.ItemOrderId = ItemOrderId;
            clone.SourceItemOrderId = SourceItemOrderId;
            clone.Quantity = Quantity;

            clone.ListingId = ListingId;
            clone.ASIN = ASIN;
            clone.Market = Market;
            clone.MarketplaceId = MarketplaceId;
            clone.StyleColor = StyleColor;
            clone.StyleSize = StyleSize;
            clone.StyleString = StyleString;
            clone.StyleId = StyleId;
            clone.StyleItemId = StyleItemId;
            clone.AvailableQuantity = AvailableQuantity;

            clone.SourceStyleColor = SourceStyleColor;
            clone.SourceStyleSize = SourceStyleSize;
            clone.SourceStyleString = SourceStyleString;
            clone.SourceStyleItemId = SourceStyleItemId;

            clone.NewListingId = NewListingId;

            return clone;
        }
    }
}