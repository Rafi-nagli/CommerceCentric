using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Web.Models;
using Ccen.Model.SyncService;
using eBay.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateEBayInfo
{
    public class ReadListingEBayInfoFromMarketThread : ThreadBase
    {
        private readonly eBayApi _api;
        private TimeSpan _betweenProcessingInverval;

        public ReadListingEBayInfoFromMarketThread(eBayApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("ReadListing" + api.Market + api.MarketplaceId + "InfoFromMarket", companyId, messageService, callbackInterval)
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


            var lastSyncDate = settings.GetListingsReadDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            var sync = new eBayItemsSync(log,
                    time,
                    _api,
                    dbFactory,
                    styleManager,
                    AppSettings.eBayImageDirectory,
                    AppSettings.eBayImageBaseUrl,
                    AppSettings.LabelDirectory);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                sync.ReadItemsInfo();

                settings.SetListingsReadDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
