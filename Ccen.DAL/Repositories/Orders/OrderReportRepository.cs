using Amazon.Core;
using Amazon.Core.Contracts.Db.Orders;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.DAL.Repositories.Orders
{
    public class OrderReportRepository : Repository<ViewOrderReport>, IOrderReportRepository
    {
        public OrderReportRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public virtual IQueryable<OrderReportItemDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IQueryable<OrderReportItemDTO> GetAllForNetSalesAsDto()
        {
            var query = GetAll().Where(
                        i => i.OrderStatus != OrderStatusEnumEx.Pending && i.OrderStatus != OrderStatusEnumEx.Canceled);
            return AsDto(query);
        }

        private IQueryable<OrderReportItemDTO> AsDto(IQueryable<ViewOrderReport> query)
        {
            return query.Select(b => new OrderReportItemDTO()
            {
                Id = b.Id,
                Market = b.Market,
                MarketplaceId = b.MarketplaceId,
                DropShipperId = b.DropShipperId,
                DropShipperName = b.DropShipperName,
                OrderDate = b.OrderDate,
                OrderDateDate = b.OrderDateDate,
                AmazonIdentifier = b.AmazonIdentifier,
                CustomerOrderId = b.CustomerOrderId,
                MarketOrderId = b.MarketOrderId,
                DSOrderId = b.DSOrderId,
                TrackingNumber = b.TrackingNumber,
                PersonName = b.PersonName,
                ShippingCity = b.ShippingCity,
                ShippingState = b.ShippingState,
                QuantityOrdered = b.QuantityOrdered,
                Model = b.Model,
                StyleId = b.StyleId,
                StyleItemId = b.StyleItemId,
                ItemPaid = b.ItemPaid,
                ShippingPaid = b.ShippingPaid,
                Cost = b.Cost,
                ItemTax = b.ItemTax,
                ShippingTax = b.ShippingTax,
                OrderStatus = b.OrderStatus,
                ItemRefunded = b.ItemRefunded,
                ShippingRefunded = b.ShippingRefunded,
                ItemProfit = b.ItemProfit,
                ShippingCost = b.ShippingCost,
            });
        }
    }
}
