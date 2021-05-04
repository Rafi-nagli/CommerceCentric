using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation
{
    public class PriceManager : IPriceManager
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ISettingsService _settingService;
        private ISystemActionService _actionService;

        public PriceManager(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ISystemActionService actionService,
            ISettingsService settingsService)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _settingService = settingsService;
            _actionService = actionService;
        }

        public void FixupListingPrices(IUnitOfWork db)
        {
            _log.Info("FixupListingPrices");
            var today = _time.GetAppNowTime().Date;

            var listingsWithPissibleIssue = db.Listings
                        .GetAll()
                        .Where(l => !l.IsFBA 
                            && !l.SKU.Contains("-FBA") //NOTE: ???
                            && !l.IsRemoved
                            && !l.PriceUpdateRequested
                            && l.Market != (int)MarketType.eBay
                            && l.Market != (int)MarketType.Magento
                            && l.Market != (int)MarketType.Jet
                            && l.Market != (int)MarketType.Shopify
                            && l.Market != (int)MarketType.WooCommerce
                            && l.Market != (int)MarketType.Groupon
                            && l.CurrentPrice != l.AmazonCurrentPrice
                               //|| ((l.Market == (int)MarketType.Amazon || l.Market == (int)MarketType.AmazonEU || l.Market == (int)MarketType.AmazonAU)
                               //    && (l.ListingPriceFromMarket.HasValue && l.ListingPriceFromMarket != l.CurrentPrice)))
                               )
                            
                        .ToList();

            var listingIdList = listingsWithPissibleIssue.Select(l => l.Id).Distinct().ToList();
            var listingWithSaleList = db.StyleItemSaleToListings.GetAllListingSaleAsDTO().ToList() //NOTE: request all items, after that filter them, other wise SQL Exception: The query processor ran out of internal resources and could not produce a query plan. This is a rare event and only expected for extremely complex queries or queries that reference a very large number of tables or partitions. Please simplify the query.
                .Where(s => listingIdList.Contains(s.ListingId)).ToList();
            var usSKUList = db.Listings.GetAll().Where(l => l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                                            && !l.IsFBA
                                                            && !l.IsRemoved)
                .Select(l => l.SKU)
                .ToList();

            var listingsWithIssue = new List<Listing>();
            
            foreach (var listing in listingsWithPissibleIssue)
            {
                if (listing.MarketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId
                    && usSKUList.Contains(listing.SKU))
                {
                    continue; //Skip if CA sku is copy of US sku
                }

                if (listing.MarketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId
                    && usSKUList.Contains(listing.SKU))
                {
                    continue; //Skip if CA sku is copy of US sku
                }


                var currentPrice = listing.CurrentPrice;
                decimal? salePrice = null;
                var listingWithSale = listingWithSaleList.FirstOrDefault(l => l.ListingId == listing.Id);
                if (listingWithSale != null && listingWithSale.SalePrice.HasValue && listingWithSale.SaleStartDate <= today)
                {
                    salePrice = listingWithSale.SalePrice.Value;
                }

                if (listing.Market == (int) MarketType.Amazon
                    || listing.Market == (int) MarketType.AmazonEU
                    || listing.Market == (int) MarketType.AmazonAU)
                {
                    //NOTE: AmazonCurrentPrice always == RegularPrice (w/o sale)
                    if (listing.AmazonCurrentPrice != currentPrice)
                    {
                        if (!(listing.AmazonCurrentPrice > currentPrice
                            && listing.AmazonCurrentPrice == (salePrice ?? currentPrice))) //NOTE: validation for anti-sale (when SalePrice is higher then regular)
                            listingsWithIssue.Add(listing);
                    }
                    //NOTE: disable, because ListingPriceFromMarket contains incorrect price when we have reqular SKU, FBA, FBP skus with the same ASIN
                    //else
                    //{
                    //    if (listing.ListingPriceFromMarket.HasValue 
                    //        && listing.ListingPriceFromMarket != (salePrice ?? currentPrice))
                    //    {
                    //        listingsWithIssue.Add(listing);
                    //    }
                    //}
                }
                else
                {
                    if (listing.AmazonCurrentPrice != (salePrice ?? currentPrice))
                        listingsWithIssue.Add(listing);
                }
            }

            _log.Info("Listing with price issues: " + listingsWithIssue.Count);


            var marketplaces = new List<MarketplaceName>()
            {
                new MarketplaceName()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.AmazonEU,
                    MarketplaceId = MarketplaceKeeper.AmazonUkMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.AmazonAU,
                    MarketplaceId = MarketplaceKeeper.AmazonAuMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonCaMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonMxMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.Walmart,
                },
                new MarketplaceName()
                {
                    Market = MarketType.WalmartCA,
                }
            };

            foreach (var market in marketplaces)
            {
                var marketListings = listingsWithIssue.Where(l => l.Market == (int)market.Market
                       && (String.IsNullOrEmpty(market.MarketplaceId) || l.MarketplaceId == market.MarketplaceId)).ToList();
                var listingCount = marketListings.Count();
                _log.Debug("Checking " + market.Market + "-" + market.MarketplaceId + ", count=" + listingCount);
                _settingService.SetListingsPriceAlert(listingCount, market.Market, market.MarketplaceId ?? "");
                foreach (var listing in marketListings)
                {
                    _log.Debug("Request update for: " + listing.Id + ", SKU=" + listing.SKU);
                    listing.PriceUpdateRequested = true;
                }
                db.Commit();
            }

            db.Commit();
        }

        public void FixupBusinessPrices(IUnitOfWork db)
        {
            _log.Info("FixupBusinessPrices");
            var today = _time.GetAppNowTime().Date;

            var listingsWithBusinessPrice = db.Listings.GetAll().Where(l => l.BusinessPrice.HasValue).ToList();
            var listingWithSaleList = (from s in db.StyleItemSaleToListings.GetAllListingSaleAsDTO()
                                      join l in db.Listings.GetAll() on s.ListingId equals l.Id
                                      where l.BusinessPrice.HasValue
                                      select s).ToList();

            foreach (var listing in listingsWithBusinessPrice)
            {
                var listingWithSale = listingWithSaleList.FirstOrDefault(l => l.ListingId == listing.Id);
                if (listingWithSale != null && listingWithSale.SalePrice.HasValue && listingWithSale.SaleStartDate <= today)
                {
                    if (listing.BusinessPrice != listingWithSale.SalePrice)
                    {
                        _log.Info("Business Price changed, from=" + listing.BusinessPrice + ", to=" + listingWithSale.SalePrice);
                        listing.BusinessPrice = listingWithSale.SalePrice;
                        listing.PriceUpdateRequested = true;
                    }
                }
                else
                {
                    if (listing.BusinessPrice != listing.CurrentPrice)
                    {
                        _log.Info("Business Price changed, from=" + listing.BusinessPrice + ", to=" + listing.CurrentPrice);
                        listing.BusinessPrice = listing.CurrentPrice;
                        listing.PriceUpdateRequested = true;
                    }
                }
            }

            db.Commit();
        }

        public void FixupWalmartPrices(IUnitOfWork db)
        {
            _log.Info("FixupWalmartPrices");
            var today = _time.GetAppNowTime().Date;

            var fromDate = _time.GetAppNowTime().AddHours(-8);
            var requestedActions = _actionService.GetUnprocessedByType(db, SystemActionType.ListingPriceRecalculation, fromDate, null);

            var listingIds = requestedActions.Select(i => StringHelper.TryGetLong(i.Tag)).Where(i => i.HasValue).ToList();
            var dbListingsToUpdate = db.Listings.GetAll().Where(i => listingIds.Contains(i.Id)).ToList();
            var updatedListingIds = new List<string>();
            //51082
            foreach (var dbListing in dbListingsToUpdate)
            {
                var newPrice = GetNewPrice(db, dbListing.Id);
                if (newPrice.HasValue)
                {
                    if (dbListing.CurrentPrice != newPrice)
                    {
                        _log.Info("Changing price for SKU=" + dbListing.SKU + ", " + dbListing.CurrentPrice + " => " + newPrice + ", IsFBA=" + dbListing.IsFBA + ", IsPrime=" + dbListing.IsPrime + ", Market=" + (int)dbListing.Market);

                        LogListingPrice(db,
                            PriceChangeSourceType.SetByAutoPrice,
                            dbListing.Id,
                            dbListing.SKU,
                            newPrice.Value,
                            dbListing.CurrentPrice,
                            _time.GetAppNowTime(),
                            null);

                        dbListing.CurrentPrice = newPrice.Value;
                        dbListing.PriceUpdateRequested = true;                        
                    }
                    updatedListingIds.Add(dbListing.Id.ToString());
                }
                else
                {
                    _log.Info("Unbale to calculate new price for " + dbListing.SKU);
                }
            }

            db.Commit();

            var actionsSuccess = requestedActions.Where(i => updatedListingIds.Contains(i.Tag)).ToList();
            foreach (var actionToUpdate in actionsSuccess)
            {
                _actionService.SetResult(db, actionToUpdate.Id, SystemActionStatus.Done, null);
            }

            var actionFails = requestedActions.Where(i => !updatedListingIds.Contains(i.Tag)).ToList();
            foreach (var actionToUpdate in actionFails)
            {
                _actionService.SetResult(db, actionToUpdate.Id, SystemActionStatus.Fail, null);
            }
            db.Commit();
        }


        private decimal? GetNewPrice(IUnitOfWork db, long itemId)
        {
            var itemInfo = db.Items.GetAllViewActual()
                .Select(i => new
                {
                    Id = i.Id,
                    ListingId = i.ListingEntityId,
                    StyleId = i.StyleId,
                    StyleItemId = i.StyleItemId,
                    CurrentPrice = i.CurrentPrice,
                    IsFBA = i.IsFBA,
                    IsPrime = i.IsPrime,
                    Weight = i.Weight,
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,
                })
                .Where(i => i.ListingId == itemId)
                .FirstOrDefault();
            if (itemInfo == null)
                return null;

            decimal? sourcePrice = db.Items.GetAllViewActual()
                .Where(i => i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                    && i.StyleItemId == itemInfo.StyleItemId
                    && !i.IsFBA
                    && !i.IsPrime)
                .Select(i => i.CurrentPrice)
                .OrderBy(i => i)
                .FirstOrDefault();

            if (sourcePrice == null || sourcePrice == 0)
                return null;

            var priceService = new PriceService(_dbFactory);
            var newPrice = priceService.GetMarketPrice(sourcePrice.Value,
                null,
                itemInfo.IsPrime,
                itemInfo.IsFBA,
                itemInfo.Weight,
                (MarketType)itemInfo.Market,
                itemInfo.MarketplaceId,
                itemInfo.StyleItemId.Value);

            return newPrice;
        }



        public void FixupFBAPrices(IUnitOfWork db)
        {
            _log.Info("FixupFBAPrices");
            var today = _time.GetAppNowTime().Date;

            var fbaAndFbpListings = db.Items.GetAllViewActual().Where(l => (l.IsFBA
                                                            || l.SKU.Contains("-FBP"))
                                                          && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId).ToList();

            var fbaAndFbpAsinList = fbaAndFbpListings.Select(l => l.ASIN).ToList();

            var noFbaAndFbpListings = db.Items.GetAllViewActual().Where(l => !l.IsFBA
                                                            && (String.IsNullOrEmpty(l.SKU) || !l.SKU.Contains("-FBP"))
                                                            && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                                            && fbaAndFbpAsinList.Contains(l.ASIN))
                                                            .ToList();
            
            foreach (var fbaListing in fbaAndFbpListings)
            {
                if (!fbaListing.ListingEntityId.HasValue) //TEMP: always true
                    continue;

                decimal? newPrice = null;
                decimal? oldPrice = null;
                var requireAdjustment = false;
                var noFbaListing = noFbaAndFbpListings.FirstOrDefault(l => l.Barcode == fbaListing.Barcode);

                if (noFbaListing != null)
                {
                    var noFbaPrice = noFbaListing.CurrentPrice;
                    if (noFbaListing.SalePrice.HasValue && noFbaListing.SaleStartDate <= today)
                    {
                        noFbaPrice = noFbaListing.SalePrice.Value;
                    }

                    var fbaPrice = fbaListing.CurrentPrice;
                    if (fbaListing.SalePrice.HasValue && fbaListing.SaleStartDate <= today)
                    {
                        fbaPrice = fbaListing.SalePrice.Value;
                    }

                    var isOversize = noFbaListing.OnMarketTemplateName == AmazonTemplateHelper.OversizeTemplate;

                    //NOTE: Например, если я поставлю FBA price = 21.99, а Seller Fulfilled price 14.99+4.49, то произойдет эта проблема.
                    //NOTE: Он вроде разрешаю до 1.5 разницу. T.e. максимум FBA price который я могу поставит – 14.99+4.49+1.5 = $20.98
                    //if (fbaPrice > noFbaPrice
                    //    + (isOversize ? 8.49M : 7.49M))
                    //    //+ 4.49M //Shipping cost
                    //    //+ 1.5M) //Threashold 
                    //{
                    //    newPrice = noFbaPrice + (isOversize ? 8.49M : 7.49M);// + 4.49M + 1.5M;
                    //    oldPrice = fbaListing.AutoAdjustedPrice ?? fbaListing.CurrentPrice; //NOTE: actual old price

                    //    if (newPrice != oldPrice) //NOTE: exclude when price already has that adjustment
                    //    {
                    //        _log.Info("Change FBA listing price, ASIN=" + fbaListing.ASIN + ", from=" + oldPrice + ", to=" + newPrice);
                    //        var dbFbaListing = db.Listings.Get(fbaListing.ListingEntityId.Value);
                    //        dbFbaListing.AutoAdjustedPrice = newPrice;
                    //        dbFbaListing.PriceUpdateRequested = true;
                    //        db.Commit();
                    //    }

                    //    requireAdjustment = true;
                    //}
                }
                else
                {
                    _log.Info("No not-FBA analog, ASIN=" + fbaListing.ASIN + ", barcode=" + fbaListing.Barcode);
                }

                //NOTE: reset unnecessary adjustments
                if (!requireAdjustment
                    && fbaListing.AutoAdjustedPrice.HasValue)
                {
                    newPrice = fbaListing.CurrentPrice;
                    oldPrice = fbaListing.AutoAdjustedPrice;

                    _log.Info("Change FBA listing price, ASIN=" + fbaListing.ASIN + ", from=" + oldPrice + ", to=" + newPrice);
                    var dbFbaListing = db.Listings.Get(fbaListing.ListingEntityId.Value);
                    dbFbaListing.AutoAdjustedPrice = null;
                    dbFbaListing.PriceUpdateRequested = true;
                    db.Commit();
                }

                if (newPrice.HasValue
                    && oldPrice.HasValue
                    && newPrice != oldPrice)
                {
                    LogListingPrice(db,
                        PriceChangeSourceType.SetByAutoPrice,
                        fbaListing.ListingEntityId.Value,
                        fbaListing.SKU,
                        newPrice.Value,
                        oldPrice,
                        _time.GetAppNowTime(),
                        null);
                }
            }

            db.Commit();
        }


        public void LogListingPrice(IUnitOfWork db,
            PriceChangeSourceType type,
            long listingId,
            string sku,
            decimal newPrice,
            decimal? oldPrice,
            DateTime when,
            long? by)
        {
            db.PriceHistories.Add(new PriceHistory()
            {
                ListingId = listingId,
                SKU = sku,
                Type = (int)type,
                
                Price = newPrice,
                OldPrice = oldPrice,

                ChangeDate = when,
                ChangedBy = by,
            });

            db.Commit();
        }
    }
}
