
using System.Collections.Generic;
using System.Linq;

namespace Amazon.DTO
{
    public class DTOOrderItem
    {
        public long? OrderId { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }
        public string Title { get; set; }
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public int Quantity { get; set; }
        public decimal ShippingPrice { get; set; }

        public decimal ShippingPaid { get; set; }
        public decimal ShippingTax { get; set; }

        public string ItemPriceCurrency { get; set; }
        /// <summary>
        /// Total price include quantity multiplier
        /// </summary>
        public decimal ItemPrice { get; set; }

        public decimal? ItemPaid { get; set; }

        public decimal PricePerItem
        {
            get
            {
                if (Quantity > 0)
                    return ItemPrice/Quantity;
                return ItemPrice;
            }
        }

        /// <summary>
        /// Weight per item
        /// </summary>
        public double Weight { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public string StyleId { get; set; }
        
        public int ReplaceType { get; set; }
        public string ItemOrderId { get; set; }
        public long OrderItemEntityId { get; set; }

        public string SourceItemOrderId { get; set; }

        public string ListingId { get; set; }
        public long? ListingEntityId { get; set; }
        public string SKU { get; set; }
        public string SourceMarketId { get; set; }

        public long? StyleEntityId { get; set; }
        public long? StyleItemId { get; set; }

        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public string StyleImage { get; set; }

        public string Picture { get; set; }
        public string ItemPicture { get; set; }

        public string ItemStyle { get; set; }


        //public IList<ItemDTO> AvailableSizes { get; set; }

        public IList<StyleLocationDTO> Locations { get; set; }

        private StyleLocationDTO DefaultLocation
        {
            get
            {
                return Locations != null && Locations.Any() ? Locations.OrderByDescending(l => l.IsDefault).First() : null;
            }
        }

        public int SortIsle { get { return DefaultLocation != null ? DefaultLocation.SortIsle : int.MaxValue; } }
        public int SortSection { get { return DefaultLocation != null ? DefaultLocation.SortSection : int.MaxValue; } }
        public int SortShelf { get { return DefaultLocation != null ? DefaultLocation.SortShelf : int.MaxValue; } }

        public int QuantityShipped { get; set; }

        public override string ToString()
        {
            return "Picture=" + Picture +
                   ", Size=" + Size +
                   ", Color=" + Color +
                   ", StyleId=" + StyleId +
                   ", StyleItemId=" + StyleItemId +
                   ", Title=" + Title +
                   ", ASIN=" + ASIN +
                   ", ParentASIN=" + ParentASIN +
                   ", Quantity=" + Quantity +
                   ", ShippingPrice=" + ShippingPrice +
                   ", ItemPrice=" + ItemPrice +
                   ", Weight=" + Weight +
                   ", ItemOrderId=" + ItemOrderId +
                   ", SourceItemOrderId=" + SourceItemOrderId +
                   ", ReplaceType=" + ReplaceType +
                   ", ListingId=" + ListingId +
                   ", QuantityShipped=" + QuantityShipped;
        }
    }
}