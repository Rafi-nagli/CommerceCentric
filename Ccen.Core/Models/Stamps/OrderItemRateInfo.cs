using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Stamps
{
    public class OrderItemRateInfo
    {
        public string ItemOrderId { get; set; }
        public int ReplaceType { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public decimal ItemPrice { get; set; }
        public string ShippingSize { get; set; }
        public ItemStyleType ItemStyle { get; set; }

        
        //Additional
        public bool InPackage { get; set; }
        public IList<OrderItemRateInfo> LinkedOrderItems { get; set; }


        public OrderItemRateInfo()
        {
        }

        public OrderItemRateInfo(string id, 
            int replaceType,
            double weight, 
            decimal? packageWidth,
            decimal? packageHeight,
            decimal? packageLength,
            int quantity, 
            decimal itemPrice, 
            string shippingSize,
            ItemStyleType itemStyle,
            IList<OrderItemRateInfo> linkedOrderItems)
        {
            ItemOrderId = id;
            ReplaceType = replaceType;
            Weight = weight;
            PackageWidth = packageWidth;
            PackageHeight = packageHeight;
            PackageLength = packageLength;
            Quantity = quantity;
            ItemPrice = itemPrice;
            ShippingSize = shippingSize;
            ItemStyle = itemStyle;
            LinkedOrderItems = linkedOrderItems;
        }


        public OrderItemRateInfo Clone()
        {
            return new OrderItemRateInfo(ItemOrderId,
                ReplaceType,
                Weight,
                PackageWidth,
                PackageHeight,
                PackageLength,
                Quantity,
                ItemPrice,
                ShippingSize,
                ItemStyle,
                LinkedOrderItems);
        }
    }
}
