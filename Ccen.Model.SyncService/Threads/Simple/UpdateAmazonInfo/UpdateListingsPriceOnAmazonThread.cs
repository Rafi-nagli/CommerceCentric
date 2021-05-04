using System;
using Amazon.Api;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Amazon.Feeds;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo
{
    public class UpdateListingsPriceOnAmazonThread : ThreadBase
    {
        private readonly AmazonApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateListingsPriceOnAmazonThread(string logTag,
            AmazonApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base(logTag, companyId, messageService, callbackInterval)
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
                
            var lastSyncDate = settings.GetListingsPriceSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var priceUpdater = new ItemPriceUpdater(log, time, dbFactory);
                var unprocessedFeed = priceUpdater.GetUnprocessedFeedId(_api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    LogWrite("Update unprocessed price feed");
                    priceUpdater.UpdateSubmittedFeed(_api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                }
                else
                {
                    LogWrite("Submit new price feed");
                    priceUpdater.SubmitFeed(_api, CompanyId, null, AppSettings.FulfillmentRequestDirectory);
                    settings.SetListingsPriceSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}