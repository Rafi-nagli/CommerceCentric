using System.Collections.Generic;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IFeatureToItemTypeRepository : IRepository<FeatureToItemType>
    {
        FeatureToItemType GetByFeatureIdAndItemTypeId(int featureId, int itemTypeId);
    }
}
