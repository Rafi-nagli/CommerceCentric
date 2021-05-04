using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.ImageProcessing;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateImageThread : TimerThreadBase
    {
        public UpdateImageThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("UpdateAmazonImage", companyId, messageService, callTimeStamps, time)
        {
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var htmlScraper = new HtmlScraperService(GetLogger(), time, dbFactory);
            var log = GetLogger();

            var imageManager = new ImageManager(GetLogger(), htmlScraper, dbFactory, time);
            var imageProcessingService = new ImageProcessingService(dbFactory, 
                time,
                log,
                AppSettings.WalmartImageDirectory);

            var lastSyncDate = settings.GetImageUpdateDate();

            LogWrite("Last sync date=" + lastSyncDate);

            log.Info("UpdateStyleImageTypes");
            imageManager.UpdateStyleImageTypes(); //NOTE: call always (every 5 minutes)

            //if (!lastSyncDate.HasValue ||
            //    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                log.Info("UpdateListingsLargeImages");
                imageManager.UpdateParentItemsLargeImages(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, null, null);
                //imageManager.UpdateParentItemsLargeImages(MarketType.AmazonEU, null, null);

                imageManager.UpdateItemsLargeImages(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, null, null);
                //imageManager.UpdateItemsLargeImages(MarketType.AmazonEU, null, null);

                //imageManager.UpdateItemsLargeImages(MarketType.Walmart, null, null);

                log.Info("UpdateStyleLargeImage");
                imageManager.UpdateStyleLargeImage();

                log.Info("UpdateDifferenceForAllImages");
                imageProcessingService.UpdateDifferenceForAllImages(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, null);
                //imageProcessingService.UpdateDifferenceForAllImages(MarketType.Walmart, "", null);

                log.Info("ReplaceStyleLargeImage");
                imageManager.ReplaceStyleLargeImage();

                settings.SetImageUpdateDate(time.GetUtcTime());
            }
        }
    }
}
