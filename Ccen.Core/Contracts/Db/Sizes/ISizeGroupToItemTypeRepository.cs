using System.Collections.Generic;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface ISizeGroupToItemTypeRepository : IRepository<SizeGroupToItemType>
    {
        SizeGroupToItemType GetBySizeGroupIdAndItemTypeId(int sizeGroupId, int itemTypeId);
        IList<SizeGroupToItemType> GetByItemTypeId(int itemTypeId);
        IList<SizeGroupToItemType> GetBySizeGroupId(int sizeGroupId);
    }
}
