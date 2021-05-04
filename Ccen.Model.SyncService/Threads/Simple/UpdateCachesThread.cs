using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateCachesThread : ThreadBase
    {
        public UpdateCachesThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateCaches", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var actionService = new SystemActionService(GetLogger(), time);
            var quantityManager = new QuantityManager(GetLogger(), time);
            var cache = new CacheService(GetLogger(), time, actionService, quantityManager);
            
            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();

                var lastUpdate = settings.GetCacheUpdateDate();
                var updateInteval = settings.CacheUpdateInteval;

                if (lastUpdate + updateInteval < time.GetUtcTime())
                {
                    //Full update
                    if (cache.UpdateDbCacheUsingSettings(db, settings))
                        settings.SetCacheUpdateDate(time.GetUtcTime());
                    else
                        GetLogger().Fatal("Cache updating fails");
                }
                else
                {
                    //Process cache update actions
                    cache.ProcessUpdateCacheActions(db);
                }
            }
        }
    }
}
