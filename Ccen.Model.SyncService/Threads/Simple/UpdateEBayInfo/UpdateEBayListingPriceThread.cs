using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Web.Models;
using eBay.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateEBayInfo
{
    public class UpdateEBayListingPriceThread : ThreadBase
    {
        private readonly eBayApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateEBayListingPriceThread(eBayApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateEBay" + api.MarketplaceId + "ListingPrice", companyId, messageService, callbackInterval)
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
            var styleHistoryService = new StyleHistoryService(log, time, dbFactory);
            var styleManager = new StyleManager(log, time, styleHistoryService);

            var lastSyncDate = settings.GetListingsPriceSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var sync = new eBayItemsSync(log,
                    time,
                    _api, 
                    dbFactory, 
                    styleManager,
                    null,
                    null,
                    null);
                sync.SendPriceUpdates();
                settings.SetListingsPriceSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
