using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Mailing
{
    public class MailItemViewModel
    {
        public string Size { get; set; }
        public string ASIN { get; set; }

        public int Quantity { get; set; }
        public decimal ShippingPrice { get; set; }

        public string ItemPriceCurrency { get; set; }
        /// <summary>
        /// Total price include quantity multiplier
        /// </summary>
        public decimal ItemPrice { get; set; }

        public decimal PricePerItem
        {
            get
            {
                if (Quantity > 0)
                    return ItemPrice / Quantity;
                return ItemPrice;
            }
        }

        public int ReplaceType { get; set; }
        public string ItemOrderId { get; set; }


        /// <summary>
        /// Weight per item
        /// </summary>
        public double Weight { get; set; }
        public string StyleString { get; set; }
        public long? StyleId { get; set; }

        public long? StyleItemId { get; set; }

        public string StyleSize { get; set; }

        public string Picture { get; set; }

        public DTOOrderItem GetItemDto()
        {
            return new DTOOrderItem()
            {
                ASIN = ASIN,
                ItemOrderId = ItemOrderId,
                ItemPriceCurrency = ItemPriceCurrency,
                ItemPrice = ItemPrice,
                Weight = Weight,
                Quantity = Quantity,
                StyleItemId = StyleItemId,
                StyleId = StyleString,
                StyleEntityId = StyleId,
            };
        }


        public MailItemViewModel()
        {
            
        }

        public MailItemViewModel(DTOOrderItem item)
        {
            ASIN = item.ASIN;
            ItemOrderId = item.ItemOrderId;
            ItemPriceCurrency = item.ItemPriceCurrency;
            ItemPrice = item.ItemPrice;
            Weight = item.Weight;
            Quantity = item.Quantity;
            StyleItemId = item.StyleItemId;
            StyleString = item.StyleId;
            StyleId = item.StyleEntityId;
        }

        public override string ToString()
        {
            return "Picture=" + Picture +
                   ", Size=" + Size +
                   ", StyleId=" + StyleId +
                   ", StyleItemId=" + StyleItemId +
                   ", ASIN=" + ASIN +
                   ", Quantity=" + Quantity +
                   ", ShippingPrice=" + ShippingPrice +
                   ", ItemPrice=" + ItemPrice +
                   ", Weight=" + Weight +
                   ", Quantity=" + Quantity;
        }
    }
}