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
    public class UpdateListingsImageOnAmazonThread : ThreadBase
    {
        private readonly AmazonApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateListingsImageOnAmazonThread(string logTag,
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

            var lastSyncDate = settings.GetListingsImageSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate + ", between=" + _betweenProcessingInverval);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var imageUpdater = new ItemImageUpdater(log, time, dbFactory);
                var unprocessedFeed = imageUpdater.GetUnprocessedFeedId(_api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    LogWrite("Update unprocessed image feed");
                    imageUpdater.UpdateSubmittedFeed(_api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                }
                else
                {
                    LogWrite("Submit new image feed");
                    imageUpdater.SubmitFeed(_api, CompanyId, null, AppSettings.FulfillmentRequestDirectory);
                    settings.SetListingsImageSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}