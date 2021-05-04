using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Web.Models;
using Jet.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateJetInfo
{
    public class UpdateJetOrderAcknowledgmentDataThread : ThreadBase
    {
        private readonly JetApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateJetOrderAcknowledgmentDataThread(JetApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateJetOrderAcknowledgment", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);


            var lastSyncDate = settings.GetOrdersAcknowledgementDate(_api.Market, _api.MarketplaceId);

            using (var db = dbFactory.GetRWDb())
            {
                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new JetOrderAcknowledgement(_api, GetLogger(), time);
                    updater.UpdateOrders(db);
                    settings.SetOrdersAcknowledgementDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
