using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Db
{
    public interface IAddressProviderRepository : IRepository<AddressProvider>
    {
        IQueryable<AddressProviderDTO> GetAllAsDto();
        IList<AddressProviderDTO> GetByCompanyId(long companyId);
    }
}
