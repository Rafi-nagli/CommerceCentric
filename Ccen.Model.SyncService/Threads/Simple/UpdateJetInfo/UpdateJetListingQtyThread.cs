using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Web.Models;
using Ccen.Model.SyncService;
using Jet.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateJetInfo
{
    public class UpdateJetListingQtyThread : ThreadBase
    {
        private readonly JetApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateJetListingQtyThread(JetApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateJetListingQty", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetListingsQuantityToAmazonSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var sync = new JetItemsSync(log,
                    time,
                    _api,
                    dbFactory,
                    AppSettings.JetImageDirectory,
                    AppSettings.JetImageBaseUrl);

                sync.SendInventoryUpdates();

                settings.SetListingsQuantityToAmazonSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
