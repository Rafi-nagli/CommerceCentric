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
    public class UpdateListingsQtyOnAmazonThread : ThreadBase
    {
        private readonly AmazonApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateListingsQtyOnAmazonThread(string tag,
            AmazonApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? interval,
            TimeSpan betweenProcessingInverval)
            : base(tag, companyId, messageService, interval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var fulfillmentResponseDirectory = AppSettings.FulfillmentResponseDirectory;
            var fulfillmentRequestDirectory = AppSettings.FulfillmentRequestDirectory;

            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetListingsQuantityToAmazonSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var quantityUpdater = new ItemQuantityUpdater(GetLogger(), time, dbFactory, AppSettings.AmazonFulfillmentLatencyDays);
                var unprocessedFeed = quantityUpdater.GetUnprocessedFeedId(_api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    LogWrite("Update unprocessed quantity feed");
                    quantityUpdater.UpdateSubmittedFeed(_api,
                        unprocessedFeed,
                        fulfillmentResponseDirectory);
                }
                else
                {
                    LogWrite("Submit new quantity feed");
                    quantityUpdater.SubmitFeed(_api, CompanyId, null, fulfillmentRequestDirectory);
                    settings.SetListingsQuantityToAmazonSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
