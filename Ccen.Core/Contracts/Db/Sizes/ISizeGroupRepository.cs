using System.Collections.Generic;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface ISizeGroupRepository : IRepository<SizeGroup>
    {
        IList<SizeGroupDTO> GetAllAsDto();
        IList<SizeGroupDTO> GetByItemType(int itemTypeId);
    }
}
