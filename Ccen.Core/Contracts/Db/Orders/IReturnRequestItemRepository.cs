using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models.Stamps;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IReturnRequestItemRepository : IRepository<ReturnRequestItem>
    {
    }
}
