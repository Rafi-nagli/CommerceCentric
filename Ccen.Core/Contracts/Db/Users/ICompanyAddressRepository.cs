using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Db
{
    public interface ICompanyAddressRepository : IRepository<CompanyAddress>
    {
        IQueryable<CompanyAddressDTO> GetAllAsDto();
        IList<CompanyAddressDTO> GetByCompanyId(long companyId);
    }
}
