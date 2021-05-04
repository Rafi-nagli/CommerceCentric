using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.Web.ViewModels.Inventory.Prices;
using Amazon.Web.ViewModels.Products;
using Amazon.Core.Models.Stamps;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Api;
using Amazon.Core.Entities.Enums;

namespace Amazon.Web.ViewModels.Inventory
{
    public class MarketPriceEditViewModel
    {
        public long Id { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public string MarketName { get; set; }

        public string MarketCurrency { get; set; }

        public decimal? SalePrice { get; set; }
        public decimal? SFPSalePrice { get; set; }
        public decimal? SalePercent { get; set; }
        public bool ApplyToNewListings { get; set; }

        public IList<ListingPriceEditViewModel> Listings { get; set; }

        public override string ToString()
        {
            var msg = "Id=" + Id
                   + ", Market=" + Market
                   + ", MarketplaceId=" + MarketplaceId
                   + ", MarketName=" + MarketName
                   + ", MarketCurrency=" + MarketCurrency
                   + ", SalePrice=" + SalePrice
                   + ", SFPSalePrice=" + SFPSalePrice
                   + ", SalePercent=" + SalePercent
                   + ", ApplyToNewListings=" + ApplyToNewListings;

            msg += ", Listings=";
            if (Listings != null)
            {
                foreach (var l in Listings)
                    msg += "\r\n" + l.ToString();
            }
            else
            {
                msg += "[null]";
            }
            return msg;
        }

        public static void ApplyPermanent(IUnitOfWork db,
            ILogService log,
            IPriceManager priceManager,
            long styleItemId,
            IList<MarketPriceEditViewModel> markets,
            DateTime when,
            long? by)
        {
            var sale = db.StyleItemSales.GetAllAsDto().FirstOrDefault(s => s.StyleItemId == styleItemId
                                                                           && !s.IsDeleted);

            if (sale != null)
            {
                var dbSale = db.StyleItemSales.Get(sale.Id);
                dbSale.IsDeleted = true;
                db.Commit();
            }

            foreach (var market in markets)
            {
                if (market.Listings != null
                    && (market.SalePrice.HasValue
                        || market.SFPSalePrice.HasValue))
                {
                    foreach (var listing in market.Listings.Where(l => l.IsChecked).ToList())
                    {
                        var salePrice = listing.OverrideSalePrice ?? market.SalePrice.Value;

                        var dbListing = db.Listings.Get(listing.ListingId);
                        if (dbListing != null)
                        {
                            //var newSalePrice = market.SalePrice.Value;
                            //NOTE: moved to SalePrice/OverrideSalePrice calculation
                            //if (dbListing.IsPrime && market.Market == MarketType.Amazon)
                            //{
                            //    var item = db.Items.Get(dbListing.ItemId);

                            //    var isOversizeTemplate = item.OnMarketTemplateName == AmazonTemplateHelper.OversizeTemplate;
                            //    if (market.SFPSalePrice.HasValue)
                            //        newSalePrice = market.SFPSalePrice.Value + (isOversizeTemplate ? 2 : 0);
                            //    else
                            //        newSalePrice = PriceHelper.RoundToFloor99(isOversizeTemplate ? (newSalePrice + 9.49M) : (newSalePrice + 7.49M));
                            //}
                            log.Info("Price updated, " + dbListing.Market + " - " + dbListing.MarketplaceId + ", from=" + dbListing.CurrentPrice + " => " + salePrice);


                            priceManager.LogListingPrice(db,
                                PriceChangeSourceType.EnterNewPrice,
                                dbListing.Id,
                                dbListing.SKU,
                                salePrice,
                                dbListing.CurrentPrice,
                                when,
                                by);

                            dbListing.CurrentPrice = salePrice;
                            dbListing.PriceUpdateRequested = true;
                        }
                    }
                }
            }
            db.Commit();
        }


        public static IList<MarketPriceViewViewModel> ApplySale(IUnitOfWork db,
            ILogService log,
            long styleItemId,
            IList<MarketPriceEditViewModel> markets,
            DateTime when,
            long? by)
        {
            var sale = db.StyleItemSales.GetAllAsDto().FirstOrDefault(s => s.StyleItemId == styleItemId
                                                                           && !s.IsDeleted);
            if (sale == null)
            {
                log.Info("Create empty Sale");

                var dbSale = new StyleItemSale()
                {
                    SaleStartDate = when.Date,
                    StyleItemId = styleItemId,
                    CreateDate = when,
                    CreatedBy = by,
                };
                db.StyleItemSales.Add(dbSale);
                db.Commit();

                sale = new StyleItemSaleDTO()
                {
                    Id = dbSale.Id,
                    StyleItemId = styleItemId
                };
            }

            var marketDtos = markets
                .Select(m => new StyleItemSaleToMarketDTO()
                {
                    Id = m.Id,
                    SaleId = sale.Id,
                    Market = (int)m.Market,
                    MarketplaceId = m.MarketplaceId,
                    SalePrice = m.SalePrice,
                    SFPSalePrice = m.SFPSalePrice,
                    SalePercent = m.SalePercent,
                    ApplyToNewListings = m.ApplyToNewListings,
                })
                .ToList();

            log.Info("All marktes=" + marketDtos.Count);

            db.StyleItemSaleToMarkets.UpdateForSale(sale.Id,
                marketDtos,
                when,
                by);


            var allCheckedListings = new List<StyleItemSaleToListingDTO>();
            foreach (var market in markets)
            {
                var marketDto = marketDtos.FirstOrDefault(m => m.Market == (int)market.Market
                                                               && (m.MarketplaceId == market.MarketplaceId
                                                                || String.IsNullOrEmpty(market.MarketplaceId)));
                if (marketDto != null)
                    market.Id = marketDto.Id;
                
                if (market.Listings != null)
                    allCheckedListings.AddRange(
                        market.Listings
                        .Where(l => l.IsChecked)
                        .Select(l => new StyleItemSaleToListingDTO()
                        {
                            SaleToMarketId = market.Id,
                            ListingId = l.ListingId,
                            OverrideSalePrice = l.OverrideSalePrice
                        }).ToList());
            }
            log.Info("Checked listings=" + allCheckedListings.Count);
            
            db.StyleItemSaleToListings.UpdateForSale(sale.Id,
                allCheckedListings,
                when,
                by);

            return marketDtos.Select(m => new MarketPriceViewViewModel(m)).ToList();
        }

        //public static void UpdateSalePrices(IUnitOfWork db,
        //    long styleItemId,
        //    long saleId,
        //    decimal newSalePrice,
        //    decimal? newSFPSalePrice)
        //{
        //    var rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, styleItemId);

        //    var marketPrices = db.StyleItemSaleToMarkets.GetAll().Where(s => s.SaleId == saleId).ToList();
        //    foreach (var marketPrice in marketPrices)
        //    {
        //        if (marketPrice.SalePrice.HasValue)
        //        {
        //            var marketListings = db.StyleItemSaleToListings.GetAll().Where(l => l.SaleToMarketId == )

        //            marketPrice.SalePrice = RateHelper.CalculateForMarket((MarketType) marketPrice.Market,
        //                marketPrice.MarketplaceId,
        //                newSalePrice,
        //                rateForMarketplace[MarketplaceKeeper.AmazonComMarketplaceId],
        //                rateForMarketplace[MarketplaceKeeper.AmazonCaMarketplaceId],
        //                rateForMarketplace[MarketplaceKeeper.AmazonUkMarketplaceId],
        //                rateForMarketplace[MarketplaceKeeper.AmazonAuMarketplaceId],
        //                RateService.GetMarketShippingAmount(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId),
        //                RateService.GetMarketShippingAmount((MarketType)marketPrice.Market, marketPrice.MarketplaceId),
        //                RateService.GetMarketExtraAmount((MarketType)marketPrice.Market, marketPrice.MarketplaceId));


        //        }
        //    }

        //    db.Commit();
        //}

        public static IList<MarketPriceEditViewModel> GetForStyleItemId(IUnitOfWork db,
            IDbFactory dbFactory,
            long styleItemId,
            decimal? initSalePrice,
            decimal? initSFPSalePrice)
        {
            var priceService = new PriceService(dbFactory);

            var results = new List<MarketPriceEditViewModel>();
            var allMarketplaces = UIMarketHelper.GetSalesMarketplaces();
            var allListings = db.Listings.GetListingsAsListingDto()
                .Where(l => l.StyleItemId == styleItemId)
                .ToList()
                .Where(l => !l.IsFBA)
                .ToList();
                        
            var rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, styleItemId);

            var sale = db.StyleItemSales.GetAllAsDto().FirstOrDefault(s => s.StyleItemId == styleItemId && !s.IsDeleted);
            var checkedListings = new List<StyleItemSaleToListingDTO>();
            if (sale != null)
                checkedListings = db.StyleItemSaleToListings.GetAllAsDto().Where(s => s.SaleId == sale.Id).ToList();

            var checkedMarkets = new List<StyleItemSaleToMarketDTO>();
            if (sale != null)
                checkedMarkets = db.StyleItemSaleToMarkets.GetAllAsDto().Where(s => s.SaleId == sale.Id).ToList();

            foreach (var market in allMarketplaces)
            {
                var checkedMarket = checkedMarkets.FirstOrDefault(l => l.Market == (int) market.Market
                                                                       && (l.MarketplaceId == market.MarketplaceId || String.IsNullOrEmpty(market.MarketplaceId)));

                var marketListings = allListings
                    .Where(l => l.Market == (int)market.Market
                                && (l.MarketplaceId == market.MarketplaceId ||
                                    String.IsNullOrEmpty(market.MarketplaceId)))
                    .Select(l => new ListingPriceEditViewModel(l))
                    .ToList();

                foreach (var listing in marketListings)
                {
                    listing.IsChecked = sale == null ?
                        ((listing.IsPrime || SkuHelper.IsPrime(listing.SKU)) ? initSFPSalePrice.HasValue : initSalePrice.HasValue) :
                        checkedListings.Any(l => l.ListingId == listing.ListingId);
                }

                var defaultPrice = //sale == null &&
                    initSalePrice.HasValue
                    && marketListings.Any() ? priceService.GetMarketDefaultPrice(initSalePrice.Value,
                        market.Market,
                        market.MarketplaceId,
                        rateForMarketplace) : (decimal?)null;

                foreach (var marketListing in marketListings)
                {
                    var newDefaultPrice = priceService.ApplyMarketSpecified(defaultPrice,
                        initSFPSalePrice,
                        market.Market,
                        market.MarketplaceId,
                        marketListing.Weight,
                        marketListing.IsPrime,
                        marketListing.IsFBA);

                    if (newDefaultPrice != defaultPrice)
                        marketListing.OverrideSalePrice = newDefaultPrice;
                }

                var saleToMarket = new MarketPriceEditViewModel()
                {
                    Id = checkedMarket != null ? checkedMarket.Id : 0,

                    Market = (MarketType) market.Market,
                    MarketplaceId = market.MarketplaceId,
                    MarketName = MarketHelper.GetMarketName((int)market.Market, market.MarketplaceId),
                    MarketCurrency = PriceHelper.GetCurrencySymbol(market.Market, market.MarketplaceId),

                    SalePrice = checkedMarket != null ? checkedMarket.SalePrice : defaultPrice,
                    SFPSalePrice = market.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId ? initSFPSalePrice : null,
                    SalePercent = checkedMarket != null ? checkedMarket.SalePercent : null,
                    ApplyToNewListings = checkedMarket != null ? checkedMarket.ApplyToNewListings : false,

                    Listings = marketListings
                };

                results.Add(saleToMarket);
            }

            return results;
        }

    }
}