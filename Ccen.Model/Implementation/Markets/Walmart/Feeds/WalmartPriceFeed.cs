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
    public class WalmartPriceFeed : BaseWalmartFeed, IFeed
    {
        public string _feedBaseDirectory;

        public override int FeedType
        {
            get { return (int) WalmartReportType.Prices; }  
        }

        public WalmartPriceFeed(ILogService log,
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
            var today = _time.GetAppNowTime().Date;

            using (var db = _dbFactory.GetRWDb())
            {
                IList<ItemDTO> dtoListings;

                if (skuList == null || !skuList.Any())
                {
                    var listingQuery = from l in db.Listings.GetAll()
                        join i in db.Items.GetAll() on l.ItemId equals i.Id
                        join s in db.Styles.GetAll() on i.StyleId equals s.Id
                        join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId
                            into withSale
                        from sale in withSale.DefaultIfEmpty()
                        where l.PriceUpdateRequested

                            && (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                || i.ItemPublishedStatus == (int)PublishedStatuses.Unpublished //NOTE: Also send to unpublished, it may auto converted to published                                               
                                || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithProductId
                                || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithSKU
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                                || ((i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges
                                    || i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited)
                                    && !String.IsNullOrEmpty(i.SourceMarketId)))

                            && i.Market == (int) Market
                            && !l.IsRemoved

                        select new ItemDTO()
                        {
                            ListingEntityId = l.Id,
                            SKU = l.SKU,
                            StyleId = i.StyleId,
                            CurrentPrice = l.CurrentPrice,
                            ListPrice = s.MSRP,
                            SalePrice = sale != null && sale.SaleStartDate <= today ? sale.SalePrice : null,
                        };

                    dtoListings = listingQuery.ToList();
                }
                else
                {
                    var listingQuery = from l in db.Listings.GetAll()
                                       join i in db.Items.GetAll() on l.ItemId equals i.Id
                                       join s in db.Styles.GetAll() on i.StyleId equals s.Id
                                       join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId
                                           into withSale
                                       from sale in withSale.DefaultIfEmpty()
                                       where i.Market == (int)Market
                                             && !l.IsRemoved
                                             && skuList.Contains(l.SKU)
                                       select new ItemDTO()
                                       {
                                           ListingEntityId = l.Id,
                                           SKU = l.SKU,
                                           StyleId = i.StyleId,
                                           CurrentPrice = l.CurrentPrice,
                                           ListPrice = s.MSRP,
                                           SalePrice = sale != null && sale.SaleStartDate <= today ? sale.SalePrice : null,
                                       };

                    dtoListings = listingQuery.ToList();
                }

                if (dtoListings.Any())
                {
                    _log.Info("Listings to submit=" + String.Join(", ", dtoListings.Select(i => i.SKU).ToList()));

                    var newFeed = new Feed()
                    {
                        Market = (int)Market,
                        MarketplaceId = MarketplaceId,
                        Type = (int)FeedType,
                        Status = (int)FeedStatus.Submitted,
                        SubmitDate = _time.GetAppNowTime()
                    };
                    db.Feeds.Add(newFeed);
                    db.Commit();

                    _log.Info("Feed id=" + newFeed.Id);

                    var items = dtoListings.ToList();

                    var submitResult = _api.SubmitPriceFeed(newFeed.Id.ToString(),
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

                        newFeed.Status = (int)FeedStatus.SubmissionFail;
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
                dbListing.PriceUpdateRequested = false;

                _log.Info("Status has been updated");
            }
        }
    }
}
