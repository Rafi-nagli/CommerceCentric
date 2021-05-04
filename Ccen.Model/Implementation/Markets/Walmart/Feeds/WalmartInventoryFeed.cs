using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart.Feeds
{
    public class WalmartInventoryFeed : BaseWalmartFeed, IFeed
    {
        public string _feedBaseDirectory;

        public override int FeedType
        {
            get { return (int) WalmartReportType.Inventory; }
        }

        public WalmartInventoryFeed(ILogService log,
            ITime time,
            IWalmartApi api,
            IDbFactory dbFactory,
            string feedBaseDirectory) : base(log, dbFactory, api, time)
        {
            _feedBaseDirectory = feedBaseDirectory;
        }

        public void SubmitFeed()
        {
            SubmitFeed(null);
        }

        public void SubmitFeed(IList<string> skuList)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                IList<Listing> dbListings = null;
                if (skuList == null)
                {
                    var listingQuery = from l in db.Listings.GetAll()
                                       join i in db.Items.GetAll() on l.ItemId equals i.Id
                                       where l.QuantityUpdateRequested
                                           && (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                               || i.ItemPublishedStatus == (int)PublishedStatuses.Unpublished //NOTE: Also send to unpublished, it may auto converted to published                                               
                                               || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithProductId
                                               || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithSKU
                                               || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                                               || ((i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges
                                                    || i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited)
                                                    && !String.IsNullOrEmpty(i.SourceMarketId)))                                           
                                           && i.Market == (int)Market
                                           && !l.IsRemoved
                                       select l;

                    dbListings = listingQuery.ToList();
                }
                else
                {
                    var listingQuery = from l in db.Listings.GetAll()
                                       where skuList.Contains(l.SKU)
                                           && l.Market == (int)Market
                                           && !l.IsRemoved
                                       select l;

                    dbListings = listingQuery.ToList();
                }

                if (dbListings.Any())
                {
                    _log.Info("Listings to submit=" + String.Join(", ", dbListings.Select(i => i.SKU).ToList()));

                    var newFeed = new Feed()
                    {
                        Market = (int) Market,
                        MarketplaceId = MarketplaceId,
                        Type = (int) FeedType,
                        Status = (int) FeedStatus.Submitted,
                        SubmitDate = _time.GetAppNowTime()
                    };
                    db.Feeds.Add(newFeed);
                    db.Commit();

                    _log.Info("Feed id=" + newFeed.Id);

                    var items = dbListings.Select(i => new ItemDTO()
                    {
                        SKU = i.SKU,
                        RealQuantity = i.RealQuantity >= 30 ? 101 : i.RealQuantity,
                    })
                    .ToList();

                    var submitResult = _api.SubmitInventoryFeed(newFeed.Id.ToString(), 
                        items,
                        _feedBaseDirectory);

                    if (submitResult.IsSuccess)
                    {
                        _log.Info("Walmart feed id=" + submitResult.Data);

                        newFeed.AmazonIdentifier = submitResult.Data;
                        db.Commit();

                        _log.Info("Feed submitted, feedId=" + newFeed.AmazonIdentifier);
                    }
                    else
                    {
                        _log.Info("Feed DIDN'T submitted, mark feed as deleted");

                        newFeed.Status = (int) FeedStatus.SubmissionFail;
                        db.Commit();
                    }
                }
            }
        }

        protected override void ProcessFeedItem(IUnitOfWork db, WalmartFeedItemDTO feedItem, DateTime feedSubmitDate)
        {
            var status = WalmartUtils.ConvertFromFeedItemPublishedStatusToStandard(feedItem.Status);
            var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == feedItem.ItemId
                && l.Market == (int)_api.Market);
            if (dbListing != null && status == PublishedStatuses.Published)
            {
                if (!dbListing.QuantityUpdateRequestedDate.HasValue
                    || dbListing.QuantityUpdateRequestedDate < feedSubmitDate)
                {
                    dbListing.QuantityUpdateRequested = false;
                }

                _log.Info("Status has been updated");
            }
        }
    }
}
