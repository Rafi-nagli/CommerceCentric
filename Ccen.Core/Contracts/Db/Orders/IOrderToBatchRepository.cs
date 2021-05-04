using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderToBatchRepository : IRepository<OrderToBatch>
    {
        IQueryable<OrderToBatchDTO> GetViewAllAsDto();
        IQueryable<OrderToBatchDTO> GetAllAsDto();
    }
}
