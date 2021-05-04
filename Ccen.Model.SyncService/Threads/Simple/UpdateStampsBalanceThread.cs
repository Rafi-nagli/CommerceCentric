using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.General;
using Amazon.Model.Implementation;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateStampsBalanceThread : ThreadBase
    {
        public UpdateStampsBalanceThread(long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval)
            : base("UpdateStampsBalance", companyId, messageService, callbackInterval)
        {
            
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);

            var serviceFactory = new ServiceFactory();

            var log = GetLogger();
            var weightService = new WeightService();

            using (var db = dbFactory.GetRWDb())
            {
                var providers = db.ShipmentProviders.GetByCompanyId(CompanyId);
                var shipmentProviders = serviceFactory.GetShipmentProviders(log,
                    time,
                    dbFactory,
                    weightService,
                    providers,
                    null,
                    null,
                    null,
                    null);
                
                var labelService = new LabelService(shipmentProviders, log,  time, dbFactory, null, null, AddressService.Default);

                labelService.UpdateBalance(db,
                    time.GetAppNowTime());
            }
        }
    }
}
