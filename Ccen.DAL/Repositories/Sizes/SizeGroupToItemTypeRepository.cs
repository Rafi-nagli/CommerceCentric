using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;

namespace Amazon.DAL.Repositories.Sizes
{
    public class SizeGroupToItemTypeRepository : Repository<SizeGroupToItemType>, ISizeGroupToItemTypeRepository
    {
        public SizeGroupToItemTypeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public SizeGroupToItemType GetBySizeGroupIdAndItemTypeId(int sizeGroupId, int itemTypeId)
        {
            return unitOfWork.GetSet<SizeGroupToItemType>().FirstOrDefault(s => s.SizeGroupId == sizeGroupId && s.ItemTypeId == itemTypeId);
        }

        public IList<SizeGroupToItemType> GetByItemTypeId(int itemTypeId)
        {
            return unitOfWork.GetSet<SizeGroupToItemType>().Where(s => s.ItemTypeId == itemTypeId).ToList();
        }

        public IList<SizeGroupToItemType> GetBySizeGroupId(int sizeGroupId)
        {
            return unitOfWork.GetSet<SizeGroupToItemType>().Where(s => s.SizeGroupId == sizeGroupId).ToList();
        }
    }
}
