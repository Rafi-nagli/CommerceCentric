using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Orders;


namespace Amazon.DAL.Repositories
{
    public class OrderItemSourceRepository : Repository<OrderItemSource>, IOrderItemSourceRepository
    {
        public OrderItemSourceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public OrderItem CreateItemFromSourceDto(OrderItemDTO item)
        {
            return new OrderItem()
            {
                OrderId = item.OrderId,

                ListingId = item.Id,

                ItemOrderIdentifier = item.ItemOrderIdentifier,
                RecordNumber = item.RecordNumber,

                SourceListingId = item.SourceListingId,

                QuantityOrdered = item.QuantityOrdered,
                QuantityShipped = item.QuantityShipped,

                ItemPrice = item.ItemPrice,
                ItemPriceCurrency = item.ItemPriceCurrency,
                ItemPriceInUSD = item.ItemPriceInUSD,

                PromotionDiscount = item.PromotionDiscount,
                PromotionDiscountCurrency = item.PromotionDiscountCurrency,
                PromotionDiscountInUSD = item.PromotionDiscountInUSD,

                ShippingPrice = item.ShippingPrice,
                ShippingPriceCurrency = item.ShippingPriceCurrency,
                ShippingPriceInUSD = item.ShippingPriceInUSD,

                ShippingDiscount = item.ShippingDiscount,
                ShippingDiscountCurrency = item.ShippingDiscountCurrency,
                ShippingDiscountInUSD = item.ShippingDiscountInUSD,
            };
        }

        public void StoreOrderItems(long orderId, 
            IList<ListingOrderDTO> items, 
            DateTime when)
        {
            foreach (var item in items)
            {
                var dbItem = new OrderItemSource()
                {
                    OrderId = orderId,

                    ListingId = item.Id,
                    StyleId = item.StyleId,
                    StyleItemId = item.StyleItemId,
                    StyleString = item.StyleID,

                    ItemOrderIdentifier = item.ItemOrderId,
                    RecordNumber = item.RecordNumber,

                    QuantityOrdered = item.QuantityOrdered,
                    QuantityShipped = item.QuantityShipped,

                    ItemPrice = item.ItemPrice,
                    ItemTax = item.ItemTax,
                    ItemPriceCurrency = item.ItemPriceCurrency,
                    ItemPriceInUSD = item.ItemPriceInUSD,

                    //ItemTax = item.ItemTax,
                    //ItemTaxCurrency = item.ItemTaxCurrency,
                    //ItemTaxInUSD = item.ItemTaxInUSD,

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

                    //ShippingTax = item.ShippingTax,
                    //ShippingTaxCurrency = item.ShippingTaxCurrency,
                    //ShippingTaxInUSD = item.ShippingTaxInUSD,

                    //GiftWrapPrice = item.GiftWrapPrice,
                    //GiftWrapPriceCurrency = item.GiftWrapPriceCurrency,
                    //GiftWrapPriceInUSD = item.GiftWrapPriceInUSD,

                    //GiftWrapTax = item.GiftWrapTax,
                    //GiftWrapTaxCurrency = item.GiftWrapPriceCurrency,
                    //GiftWrapTaxInUSD = item.GiftWrapTaxInUSD,

                    //CODFee = item.CODFee,
                    //CODFeeCurrency = item.CODFeeCurrency,
                    //CODFeeInUSD = item.CODFeeInUSD,

                    //CODFeeDiscount = item.CODFeeDiscount,
                    //CODFeeDiscountCurrency = item.CODFeeDiscountCurrency,
                    //CODFeeDiscountInUSD = item.CODFeeDiscountInUSD,

                    //TotalFee = item.TotalFee,
                    //TotalFeeCurrency = item.TotalFeeCurrency,
                    //TotalFeeInUSD = item.TotalFeeInUSD,

                    CreateDate = when
                };

                this.Add(dbItem);

                unitOfWork.Commit();

                //item.OrderItemEntityId = dbItem.Id;
            }
        }

        public IQueryable<OrderItemDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
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

        public IQueryable<ListingOrderDTO> GetByOrderIdsAsDto(IList<long> orderIds)
        {
            return AsListingDto(GetAll().Where(oi => orderIds.Contains(oi.OrderId)));
        }

        public IQueryable<DTOOrderItem> GetWithListingInfo()
        {
            var query = from oi in GetAll()
                join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                select new DTOOrderItem()
                {
                    OrderId = oi.OrderId,
                    ItemOrderId = oi.ItemOrderIdentifier,
                    ListingId = l.ListingId,
                    SourceMarketId = l.SourceMarketId,

                    Quantity = oi.QuantityOrdered,

                    ASIN = l.ASIN,
                    StyleId = l.StyleString,
                    StyleEntityId = l.StyleId,
                    StyleItemId = l.StyleItemId,
                    StyleSize = l.StyleSize,
                };

            return query;
        }

        private IQueryable<ListingOrderDTO> AsListingDto(IQueryable<OrderItemSource> query)
        {
            return query.Select(oi => new ListingOrderDTO()
            {
                OrderItemEntityId = oi.Id,
                ItemOrderId = oi.ItemOrderIdentifier,
                OrderId = oi.OrderId,

                StyleId = oi.StyleId,
                StyleItemId = oi.StyleItemId,
                StyleID = oi.StyleString,

                ItemPrice = oi.ItemPrice,
                ItemPriceCurrency = oi.ItemPriceCurrency,
                ShippingPrice = oi.ShippingPrice,
                ShippingPriceCurrency = oi.ShippingPriceCurrency,
            });
        }

        private IQueryable<OrderItemDTO> AsDto(IQueryable<OrderItemSource> query)
        {
            return query.Select(oi => new OrderItemDTO()
            {
                Id = oi.Id,

                OrderId = oi.OrderId,
                ItemOrderIdentifier = oi.ItemOrderIdentifier,

                ListingId = oi.ListingId,
                
                StyleId = oi.StyleId,
                StyleItemId = oi.StyleItemId,
                StyleString = oi.StyleString,

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
