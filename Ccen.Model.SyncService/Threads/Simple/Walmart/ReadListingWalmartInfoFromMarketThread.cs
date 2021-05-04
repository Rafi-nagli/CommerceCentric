using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Walmart
{
    public class ReadListingWalmartInfoFromMarketThread : ThreadBase
    {
        private readonly IWalmartApi _api;
        private TimeSpan _betweenProcessingInverval;

        public ReadListingWalmartInfoFromMarketThread(IWalmartApi api, 
            long userId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("ReadListing" + api.Market + "InfoFromMarket", userId, messageService, callbackInterval)
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
            var systemAction = new SystemActionService(log, time);
            var itemHistoryService = new ItemHistoryService(log, time, dbFactory);
            var lastSyncDate = settings.GetListingsReadDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            var walmartReader = new WalmartListingInfoReader(GetLogger(), 
                time, 
                _api, 
                dbFactory, 
                systemAction,
                itemHistoryService,
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                walmartReader.UpdateListingInfo();
                walmartReader.ReadListingInventory();

                walmartReader.ResetQtyForNotExistListings();
                walmartReader.RetireNotExistListings();
                
                settings.SetListingsReadDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
