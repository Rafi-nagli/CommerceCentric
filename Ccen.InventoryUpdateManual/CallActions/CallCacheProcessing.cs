using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.DAL;
using Amazon.Model.Implementation;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallCacheProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IQuantityManager _quantityManager;

        public CallCacheProcessing(ILogService log, 
            ITime time,
            IQuantityManager quantityManager)
        {
            _log = log;
            _time = time;
            _quantityManager = quantityManager;
        }

        public void CallUpdateRefundAmounts()
        {
            using (var db = new UnitOfWork(_log))
            {
                db.DisableValidation();

                new CacheService(_log, _time, null, _quantityManager).UpdateProcessedRefunds(db, false);
            }
        }

        public void CallUpdateCache()
        {
            using (var db = new UnitOfWork(_log))
            {
                db.DisableValidation();

                new CacheService(_log, _time, null, _quantityManager).UpdateDbCache(db);
            }
        }

        public void CallUpdateStyleItemIdCache(long styleId)
        {
            var cacheService = new CacheService(_log, _time, null, _quantityManager);
            using (var db = new UnitOfWork(_log))
            {
                db.DisableValidation();

                cacheService.UpdateStyleItemCacheForStyleId(db, 
                    new List<long>() { styleId });
            }
        }

        public void CallUpdateStyleIdCache(long styleId)
        {
            var cacheService = new CacheService(_log, _time, null, _quantityManager);
            using (var db = new UnitOfWork(_log))
            {
                db.DisableValidation();

                cacheService.UpdateStyleCacheForStyleId(db,
                    new List<long>() { styleId });
            }
        }
    }
}
