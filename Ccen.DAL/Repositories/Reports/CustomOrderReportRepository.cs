using Amazon.Core;
using Amazon.Core.Views;
using Amazon.DAL.Repositories.Orders;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DAL.Repositories.Reports
{
    public class CustomOrderReportRepository : OrderReportRepository
    {
        public CustomOrderReportRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public override IQueryable<OrderReportItemDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
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
                ShippingDate = b.ShippingDate,
                CarrierName = b.CarrierName,
                ShippingMethodName = b.ShippingMethodName,
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
                UpChargeCost = b.UpChargeCost,
            });
        }
    }
}
