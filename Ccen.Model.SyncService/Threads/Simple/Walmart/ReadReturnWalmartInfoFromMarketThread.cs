using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Web.Models;
using Ccen.Model.SyncService;
using Walmart.Api;

namespace Amazon.Model.SyncService.Threads.Simple.Walmart
{
    public class ReadReturnWalmartInfoFromMarketThread : ThreadBase
    {
        private readonly IWalmartApi _api;
        private TimeSpan _betweenProcessingInverval;

        public ReadReturnWalmartInfoFromMarketThread(IWalmartApi api, 
            long userId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("ReadReturn" + api.Market + "InfoFromMarket", userId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);

            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetReturnsDataReadDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            var walmartReader = new WalmartReturnInfoReader(GetLogger(), 
                time, 
                (WalmartApi)_api, 
                dbFactory, 
                AppSettings.WalmartReportBaseDirectory,
                AppSettings.WalmartFeedBaseDirectory);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                walmartReader.UpdateReturnInfo();

                settings.SetReturnsDataReadDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
