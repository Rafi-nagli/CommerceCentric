using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.Model.Implementation.Emails;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemRankViewModel
    {
        public string ASIN { get; set; }
        public int? Rank { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public IList<ItemRankClosestListingViewModel> ClosestListings { get; set; }


        public ItemRankViewModel()
        {
            
        }

        public static ItemRankViewModel GetRank(IMarketApi api,
            IUnitOfWork db,
            ILogService log,
            ITime time,
            string asin)
        {
            var model = new ItemRankViewModel();
            var soldFrom = time.GetAppNowTime().Date.AddDays(-7);

            asin = StringHelper.TrimWhitespace(asin);
            model.ASIN = asin;

            var asinsWithError = new List<string>();
            var parentItems = api.GetItems(log, time, MarketItemFilters.Build(new List<string>() { asin }), ItemFillMode.NoAdv, out asinsWithError)
                .Where(i => i.IsAmazonUpdated == true)
                .ToList();
            if (parentItems.Any())
            {
                var parentItem = parentItems.First();
                if (!String.IsNullOrEmpty(parentItem.TempParentASIN))
                    parentItems = api.GetItems(log, time, MarketItemFilters.Build(new List<string>() { parentItem.TempParentASIN }), ItemFillMode.NoAdv, out asinsWithError)
                        .Where(i => i.IsAmazonUpdated == true)
                        .ToList();

                if (parentItems.Any())
                {
                    model.Rank = parentItems.First().Rank;
                    model.ClosestListings = GetClosestListings(db, model.Rank, soldFrom);
                    return model;
                }
            }
            throw new Exception("Not found");
        }

        private static IList<ItemRankClosestListingViewModel> GetClosestListings(IUnitOfWork db, 
            int? rank,
            DateTime soldFrom)
        {
            var results = new List<ItemRankClosestListingViewModel>();
            if (!rank.HasValue)
                return results;
            
            var upperListingsQuery = db.Listings.GetViewListings()
                .Where(r => r.Rank.HasValue 
                        && !r.IsRemoved
                        && r.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                        && r.Rank >= rank.Value)
                .OrderBy(r => r.Rank)
                .Take(11)
                .ToList();

            var upperListings = upperListingsQuery.ToList()
                .Select(l => new ItemRankClosestListingViewModel()
                {
                    ListingId = l.Id,
                    SKU = l.SKU,
                    ASIN = l.ASIN,
                    StyleId = l.StyleId,
                    StyleString = l.StyleString,
                    StyleSize = l.StyleSize,
                    Market = l.Market,
                    MarketplaceId = l.MarketplaceId,
                    Rank = l.Rank,
                    RankUpdateDate = l.RankUpdateDate,
                }).ToList();

            var lowerListingsQuery = db.Listings.GetViewListings()
                .Where(r => r.Rank.HasValue
                        && !r.IsRemoved
                        && r.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                        && r.Rank < rank.Value)
                .OrderByDescending(r => r.Rank)
                .Take(10)
                .ToList();

            var lowerListings = lowerListingsQuery.ToList()
                .Select(l => new ItemRankClosestListingViewModel()
                {
                    ListingId = l.Id,
                    SKU = l.SKU,
                    ASIN = l.ASIN,
                    StyleId = l.StyleId,
                    StyleString = l.StyleString,
                    StyleSize = l.StyleSize,
                    Market = l.Market,
                    MarketplaceId = l.MarketplaceId,
                    Rank = l.Rank,
                    RankUpdateDate = l.RankUpdateDate,
                }).ToList();

            results = upperListings;
            results.AddRange(lowerListings);

            var listingIds = results.Select(l => l.ListingId).ToList();
            var soldUnitsQuery = from oi in db.OrderItems.GetAllAsDto()
                join o in db.Orders.GetAll() on oi.OrderId equals o.Id
                where listingIds.Contains(oi.ListingId)
                      && o.FulfillmentChannel == "MFN"
                      && o.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                      && o.OrderStatus == OrderStatusEnumEx.Shipped
                      && o.OrderDate >= soldFrom
                group oi by oi.ListingId
                into byListing
                select new
                {
                    ListingId = byListing.Key,
                    SoldUnits = byListing.Sum(i => i.QuantityOrdered)
                };

            var soldUnits = soldUnitsQuery.ToList();

            foreach (var soldUnit in soldUnits)
            {
                var item = results.FirstOrDefault(r => r.ListingId == soldUnit.ListingId);
                if (item != null)
                {
                    item.SoldUnits = soldUnit.SoldUnits;
                }
            }


            return results.OrderByDescending(r => r.Rank)
                .ThenBy(r => r.StyleString)
                .ThenBy(r => SizeHelper.GetSizeIndex(r.StyleSize))
                .ToList();
        }
    }
}