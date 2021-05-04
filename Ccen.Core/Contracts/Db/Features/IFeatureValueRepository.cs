using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Features;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db
{
    public interface IFeatureValueRepository : IRepository<FeatureValue>
    {
        IList<FeatureValueDTO> GetValuesByFeatureId(int featureId);
        FeatureValueDTO GetValueByStyleAndFeatureId(long styleId, int featureId);
        IList<FeatureValueDTO> GetAllFeatureValueByItemType(int itemTypeId);
        IList<FeatureValueDTO> GetValuesByStyleIds(IList<long> styleIdList);
        IQueryable<FeatureValueDTO> GetAllAsDto();
    }
}
