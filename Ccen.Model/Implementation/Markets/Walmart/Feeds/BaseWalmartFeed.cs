using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DTO.Listings;
using Walmart.Api;
using Walmart.Api.Core.Models;


namespace Amazon.Model.Implementation.Markets.Walmart.Feeds
{
    public abstract class BaseWalmartFeed
    {
        public abstract int FeedType { get;  }

        public MarketType Market
        {
            get { return _api.Market; }
        }

        public string MarketplaceId
        {
            get { return null; }
        }

        protected IDbFactory _dbFactory;
        protected ILogService _log;
        protected IWalmartApi _api;
        protected ITime _time;

        public BaseWalmartFeed(ILogService log,
            IDbFactory dbFactory,
            IWalmartApi api,
            ITime time)
        {
            _log = log;
            _time = time;
            _api = api;
            _dbFactory = dbFactory;
        }

        protected FeedDTO GetExistFeed()
        {
            FeedDTO currentFeed = null;
            using (var db = _dbFactory.GetRWDb())
            {
                currentFeed = db.Feeds.GetUnprocessedFeed(FeedType, Market, MarketplaceId);
                if (currentFeed != null
                    && String.IsNullOrEmpty(currentFeed.AmazonIdentifier))
                    currentFeed = null;
            }

            return currentFeed;
        }

        protected void UpdateFeedStatus(IUnitOfWork db, 
            FeedDTO currentFeed, 
            IList<WalmartFeedItemDTO> feedItems,
            TimeSpan waitTimeOfProcessing)
        {
            var thresholdDate = waitTimeOfProcessing == TimeSpan.Zero ? _time.GetAppNowTime().AddHours(-8) : _time.GetAppNowTime().Subtract(waitTimeOfProcessing);
            _log.Info("thresholdDate=" + thresholdDate);

            if (feedItems.All(i => WalmartUtils.ConvertFromFeedItemPublishedStatusToStandard(i.Status)
                                 != PublishedStatuses.PublishedInProgress)
                     || currentFeed.SubmitDate < thresholdDate)
            {
                var feed = db.Feeds.GetAll().FirstOrDefault(f => f.Id == currentFeed.Id);
                if (feed != null)
                {
                    feed.Status = (int) FeedStatus.Processed;
                    db.Commit();

                    currentFeed.Status = (int)FeedStatus.Processed;

                    _log.Info(String.Format("Mark feed, id={0}, marketId={1}, as processed", feed.Id, feed.AmazonIdentifier));
                }
            }
        }

        protected abstract void ProcessFeedItem(IUnitOfWork db, WalmartFeedItemDTO feedItem, DateTime feedSubmitDate);

        public FeedDTO CheckFeedStatus(TimeSpan waitTimeOfProcessing)
        {
            var currentFeed = GetExistFeed();
            if (currentFeed == null)
                return null;
            //if (waitTimeOfProcessing != TimeSpan.Zero && currentFeed.SubmitDate < _time.GetAppNowTime().Subtract(waitTimeOfProcessing))
            //    return null;

            var feedStatusUpdated = false;
            var feedItemsResult = _api.GetFeedItems(currentFeed.AmazonIdentifier);
            if (feedItemsResult.IsSuccess)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    foreach (var item in feedItemsResult.Data)
                    {
                        _log.Info("Item status: itemId=" + item.ItemId + ", status=" + item.Status);
                        item.FeedId = currentFeed.Id;

                        ProcessFeedItem(db, item, currentFeed.SubmitDate);
                    }

                    db.Commit();

                    UpdateFeedStatus(db, currentFeed, feedItemsResult.Data, waitTimeOfProcessing);
                    feedStatusUpdated = true;
                }
            }

            if (!feedStatusUpdated)
            {
                var feedResult = _api.GetFeed(currentFeed.AmazonIdentifier);
                if (feedResult.IsSuccess)
                {
                    if (feedResult.Data.Status == (int) WalmartFeedStatus.ERROR)
                    {
                        _log.Info("Feed Status=ERROR, FeedId=" + currentFeed.AmazonIdentifier);
                        using (var db = _dbFactory.GetRWDb())
                        {
                            UpdateFeedStatus(db, currentFeed, new List<WalmartFeedItemDTO>(), waitTimeOfProcessing);
                        }
                    }
                }
            }

            return currentFeed;
        }
    }
}
