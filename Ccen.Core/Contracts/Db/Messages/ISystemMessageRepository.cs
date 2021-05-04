using Amazon.Core.Entities;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts.Db.Orders
{
    public interface ISystemMessageRepository : IRepository<SystemMessage>
    {
        IQueryable<SystemMessageDTO> GetAllAsDto();
    }
}
