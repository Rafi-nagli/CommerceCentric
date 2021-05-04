using System;
using Amazon.Api;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Amazon.Feeds;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo
{
    public class UpdateCancellationDataThread : ThreadBase
    {
        private readonly AmazonApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateCancellationDataThread(AmazonApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateCancellationData", companyId, messageService, callbackInterval)
        {
            LogWrite("callbackInterval=" + callbackInterval);
            LogWrite("betweenProcessingInverval=" + betweenProcessingInverval);
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var settings = new SettingsService(dbFactory);
            var actionService = new SystemActionService(GetLogger(), time);

            var lastSyncDate = settings.GetOrdersCancellationDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var updater = new AcknowledgementDataUpdater(actionService,
                    log, 
                    time,
                    dbFactory);
                var unprocessedFeed = updater.GetUnprocessedFeedId(_api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    LogWrite("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(_api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                }
                else
                {
                    LogWrite("Submit new feed");
                    updater.SubmitFeed(_api, CompanyId, null, AppSettings.FulfillmentRequestDirectory);
                    settings.SetOrdersCancellationtDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}