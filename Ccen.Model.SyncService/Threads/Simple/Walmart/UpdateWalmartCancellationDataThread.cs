using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple.Walmart
{
    public class UpdateWalmartCancellationDataThread : ThreadBase
    {
        private readonly IWalmartApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateWalmartCancellationDataThread(IWalmartApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("Update" + api.Market + "CancellationData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);

            var actionService = new SystemActionService(GetLogger(), time);

            var lastSyncDate = settings.GetOrdersCancellationDate(_api.Market, _api.MarketplaceId);

            using (var db = dbFactory.GetRWDb())
            {
                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new WalmartOrderCancellation(_api, actionService, GetLogger(), time);
                    updater.ProcessCancellations(db);
                    settings.SetOrdersCancellationtDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
