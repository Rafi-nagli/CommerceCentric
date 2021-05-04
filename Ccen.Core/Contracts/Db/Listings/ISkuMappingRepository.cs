using System.Linq;
using Amazon.Core.Entities.Listings;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db.Listings
{
    public interface ISkuMappingRepository : IRepository<SkuMapping>
    {
        IQueryable<SkuMappingDTO> GetAllAsDto();
    }
}
