using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.Core.Contracts.Db
{
    public interface IPackingSlipSizeMappingRepository : IRepository<PackingSlipSizeMapping>
    {
        IQueryable<PackingSlipSizeMappingDTO> GetAllAsDto();
    }
}
