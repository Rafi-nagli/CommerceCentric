using System;
using System.Collections.Generic;
using Amazon.Api;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo
{
    public class AmazonReadPriceThread : TimerThreadBase
    {
        private AmazonApi _api;

        public AmazonReadPriceThread(long companyId, ISystemMessageService messageService, AmazonApi api, IList<TimeSpan> callTimeStamps, ITime time)
            : base("AmazonReadPrice", companyId, messageService, callTimeStamps, time)
        {
            _api = api;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var settings = new SettingsService(dbFactory);
            var ratingUpdater = new RatingUpdater(log, time);

            using (var db = dbFactory.GetRWDb())
            {
                try
                {
                    log.Info("Start update lowest prices");
                    //ratingUpdater.UpdateLowestPrice(_api, db);

                    log.Info("Start update myprice");
                    ratingUpdater.UpdateMyPrice(_api, db);

                    //NOTE: No needed, already updated when sync lisings
                    //log.Info("Start update rating");
                    //ratingUpdater.UpdateRatingByProductApi(_api, db);
                    
                    log.Info("End process");
                }
                catch (Exception ex)
                {
                    log.Error("Error when processing items", ex);
                }
                settings.SetRankSyncDate(DateTime.UtcNow, _api.Market, _api.MarketplaceId);
            }
        }
    }
}
