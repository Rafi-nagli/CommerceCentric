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
    public class WalmartPromotionFeed : BaseWalmartFeed, IFeed
    {
        public string _feedBaseDirectory;

        public override int FeedType
        {
            get { return (int) WalmartReportType.Promotions; }  
        }

        public WalmartPromotionFeed(ILogService log,
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

        public void SubmitFeed(IList<string> asinList)
        {
            var today = _time.GetAppNowTime().Date;

            using (var db = _dbFactory.GetRWDb())
            {
                IList<ItemDTO> dtoListings;

                if (asinList == null || !asinList.Any())
                {
                    var listingQuery = from l in db.Listings.GetAll()
                        join i in db.Items.GetAll() on l.ItemId equals i.Id
                        join s in db.Styles.GetAll() on i.StyleId equals s.Id
                        join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId
                            into withSale
                        from sale in withSale.DefaultIfEmpty()
                        where l.PriceUpdateRequested
                              && (i.ItemPublishedStatus != (int)PublishedStatuses.New
                                || i.ItemPublishedStatus != (int)PublishedStatuses.None
                                || i.ItemPublishedStatus != (int)PublishedStatuses.PublishingErrors)
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
                            SaleStartDate = sale != null ? sale.SaleStartDate : null,
                            SaleEndDate = sale != null ? sale.SaleEndDate : null,
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
                                             && asinList.Contains(l.SKU)
                                       select new ItemDTO()
                                       {
                                           ListingEntityId = l.Id,
                                           SKU = l.SKU,
                                           StyleId = i.StyleId,
                                           CurrentPrice = l.CurrentPrice,
                                           ListPrice = s.MSRP,
                                           SalePrice = sale != null && sale.SaleStartDate <= today ? sale.SalePrice : null,
                                           SaleStartDate = sale != null ? sale.SaleStartDate : null,
                                           SaleEndDate = sale != null ? sale.SaleEndDate : null,
                                       };

                    dtoListings = listingQuery.ToList();
                }

                if (dtoListings.Any())
                {
                    _log.Info("Listings to submit=" + String.Join(", ", dtoListings.Select(i => i.SKU).ToList()));

                    var items = dtoListings.ToList();

                    foreach (var item in items)
                    {
                        if (item.StyleId.HasValue)
                        {
                            var itemStyle = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(
                                item.StyleId.Value,
                                StyleFeatureHelper.ITEMSTYLE);

                            if (!item.ListPrice.HasValue && itemStyle != null)
                            {
                                item.ListPrice = PriceHelper.GetDefaultMSRP(itemStyle.Value);
                            }
                        }

                        _log.Info(String.Format("Send SKU={0}, price={1}, msrp={2}, salePrice={3}", 
                            item.SKU,
                            item.CurrentPrice, 
                            item.ListPrice, 
                            item.SalePrice));

                        var result = _api.SendPromotion(item);
                        if (result.IsSuccess)
                        {
                            var dbListing = db.Listings.GetAll().FirstOrDefault(i => i.Id == item.ListingEntityId);
                            if (dbListing != null)
                            {
                                dbListing.PriceUpdateRequested = false;
                                db.Commit();
                            }
                        }
                    }

                    //var newFeed = new Feed()
                    //{
                    //    Market = (int)Market,
                    //    MarketplaceId = MarketplaceId,
                    //    Type = (int)FeedType,
                    //    IsProcessed = false,
                    //    SubmitDate = _time.GetAppNowTime()
                    //};
                    //db.Feeds.Add(newFeed);
                    //db.Commit();

                    //_log.Info("Feed id=" + newFeed.Id);

                    //var submitResult = _api.SubmitPriceFeed(newFeed.Id.ToString(),
                    //    items,
                    //    _feedBaseDirectory);

                    //if (submitResult.IsSuccess)
                    //{
                    //    _log.Info("Walmart feed id=" + submitResult.Data);

                    //    newFeed.AmazonIdentifier = submitResult.Data;
                    //    db.Commit();

                    //    _log.Info("Feed submitted, feedId=" + newFeed.AmazonIdentifier);
                    //}
                    //else
                    //{
                    //    _log.Info("Feed DIDN'T submitted, mark feed as deleted");

                    //    newFeed.Deleted = true;
                    //    db.Commit();
                    //}
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
