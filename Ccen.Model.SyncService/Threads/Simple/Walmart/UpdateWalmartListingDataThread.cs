using System;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Walmart
{
    public class UpdateWalmartListingDataThread : ThreadBase
    {
        private readonly IWalmartApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateWalmartListingDataThread(IWalmartApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("Update" + api.Market + "ListingData", companyId, messageService, callbackInterval)
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
            var htmlScraper = new HtmlScraperService(log, time, dbFactory);
            var imageManager = new ImageManager(log, htmlScraper, dbFactory, time);

            var lastSyncDate = settings.GetListingsSendDate(_api.Market, _api.MarketplaceId);
            var pauseStatus = settings.GetListingsSyncPause(_api.Market, _api.MarketplaceId) ?? false;

            LogWrite("Last sync date=" + lastSyncDate);

            if (pauseStatus)
            {
                LogWrite("Listings sync in pause");
                return;
            }
            
            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var feed = new WalmartItemsFeed(GetLogger(), 
                    time, 
                    _api, 
                    dbFactory, 
                    AppSettings.WalmartFeedBaseDirectory,
                    AppSettings.SwatchImageDirectory,
                    AppSettings.SwatchImageBaseUrl,
                    AppSettings.WalmartImageDirectory,
                    AppSettings.WalmartImageBaseUrl);

                var feedDto = feed.CheckFeedStatus(TimeSpan.FromHours(24));

                if (feedDto == null) //NOTE: no feed to check
                {
                    //NOTE: Update Image Types, to get right image
                    imageManager.UpdateStyleImageTypes();
                    
                    feed.SubmitFeed();
                    settings.SetListingsSendDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
