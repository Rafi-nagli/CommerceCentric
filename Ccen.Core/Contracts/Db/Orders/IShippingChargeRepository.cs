using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IShippingChargeRepository : IRepository<ShippingCharge>
    {
        IQueryable<ShippingChargeDTO> GetAllAsDto();
    }
}
