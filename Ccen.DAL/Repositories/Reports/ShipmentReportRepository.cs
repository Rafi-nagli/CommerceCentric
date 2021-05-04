using Amazon.Core;
using Amazon.Core.Contracts.Db.Orders;
using Amazon.Core.Views;
using Amazon.DAL;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DAL.Repositories.Reports
{
    public class ShipmentReportRepository : Repository<ViewShipmentReport>, IShipmentReportRepository
    {
        public ShipmentReportRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<ShipmentReportItemDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<ShipmentReportItemDTO> AsDto(IQueryable<ViewShipmentReport> query)
        {
            return query.Select(b => new ShipmentReportItemDTO()
            {
                Id = b.Id,
                Market = b.Market,
                MarketplaceId = b.MarketplaceId,
                AmazonIdentifier = b.AmazonIdentifier,
                CustomerOrderId = b.CustomerOrderId,
                MarketOrderId = b.MarketOrderId,
                QuantityOrdered = b.QuantityOrdered,
                ShippingDate = b.ShippingDate,
                ShippingCost = b.ShippingCost,
                UpChargeCost = b.UpChargeCost,
            });
        }
    }
}
