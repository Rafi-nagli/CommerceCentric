using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Caches;
using Amazon.DTO.Caches;

namespace Amazon.Core.Contracts.Db.Cache
{
    public interface IListingCacheRepository : IBaseCacheRepository<ListingCache, ListingCacheDTO>
    {

    }
}
