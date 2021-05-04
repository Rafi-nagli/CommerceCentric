using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db.Orders
{
    public interface IShipmentReportRepository
    {
        IQueryable<ShipmentReportItemDTO> GetAllAsDto();
    }
}
