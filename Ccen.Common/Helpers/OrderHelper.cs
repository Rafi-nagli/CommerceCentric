using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.Utils;

namespace Amazon.Common.Helpers
{
    public class OrderHelper
    {
        public static string FormatOrderNumber(string orderNumber, MarketType market)
        {
            if (String.IsNullOrEmpty(orderNumber))
                return orderNumber;

            if (market == MarketType.Walmart)
            {
                if (orderNumber.Length > 9)
                    return orderNumber.Insert(3, "-").Insert(7, "-").Insert(11, "-");
            }

            if (market == MarketType.WalmartCA)
            {
                if (orderNumber.Length > 9)
                    return orderNumber.Insert(3, "-").Insert(7, "-").Insert(11, "-");
            }

            if (market == MarketType.Shopify)
            {
                return orderNumber?.Trim('#');
            }

            return orderNumber;
        }

        public static string FormatDisplayOrderNumber(string orderNumber, MarketType market)
        {
            var formatted = new string(orderNumber.Where(char.IsDigit).ToArray());
            if (market == MarketType.Shopify)
            {
                return formatted;
            }
            if (market == MarketType.OverStock)
            {
                return formatted;
            }

            if (formatted.Length >= 3)
            {
                formatted = Regex.Replace(formatted, "(.{3})(.*)", "$1" + "-" + "$2");
            }
            if (formatted.Length >= 11)
            {
                formatted = Regex.Replace(formatted, "(.{11})(.*)", "$1" + "-" + "$2");
            }
            return formatted;
        }

        public static string RemoveOrderNumberFormat(string orderNumber)
        {
            if (String.IsNullOrEmpty(orderNumber))
                return orderNumber;

            if (orderNumber.Count(ch => ch == '-') == 3)
                return orderNumber.Replace("-", "");

            return orderNumber;
        }


        public static IList<OrderItemRateInfo> BuildAndGroupOrderItems(IList<ListingOrderDTO> orderItems)
        {
            return orderItems.Select(i => new OrderItemRateInfo()
            {
                ItemOrderId = i.ItemOrderId,
                ReplaceType = i.ReplaceType,
                Weight = i.Weight ?? 0,                
                PackageLength = i.PackageLength,
                PackageWidth = i.PackageWidth,
                PackageHeight = i.PackageHeight,                
                Quantity = i.QuantityOrdered,
                ItemPrice = ShippingUtils.GetItemPrice(i),
                ShippingSize = i.ShippingSize,
                ItemStyle = ItemStyleHelper.GetFromItemStyleOrTitle(i.ItemStyle, i.Title),
            }).ToList();
        }

        public static string GetSourceOrderItemId(string orderItemId)
        {
            return orderItemId.Split("_".ToCharArray())[0];
        }

        public static void PrepareSourceItemOrderId(IList<DTOOrderItem> items)
        {
            foreach (var item in items)
            {
                if (String.IsNullOrEmpty(item.SourceItemOrderId))
                    item.SourceItemOrderId = item.ItemOrderId;
            }
        }

        public static void PrepareSourceItemOrderId(IList<OrderItemDTO> items)
        {
            foreach (var item in items)
            {
                if (String.IsNullOrEmpty(item.SourceItemOrderIdentifier))
                    item.SourceItemOrderIdentifier = item.ItemOrderIdentifier;
            }
        }
        
        public static List<DTOOrderItem> GroupBySourceItemOrderId(IList<DTOOrderItem> items)
        {
            var results = new List<DTOOrderItem>();
            foreach (var item in items)
            {
                var existItem = results.FirstOrDefault(i => i.SourceItemOrderId == item.SourceItemOrderId);
                if (existItem == null)
                {
                    results.Add(new DTOOrderItem()
                    {
                        OrderId = item.OrderId,
                        Quantity = item.Quantity,
                        ItemOrderId = item.ItemOrderId,
                        SourceItemOrderId = item.SourceItemOrderId,
                        SourceMarketId = item.SourceMarketId,
                        ListingId = item.ListingId,
                    });
                }
                else
                {
                    //NOTE: with linked style we dosn't sum quantity!
                    existItem.Quantity += item.ReplaceType == (int)ItemReplaceTypes.Combined 
                        //|| item.ItemOrderId == existItem.ItemOrderId //TEMP: excluded, impossible equal ItemOrderId in one shipping DB restriction
                        ? 0 : item.Quantity;
                    //existItem.ItemPrice += item.ItemPrice;
                    //existItem.ShippingPrice += item.ShippingPrice;
                }
            }
            return results;
        }

        public static List<OrderItemRateInfo> GroupBySourceItemOrderId(IList<OrderItemRateInfo> items)
        {
            var results = new List<OrderItemRateInfo>();
            foreach (var item in items)
            {
                var existItem = results.FirstOrDefault(i => OrderHelper.GetSourceOrderItemId(i.ItemOrderId) == OrderHelper.GetSourceOrderItemId(item.ItemOrderId));
                if (existItem == null)
                {
                    results.Add(new OrderItemRateInfo()
                    {
                        ItemOrderId = item.ItemOrderId,
                        Quantity = item.Quantity
                    });
                }
                else
                {
                    //NOTE: with linked style we dosn't sum quantity!
                    existItem.Quantity += item.ReplaceType == (int)ItemReplaceTypes.Combined ? 0 : item.Quantity;
                }
            }
            return results;
        }

        public static List<OrderItemDTO> GroupBySourceItemOrderId(IList<OrderItemDTO> items)
        {
            var results = new List<OrderItemDTO>();
            foreach (var item in items)
            {
                var existItem = results.FirstOrDefault(i => i.SourceItemOrderIdentifier == item.SourceItemOrderIdentifier);
                if (existItem == null)
                {
                    results.Add(new OrderItemDTO()
                    {
                        ItemOrderIdentifier = item.ItemOrderIdentifier,
                        SourceItemOrderIdentifier = item.SourceItemOrderIdentifier,
                        QuantityOrdered = item.QuantityOrdered,
                        SKU = item.SKU,
                    });
                }
                else
                {
                    //NOTE: with linked style we dosn't sum quantity
                    existItem.QuantityOrdered += item.ReplaceType == (int)ItemReplaceTypes.Combined
                        //|| item.ItemOrderIdentifier == existItem.ItemOrderIdentifier //TEMP: excluded, impossible equal ItemOrderId in one shipping DB restriction
                        ? 0 : item.QuantityOrdered;
                }
            }
            return results;
        }

        public static bool AcceptReturnRequest(DateTime orderDate, 
            DateTime? estDeliveryDate,
            DateTime? deliveryDate,
            DateTime requestDate,
            DateTime now)
        {
            var nov1 = now.Month == 1 ? new DateTime(now.Year - 1, 11, 1) : new DateTime(now.Year, 11, 1);
            var jan31 = now.Month == 1 ? new DateTime(now.Year, 1, 31) : new DateTime(now.Year + 1, 1, 31);

            if (orderDate >= nov1
                && orderDate <= jan31
                && requestDate <= jan31)
            {
                return false;
            }
            else
            {
                return (deliveryDate ?? estDeliveryDate ?? orderDate).AddDays(30) < requestDate;
            }
        }
    }
}
