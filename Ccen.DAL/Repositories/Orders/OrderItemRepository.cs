using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Orders;


namespace Amazon.DAL.Repositories
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public void StoreOrderItems(long orderId, 
            IList<ListingOrderDTO> items, 
            DateTime when)
        {
            foreach (var item in items)
            {
                var dbItem = new OrderItem()
                {
                    OrderId = orderId,

                    ListingId = item.Id,

                    StyleString = item.StyleID,
                    StyleId = item.StyleId,
                    StyleItemId = item.StyleItemId,

                    ReplaceType = item.ReplaceType,

                    ItemOrderIdentifier = item.ItemOrderId,
                    SourceItemOrderIdentifier = item.SourceItemOrderIdentifier,

                    RecordNumber = item.RecordNumber,

                    SourceListingId = item.SourceListingId,

                    SourceStyleString = item.SourceStyleString,
                    SourceStyleItemId = item.SourceStyleItemId,
                    SourceStyleSize = item.SourceStyleSize,
                    SourceStyleColor = item.SourceStyleColor,

                    QuantityOrdered = item.QuantityOrdered,
                    QuantityShipped = item.QuantityShipped,

                    ItemGrandPrice = item.ItemGrandPrice,

                    ItemPaid = item.ItemPaid,
                    ShippingPaid = item.ShippingPaid,
                    ItemTotal = item.ItemTotal,

                    ItemPrice = item.ItemPrice,
                    ItemTax = item.ItemTax,
                    ItemPriceCurrency = item.ItemPriceCurrency,
                    ItemPriceInUSD = item.ItemPriceInUSD,


                    PromotionDiscount = item.PromotionDiscount,
                    PromotionDiscountCurrency = item.PromotionDiscountCurrency,
                    PromotionDiscountInUSD = item.PromotionDiscountInUSD,

                    ShippingPrice = item.ShippingPrice,
                    ShippingTax = item.ShippingTax,
                    ShippingPriceCurrency = item.ShippingPriceCurrency,
                    ShippingPriceInUSD = item.ShippingPriceInUSD,

                    ShippingDiscount = item.ShippingDiscount,
                    ShippingDiscountCurrency = item.ShippingDiscountCurrency,
                    ShippingDiscountInUSD = item.ShippingDiscountInUSD,



                    CreateDate = when
                };

                unitOfWork.OrderItems.Add(dbItem);

                unitOfWork.Commit();

                item.OrderItemEntityId = dbItem.Id;
            }
        }

        public IQueryable<OrderItemDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IQueryable<ListingOrderDTO> GetAllAsListingDto()
        {
            return GetAll().Select(oi => new ListingOrderDTO()
            {
                OrderItemEntityId = oi.Id,
                OrderId = oi.OrderId,

                Id = oi.ListingId,

                StyleID = oi.StyleString,
                StyleId = oi.StyleId,
                StyleItemId = oi.StyleItemId,

                ReplaceType = oi.ReplaceType,

                ItemOrderId = oi.ItemOrderIdentifier,
                SourceItemOrderIdentifier = oi.SourceItemOrderIdentifier,

                RecordNumber = oi.RecordNumber,

                SourceListingId = oi.SourceListingId,

                SourceStyleString = oi.SourceStyleString,
                SourceStyleItemId = oi.SourceStyleItemId,
                SourceStyleSize = oi.SourceStyleSize,
                SourceStyleColor = oi.SourceStyleColor,

                QuantityOrdered = oi.QuantityOrdered,
                QuantityShipped = oi.QuantityShipped,

                ItemPaid = oi.ItemPaid,
                ShippingPaid = oi.ShippingPaid,
                ItemTotal = oi.ItemTotal,
                ItemGrandPrice = oi.ItemGrandPrice,
                ItemPrice = oi.ItemPrice,
                ItemTax = oi.ItemTax,
                ItemPriceCurrency = oi.ItemPriceCurrency,
                ItemPriceInUSD = oi.ItemPriceInUSD,


                PromotionDiscount = oi.PromotionDiscount,
                PromotionDiscountCurrency = oi.PromotionDiscountCurrency,
                PromotionDiscountInUSD = oi.PromotionDiscountInUSD,

                ShippingPrice = oi.ShippingPrice,
                ShippingTax = oi.ShippingTax,
                ShippingPriceCurrency = oi.ShippingPriceCurrency,
                ShippingPriceInUSD = oi.ShippingPriceInUSD,

                ShippingDiscount = oi.ShippingDiscount,
                ShippingDiscountCurrency = oi.ShippingDiscountCurrency,
                ShippingDiscountInUSD = oi.ShippingDiscountInUSD,
            });
        }

        public IQueryable<OrderItemDTO> GetByOrderIdAsDto(string orderNumber)
        {
            var query = from oi in GetAll()
                join o in unitOfWork.GetSet<Order>() on oi.OrderId equals o.Id
                where o.AmazonIdentifier == orderNumber
                select oi;

            return AsDto(query);
        }

        public IQueryable<OrderItemDTO> GetByOrderIdAsDto(long orderId)
        {
            return AsDto(GetAll().Where(oi => oi.OrderId == orderId));
        }

        public IQueryable<DTOOrderItem> GetWithListingInfo()
        {
            var query = from oi in GetAll()
                join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                select new DTOOrderItem()
                {
                    OrderId = oi.OrderId,
                    ItemOrderId = oi.ItemOrderIdentifier,
                    SourceItemOrderId = oi.SourceItemOrderIdentifier,
                    ReplaceType = oi.ReplaceType,
                    OrderItemEntityId = oi.Id,
                    ListingId = l.ListingId,
                    
                    Quantity = oi.QuantityOrdered,
                    Weight = l.Weight ?? 0,

                    SKU = l.SKU,
                    ASIN = l.ASIN,
                    StyleId = l.StyleString,
                    StyleEntityId = l.StyleId,
                    StyleItemId = l.StyleItemId,
                    StyleSize = l.StyleSize,
                    StyleColor = l.StyleColor,

                    ItemPrice = oi.ItemPrice,
                    ItemPaid = oi.ItemPaid,

                    ShippingPrice = oi.ShippingPrice,
                    ShippingPaid = oi.ShippingPaid ?? 0,

                    StyleImage = l.StyleImage,

                    SourceMarketId = l.SourceMarketId,
                };

            return query;
        }

        public IQueryable<OrderItemDTO> GetByShippingInfoIdAsDto(long shippingInfoId)
        {
            var query = from oi in GetAll()
                        join m in unitOfWork.GetSet<ItemOrderMapping>() on oi.Id equals m.OrderItemId
                        where m.ShippingInfoId == shippingInfoId
                        select new OrderItemDTO()
                        {
                            Id = oi.Id,
                            OrderId = oi.OrderId,
                            ItemOrderIdentifier = oi.ItemOrderIdentifier,
                            SourceItemOrderIdentifier = oi.SourceItemOrderIdentifier,
                            RecordNumber = oi.RecordNumber,
                            QuantityOrdered = m.MappedQuantity,
                            ListingId = oi.ListingId,
                            ItemPrice = oi.ItemPrice,
                            ShippingPrice = oi.ShippingPrice,
                            ReplaceType = oi.ReplaceType,
                        };

            return query;
        }

        public IQueryable<OrderItemDTO> GetWithListingInfoByShippingInfoIdAsDto(long shippingInfoId)
        {
            var query = from oi in GetAll()
                        join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                        join m in unitOfWork.GetSet<ItemOrderMapping>() on oi.Id equals m.OrderItemId
                        where m.ShippingInfoId == shippingInfoId
                        select new OrderItemDTO()
                        {
                            Id = oi.Id,
                            SKU = l.SKU,
                            OrderId = oi.OrderId,
                            ItemOrderIdentifier = oi.ItemOrderIdentifier,
                            SourceItemOrderIdentifier = oi.SourceItemOrderIdentifier,
                            RecordNumber = oi.RecordNumber,
                            QuantityOrdered = m.MappedQuantity,
                            ListingId = oi.ListingId,
                            ItemPrice = oi.ItemPrice,
                            Weight = l.Weight,
                            ShippingPrice = oi.ShippingPrice,
                            ReplaceType = oi.ReplaceType,
                        };

            return query;
        }

        public IQueryable<DTOOrderItem> GetOrderPackageSizes(long orderId)
        {
            return unitOfWork.GetSet<ViewOrderItem>().Where(x => x.OrderId == orderId).Select(x => new DTOOrderItem()
            {
                PackageWidth = x.PackageWidth,
                PackageHeight = x.PackageHeight,
                PackageLength = x.PackageLength,
                Quantity = x.QuantityOrdered,
            });
        }

        private IQueryable<OrderItemDTO> AsDto(IQueryable<OrderItem> query)
        {
            return query.Select(oi => new OrderItemDTO()
            {
                Id = oi.Id,

                OrderId = oi.OrderId,
                ItemOrderIdentifier = oi.ItemOrderIdentifier,
                SourceItemOrderIdentifier = oi.SourceItemOrderIdentifier,

                ListingId = oi.ListingId,
                StyleId = oi.StyleId,
                StyleItemId = oi.StyleItemId,
                StyleString = oi.StyleString,
                ReplaceType = oi.ReplaceType,

                SourceListingId = oi.SourceListingId,

                RecordNumber = oi.RecordNumber,

                ItemPrice = oi.ItemPrice,
                ItemPriceCurrency = oi.ItemPriceCurrency,
                ItemPriceInUSD = oi.ItemPriceInUSD,

                

                PromotionDiscount = oi.PromotionDiscount,
                PromotionDiscountCurrency = oi.PromotionDiscountCurrency,
                PromotionDiscountInUSD = oi.PromotionDiscountInUSD,

                ShippingPrice = oi.ShippingPrice,
                ShippingPriceCurrency = oi.ShippingPriceCurrency,
                ShippingPriceInUSD = oi.ShippingPriceInUSD,

                ShippingDiscount = oi.ShippingDiscount,
                ShippingDiscountCurrency = oi.ShippingDiscountCurrency,
                ShippingDiscountInUSD = oi.ShippingDiscountInUSD,


                QuantityOrdered = oi.QuantityOrdered,
                QuantityShipped = oi.QuantityShipped,
            });
        }
    }
}
