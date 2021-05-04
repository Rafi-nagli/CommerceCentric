using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Features;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db
{
    public interface IFeatureRepository : IRepository<Feature>
    {
        IList<FeatureDTO> GetByItemType(int itemTypeId);
        IQueryable<FeatureDTO> GetAllAsDto();
    }
}
