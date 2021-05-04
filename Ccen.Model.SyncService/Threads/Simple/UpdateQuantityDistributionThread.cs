using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.General.Services;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateQuantityDistibutionThread : ThreadBase
    {
        public UpdateQuantityDistibutionThread(long companyId, ISystemMessageService messageService, IEmailService emailService, TimeSpan? callbackInterval = null)
            : base("UpdateQuantityDistibution", companyId, messageService, callbackInterval, emailService: emailService)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var quantityManager = new QuantityManager(GetLogger(), time);

            using (var db = dbFactory.GetRWDb())
            {
                var service = new QuantityDistributionService(dbFactory, 
                    quantityManager, 
                    GetLogger(), 
                    time,
                    QuantityDistributionHelper.GetDistributionMarkets(),
                    DistributeMode.None);
                service.Redistribute(db);

                settings.SetQuantityDistributeDate(time.GetAppNowTime());
            }
        }
    }
}
