using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class RestartServiceThread : ThreadBase
    {
        public RestartServiceThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("RestartService", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);

            var log = GetLogger();
            var settings = new SettingsService(dbFactory);
            var actionService = new SystemActionService(GetLogger(), time);
            var quantityManager = new QuantityManager(GetLogger(), time);
            var cache = new CacheService(GetLogger(), time, actionService, quantityManager);

            var manageService = new ManageService(log, dbFactory, actionService);

            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();
                
                //Process cache update actions
                manageService.ProcessRestartActions();
            }
        }
    }
}
