using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Walmart
{
    public class UpdateWalmartListingQtyThread : ThreadBase
    {
        private readonly IWalmartApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateWalmartListingQtyThread(IWalmartApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("Update" + api.Market + "ListingQty", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);

            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetListingsQuantityToAmazonSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate + ", _betweenProcessingInverval=" + _betweenProcessingInverval);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var feed = new WalmartInventoryFeed(GetLogger(), time, _api, dbFactory,
                    AppSettings.WalmartFeedBaseDirectory);

                var feedDto = feed.CheckFeedStatus(TimeSpan.FromHours(1));

                if (feedDto == null) //NOTE: no feed to check
                {
                    feed.SubmitFeed();                    
                }

                settings.SetListingsQuantityToAmazonSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
