using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts.Db.Cache
{
    public interface IBaseCacheRepository<TDb, TDto> : IRepository<TDb> where TDb:class
    {
        EntityUpdateStatus<long> UpdateCacheItem(TDto cache);
        IList<EntityUpdateStatus<long>> RefreshCacheItems(IList<TDto> caches, DateTime when);
        IQueryable<TDto> GetAllCacheItems();
    }
}
