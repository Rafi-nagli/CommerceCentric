using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Rates;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Rates;

namespace Amazon.Core.Contracts.Db
{
    public interface IDhlGroundZipCodeRepository : IRepository<DhlGroundZipCode>
    {
        IQueryable<ZipCodeZoneDTO> GetAllAsDto();
    }
}
