using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IZipCodeZoneRepository : IRepository<ZipCodeZone>
    {
        IQueryable<ZipCodeZoneRangeDTO> GetAllAsDto();
    }
}
