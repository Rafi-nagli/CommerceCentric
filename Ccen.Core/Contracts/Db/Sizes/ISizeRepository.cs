using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface ISizeRepository : IRepository<Size>
    {
        IList<SizeDTO> GetAllAsDto();
        IQueryable<SizeDTO> GetAllWithGroupAsDto();
        IList<SizeDTO> GetAllWithGroupByItemTypeAsDto(int? itemTypeId);
        IList<SizeDTO> GetSizesForGroup(int groupId);
        bool IsSize(string postfix);
        IList<SizeGroupToItemType> GetItemTypeBySizeGroups(IList<int> groupIdList);
    }
}
