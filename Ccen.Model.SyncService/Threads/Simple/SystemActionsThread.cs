using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.General.Services;
using Amazon.Model.Implementation;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class SystemActionsThread : ThreadBase
    {
        public SystemActionsThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("SystemActions", companyId, messageService, callbackInterval)
        {

        }

        protected override void Init()
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            CompanyDTO company;
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var weightService = new WeightService();
            var messageService = new SystemMessageService(log, time, dbFactory);

            var serviceFactory = new ServiceFactory();
            var quantityManager = new QuantityManager(log, time);
            var rateProviders = serviceFactory.GetShipmentProviders(log,
                    time,
                    dbFactory,
                    weightService,
                    company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

            var actionService = new SystemActionService(log, time);

            var commentService = new OrderCommentService(dbFactory, 
                log, 
                time, 
                actionService);
            commentService.ProcessSystemAction();

            var rateService = new RateService(dbFactory, 
                log, 
                time, 
                weightService,
                messageService,
                company, 
                actionService,
                rateProviders);
            rateService.ProcessSystemAction(CancellationToken);


            var quantityDistribution = new QuantityDistributionService(dbFactory,
                quantityManager,
                log,
                time,
                QuantityDistributionHelper.GetDistributionMarkets(),
                DistributeMode.None);
            quantityDistribution.ProcessSystemAction(actionService);
        }
    }
}
