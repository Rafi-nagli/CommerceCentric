using System;
using System.Collections.Generic;

namespace Amazon.Core.Contracts.Cache
{
    public interface IDbCacheService
    {
        IList<T> GetWithUpdates<T>(string key,
            Func<DateTime?, IList<T>> getUpdates,
            TimeSpan lifeTime) where T : IDbCacheableEntry;

        void ClearCache(string key);
    }
}
