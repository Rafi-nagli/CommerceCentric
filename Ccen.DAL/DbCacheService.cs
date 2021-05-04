using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Cache;

namespace Amazon.DAL
{
    public class DbCacheService : IDbCacheService
    {
        static private IDictionary<string, CacheEntry> _cacheList = new ConcurrentDictionary<string, CacheEntry>();

        private class CacheEntry
        {
            public DateTime Date { get; set; }
            public IList<IDbCacheableEntry> Items { get; set; }
        }

        private ITime _time;
        public DbCacheService(ITime time)
        {
            _time = time;
        }

        public void ClearCache(string key)
        {
            if (_cacheList.ContainsKey(key))
                _cacheList[key] = null;
        }

        public IList<T> GetWithUpdates<T>(string key,
            Func<DateTime?, IList<T>> getUpdates,
            TimeSpan lifeTime) where T : IDbCacheableEntry
        {
            var existCache = _cacheList.ContainsKey(key) ? _cacheList[key] : null;
            DateTime? fromDate = null;
            IList<IDbCacheableEntry> items = new List<IDbCacheableEntry>();
            if (existCache != null && existCache.Date.Add(lifeTime) < _time.GetAppNowTime())
            {
                existCache = null;
            }

            if (existCache != null)
            {
                fromDate = existCache.Date;
                items = existCache.Items;
            }
            var newItems = getUpdates(fromDate);
            if (existCache == null)
            {
                existCache = new CacheEntry()
                {
                    Date = _time.GetAppNowTime(),
                    Items = newItems.Cast<IDbCacheableEntry>().ToList()
                };
                _cacheList[key] = existCache;
            }
            else
            {
                foreach (var newItem in newItems)
                {
                    var existItem = items.FirstOrDefault(i => i.Key == newItem.Key);
                    if (existItem != null)
                    {
                        if (newItem.Deleted)
                        {
                            items.Remove(existItem);
                        }
                        else
                        {
                            var index = items.IndexOf(existItem);
                            items[index] = newItem;
                        }
                    }
                    else
                    {
                        items.Add(newItem);
                    }
                }
                _cacheList[key].Date = _time.GetAppNowTime();
            }

            return _cacheList[key].Items.Cast<T>().ToList();
        }
    }
}
