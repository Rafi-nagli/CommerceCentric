using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Caches;
using Amazon.DTO.Caches;

namespace Amazon.Core.Contracts.Db.Cache
{
    public interface IStyleItemCacheRepository : IBaseCacheRepository<StyleItemCache, StyleItemCacheDTO>
    {
        IQueryable<StyleItemCacheDTO> GetAllAsDto();
        IList<StyleItemCacheDTO> GetForStyleId(long styleId);
        IList<StyleItemCacheDTO> GetForStyleItemId(long styleItemId);
    }
}
