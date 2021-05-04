using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;

namespace Amazon.DAL.Repositories.Features
{
    public class FeatureToItemTypeRepository : Repository<FeatureToItemType>, IFeatureToItemTypeRepository
    {
        public FeatureToItemTypeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public FeatureToItemType GetByFeatureIdAndItemTypeId(int featureId, int itemTypeId)
        {
            return unitOfWork.GetSet<FeatureToItemType>().FirstOrDefault(s => s.FeatureId == featureId && s.ItemTypeId == itemTypeId);
        }
    }
}
