using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateSalesEndThread : ThreadBase
    {
        public UpdateSalesEndThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateSalesEnd", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);

            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();

                var saleService = new SaleManager(GetLogger(), time);
                saleService.CheckSaleEndForAll(db);
            }
        }
    }
}
