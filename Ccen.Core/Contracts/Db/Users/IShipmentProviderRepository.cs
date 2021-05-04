using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Db
{
    public interface IShipmentProviderRepository : IRepository<ShipmentProvider>
    {
        IQueryable<ShipmentProviderDTO> GetAllAsDto();
        IList<ShipmentProviderDTO> GetByCompanyId(long companyId);
        void UpdateBalance(long providerId, decimal newBalance, DateTime? when);
    }
}
