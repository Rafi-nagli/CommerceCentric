using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateRecountingThread : ThreadBase
    {
        public UpdateRecountingThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateRecounting", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var inventorization = new RecountingService(GetLogger(),
                time,
                dbFactory);
            
            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();
                
                inventorization.ProcessStatuses();

                log.Info("Reinventorization was processed");
            }
        }
    }
}
