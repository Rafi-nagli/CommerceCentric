using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Amazon.Api.Feeds;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DTO.Listings;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class ItemPriceUpdater : BaseFeedUpdater
    {
        protected IList<Listing> Listings { get; set; }

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.Price; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PRODUCT_PRICING_DATA_"; }
        }

        public ItemPriceUpdater(ILogService log, 
            ITime time,
            IDbFactory dbFactory) : base(log, time, dbFactory)
        {
            
        }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db, 
            long companyId, 
            MarketType market, 
            string marketplaceId,
            IList<string> asinList)
        {
            Log.Info("Get listings for price update");
            
            if (asinList == null || !asinList.Any())
            {
                Listings = db.Listings.GetPriceUpdateRequiredList(market, marketplaceId);
            }
            else
            {
                Listings = db.Listings.GetAll().Where(l => asinList.Contains(l.SKU)
                        && l.Market == (int)market
                        && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                    .ToList();
            }


            if (Listings.Any())
            {
                var listingIdList = Listings.Select(l => l.Id).Distinct().ToList();
                var saleList = db.StyleItemSaleToListings.GetAllListingSaleAsDTO().Where(s => listingIdList.Contains(s.ListingId)).ToList();
                var itemIdList = Listings.Select(l => l.ItemId).Distinct().ToList();
                var items = db.Items.GetAll().Where(i => itemIdList.Contains(i.Id)).ToList();
                var styleItemIdList = items.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId).Distinct().ToList();
                var styleItems = db.StyleItems.GetAll().Where(si => styleItemIdList.Contains(si.Id)).ToList();

                var priceMessages = new List<XmlElement>();
                var index = 0;

                var priceToSend = new Dictionary<string, decimal>();

                foreach (var listing in Listings)
                {
                    if (priceToSend.ContainsKey(listing.SKU))
                        continue;

                    var existAutoPricing = false;
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    decimal? minAllowedPrice = null;
                    decimal? maxAllowedPrice = null;
                    if (item != null && item.StyleItemId.HasValue)
                    {
                        var styleItem = styleItems.FirstOrDefault(si => si.Id == item.StyleItemId.Value);
                        minAllowedPrice = styleItem.MinPrice;
                        maxAllowedPrice = styleItem.MaxPrice;
                        existAutoPricing = styleItem.MinPrice > 0 && styleItem.MaxPrice > 0;
                    }

                    index++;
                    Log.Info("add listing, index=" + index + ", listing=" + listing);

                    ViewListingSaleDTO sale = saleList.FirstOrDefault(s => s.ListingId == listing.Id);
                    if (sale != null)
                    {
                        sale.SaleStartDate = sale.SaleStartDate ?? sale.ListingSaleCreateDate;
                        sale.SaleEndDate = sale.SaleEndDate ?? Time.GetAppNowTime().Add(SaleManager.DefaultSalesPeriod);
                        Log.Info("sale: salePrice=" + sale.SalePrice
                                 + ", maxPiecesOnSale=" + sale.MaxPiecesOnSale
                                 + ", currentPiecesSaled=" + sale.PiecesSoldOnSale
                                 + ", saleStartDate=" + sale.SaleStartDate
                                 + ", saleEndDate=" + sale.SaleEndDate);
                    }

                    var now = Time.GetAppNowTime();
                    
                    var saleOn = sale != null 
                        && sale.SaleStartDate <= now
                        && (sale.SalePrice.HasValue || sale.SalePercent.HasValue);

                    var businessPrice = listing.AutoAdjustedPrice ?? listing.CurrentPrice;
                    var currentPrice = listing.AutoAdjustedPrice ?? listing.CurrentPrice;

                    decimal? salePrice = null;
                    if (sale != null && sale.SalePrice.HasValue)
                        salePrice = sale.SalePrice.Value;
                    if (sale != null && sale.SalePercent.HasValue)
                        salePrice = currentPrice - currentPrice * sale.SalePercent.Value/(decimal) 100;

                    if (saleOn)
                        businessPrice = salePrice.Value;
                    Log.Info("Current price=" + listing.CurrentPrice + ", Business price=" + businessPrice + ", Adjusted price=" + listing.AutoAdjustedPrice);

                    if (saleOn)
                    {
                        if (sale.SalePrice > currentPrice)
                        {
                            Log.Info("SalePrice > CurrentPrice, new currentPrice=" + currentPrice + " => " + salePrice + ", sale is OFF");

                            currentPrice = salePrice.Value;
                            saleOn = false;
                            sale = null;
                        }
                    }

                    priceMessages.Add(FeedHelper.ComposePriceMessage(index, 
                        listing.SKU, 
                        currentPrice,
                        businessPrice,
                        null,
                        sale != null ? salePrice : (decimal?)null,
                        sale != null ? sale.SaleStartDate : (DateTime?)null,
                        sale != null ? sale.SaleEndDate : (DateTime?)null,
                        minAllowedPrice,
                        maxAllowedPrice,
                        market,
                        marketplaceId));

                    listing.MessageIdentifier = index;

                    priceToSend.Add(listing.SKU, listing.CurrentPrice);
                }
                Log.Info("Compose feed");
                var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
                var document = FeedHelper.ComposeFeed(priceMessages, merchant, Type.ToString());
                return new DocumentInfo
                {
                    XmlDocument = document,
                    NodesCount = index
                };
            }
            return null;
        }

        protected override void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId)
        {
            foreach (var listing in Listings)
            {
                listing.PriceUpdateRequested = false;
                listing.PriceFeedId = feedId;
            }
        }

        protected override void UpdateEntitiesAfterResponse(long feedId, 
            IList<FeedResultMessage> errorList)
        {
            using (var db = DbFactory.GetRWDb())
            {
                var listings = db.Listings.GetFiltered(l => l.PriceFeedId == feedId).ToList();
                if (errorList.Any())
                {
                    foreach (var listing in listings)
                    {
                        if (errorList.Any(e => e.MessageId == listing.MessageIdentifier))
                        {
                            listing.PriceFeedId = null;
                            listing.PriceUpdateRequested = true;

                            Log.Warn("Listing NOT updated, listingId=" + listing.Id);
                        }
                        else
                        {
                            listing.LastPriceUpdatedOnMarket = Time.GetAppNowTime();
                        }
                    }
                }
                else
                {

                }
                db.Commit();
            }
        }
    }
}
