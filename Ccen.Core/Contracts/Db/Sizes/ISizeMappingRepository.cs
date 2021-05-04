using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.Core.Contracts.Db
{
    public interface ISizeMappingRepository : IRepository<SizeMapping>
    {
        IList<SizeDTO> GetStyleSizesByItemSize(string itemSize, int itemTypeId);
        bool IsSize(string postfix);

        IQueryable<SizeMappingDTO> GetAllAsDto();
        IList<SizeMappingDTO> GetExists(string styleSize, string itemSize);
    }
}
