using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Histories;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Caches;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory.Prices;
using Amazon.Web.ViewModels.Products;
using Amazon.Core.Models.Histories.HistoryDatas;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StylePriceViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public int Type { get; set; }

        public int ItemType { get; set; }

        public string Image { get; set; }
        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Image, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail:true);
            }
        }

        public decimal? ExcessiveShipmentAmount { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public int? MaxPiecesOnSale { get; set; }

        public IList<SizePriceViewModel> Sizes { get; set; }


        public override string ToString()
        {
            var text = "Id=" + Id
                       + ", StyleId=" + StyleId
                       + ", Type=" + Type
                       + ", ItemType=" + ItemType
                       + ", MaxPiecesOnSale=" + MaxPiecesOnSale
                       + ", UpdateDate=" + UpdateDate
                       + ", CreateDate=" + CreateDate;
            if (Sizes != null)
            {
                foreach (var size in Sizes)
                    text += "\r\n Size=" + size.Size
                            + ", SalePrice=" + size.SalePrice
                            + ", SFPSalePrice=" + size.SFPSalePrice
                            + ", SaleStartDate=" + size.SaleStartDate
                            + ", SaleEndDate=" + size.SaleEndDate
                            + ", InitSalePrice=" + size.InitSalePrice
                            + ", InitSFPSalePrice=" + size.InitSFPSalePrice
                            + ", MaxPiecesOnSale=" + size.MaxPiecesOnSale;
            }
            return text;
        }

        public StylePriceViewModel()
        {
            Sizes = new List<SizePriceViewModel>();
        }


        public StylePriceViewModel(IUnitOfWork db, long styleId, DateTime when)
        {
            var style = db.Styles.Get(styleId);
            var itemTypeId = style.ItemTypeId ?? StyleViewModel.DefaultItemType;

            StyleId = styleId;
            StyleString = style.StyleID;

            Type = style.Type;

            var excessiveShipmentAmount = StringHelper.TryGetDecimal(db.StyleFeatureTextValues.GetFeatureValueByStyleIdByFeatureId(styleId, StyleFeatureHelper.EXCESSIVE_SHIPMENT)?.Value);
            ExcessiveShipmentAmount = excessiveShipmentAmount;

            Image = style.Image;
            ItemType = itemTypeId;

            //Sizes
            var styleSizes = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                .ToList();
            var styleItemIds = styleSizes.Select(s => s.StyleItemId).ToList();

            var recentPermanentPriceChangeByStyleItem =
                db.StyleItemActionHistories.GetAll().Where(a => styleItemIds.Contains(a.StyleItemId)
                                                                    && (a.ActionName == StyleItemHistoryTypes.AddPermanentSale
                                                                    || a.ActionName == StyleItemHistoryTypes.AddSale
                                                                    || a.ActionName == StyleItemHistoryTypes.RemoveSale))
                    .GroupBy(a => a.StyleItemId)
                    .Select(g => new StyleItemActionHistoryDTO()
                    {
                        StyleItemId = g.Key,
                        CreateDate = g.Max(a => a.CreateDate)
                    }).ToList();

            var itemsByStyleItem = db.Items.GetAllViewAsDto()
                .Where(i => i.StyleItemId.HasValue 
                    && styleItemIds.Contains(i.StyleItemId.Value)
                    && (i.Market == (int)MarketType.Walmart || i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId))
                .GroupBy(i => i.StyleItemId.Value)
                .Select(g => new
                {
                    StyleItemId = g.Key,
                    MinPrice = g.Min(i => i.SalePrice ?? i.CurrentPrice),
                    MaxPrice = g.Max(i => i.SalePrice ?? i.CurrentPrice),
                }).ToList()
                .Select(i => new ItemDTO()
                {
                    StyleItemId = i.StyleItemId,
                    MinPrice = i.MinPrice,
                    MaxPrice = i.MaxPrice
                }).ToList();

            var styleSizeCaches = db.StyleItemCaches.GetForStyleId(styleId);

            var styleItemSales = db.StyleItemSales.GetAllAsDto().Where(s => styleItemIds.Contains(s.StyleItemId)
                && !s.IsDeleted).ToList();
            var styleItemSaleIds = styleItemSales.Select(s => s.Id).ToList();

            var styleItemSaleToMarkets = db.StyleItemSaleToMarkets.GetAllAsDto().Where(s => styleItemSaleIds.Contains(s.SaleId)).ToList();
            var styleItemSaleToListings = db.StyleItemSaleToListings.GetAllAsDto().Where(s => styleItemSaleIds.Contains(s.SaleId)).ToList();

            MaxPiecesOnSale = styleItemSales.Any(s => s.MaxPiecesMode == (int) MaxPiecesOnSaleMode.ByStyle)
                ? styleItemSales[0].MaxPiecesOnSale : null;
            
            Sizes = BuildSizes(styleSizes,
                styleSizeCaches,
                styleItemSales,
                styleItemSaleToMarkets,
                styleItemSaleToListings,
                recentPermanentPriceChangeByStyleItem,
                itemsByStyleItem);
        }
        
        public long Apply(IUnitOfWork db, 
            IDbFactory dbFactory,
            ILogService log,
            ICacheService cache,
            IPriceManager priceManager,
            IStyleItemHistoryService styleItemHistory,
            ISystemActionService actionService,
            DateTime when, 
            long? by)
        {
            var style = db.Styles.Get(StyleId);

            style.UpdateDate = when;
            style.UpdatedBy = by;

            style.ReSaveDate = when;
            style.ReSaveBy = by;


            var excessiveShipmentAttr = db.StyleFeatureTextValues.GetAll().FirstOrDefault(sv => sv.StyleId == style.Id
                && sv.FeatureId == StyleFeatureHelper.EXCESSIVE_SHIPMENT);
            if (excessiveShipmentAttr == null)
            {
                excessiveShipmentAttr = new Core.Entities.Features.StyleFeatureTextValue()
                {
                    StyleId = StyleId,
                    CreateDate = when,
                    CreatedBy = by,
                    FeatureId = StyleFeatureHelper.EXCESSIVE_SHIPMENT,                    
                };
                db.StyleFeatureTextValues.Add(excessiveShipmentAttr);
            }
            excessiveShipmentAttr.Value = ExcessiveShipmentAmount.HasValue ? ExcessiveShipmentAmount.ToString() : null;


            var wasAnyChanges = false;
            var wasAnyMinMaxChanges = false;
            if (Sizes != null && Sizes.Any())
            {
                var styleItems = db.StyleItems.GetFiltered(si => si.StyleId == StyleId).ToList();

                foreach (var size in Sizes)  //Update prices (marking when/by)
                {
                    var changeType = PriceChangeSourceType.None;
                    string tag = null;
                    bool wasChanged = false;
                    var minMaxPriceChanged = false;

                    var styleItem = styleItems.FirstOrDefault(si => si.Id == size.StyleItemId);

                    if (styleItem != null)
                    {
                        StyleItemSale sale = size.SaleId.HasValue
                            ? db.StyleItemSales
                                .GetAll()
                                .FirstOrDefault(s => s.Id == size.SaleId.Value)
                            : null;

                        if (sale == null) //If no sale Id remove all exist sales (Remove Sale action was performed)
                        {
                            IList<StyleItemSale> saleList = db.StyleItemSales
                                .GetAll()
                                .Where(s => s.StyleItemId == styleItem.Id
                                    && !s.IsDeleted)
                                .ToList();

                            foreach (var toRemove in saleList)
                            { 
                                log.Info("Sale mark removed, saleId=" + toRemove.Id + ", Info=" +
                                         ToStringHelper.ToString(toRemove));
                                toRemove.IsDeleted = true;
                                db.Commit();

                                styleItemHistory.AddRecord(StyleItemHistoryTypes.RemoveSale, styleItem.Id, 
                                    new HistorySaleData()
                                    {
                                        SaleStartDate = toRemove.SaleStartDate,
                                        SaleEndDate = toRemove.SaleEndDate,
                                    }, by);
                            }
                        }

                        var salePrice = size.InitSalePrice ?? size.NewSalePrice;
                        var sfpSalePrice = size.InitSFPSalePrice ?? size.NewSFPSalePrice;

                        if (salePrice.HasValue
                            || sfpSalePrice.HasValue)
                        {
                            //Get Default markets
                            var markets = MarketPriceEditViewModel.GetForStyleItemId(db,
                                dbFactory,
                                size.StyleItemId,
                                salePrice,
                                sfpSalePrice);

                            if ((SizePriceViewModel.SizeMarketApplyModes)size.MarketMode ==
                                SizePriceViewModel.SizeMarketApplyModes.OnlyAmazonUS)
                                markets = markets.Where(m => m.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId).ToList();

                            //Save Default markets
                            if ((SizePriceViewModel.SizePriceApplyModes) size.ApplyMode ==
                                SizePriceViewModel.SizePriceApplyModes.Sale)
                            {
                                var results = MarketPriceEditViewModel.ApplySale(db,
                                    log,
                                    size.StyleItemId,
                                    markets,
                                    when,
                                    by);

                                if (results.Any())
                                {
                                    var saleId = results[0].SaleId;
                                    sale = db.StyleItemSales.GetAll().FirstOrDefault(s => s.Id == saleId);
                                }
                            }
                        }


                        if ((SizePriceViewModel.SizePriceApplyModes)size.ApplyMode ==
                            SizePriceViewModel.SizePriceApplyModes.Permanent)
                        {
                            //Get Default markets
                            var markets = MarketPriceEditViewModel.GetForStyleItemId(db,
                                dbFactory,
                                size.StyleItemId,
                                salePrice,
                                sfpSalePrice);

                            if ((SizePriceViewModel.SizeMarketApplyModes)size.MarketMode ==
                                SizePriceViewModel.SizeMarketApplyModes.OnlyAmazonUS)
                                markets = markets.Where(m => m.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId).ToList();

                            //NOTE: also mark exist sale as deleted
                            MarketPriceEditViewModel.ApplyPermanent(db,
                                log,
                                priceManager,
                                size.StyleItemId,
                                markets,
                                when,
                                by);

                            styleItemHistory.AddRecord(StyleItemHistoryTypes.AddPermanentSale, styleItem.Id,
                                new HistorySaleData()
                                {
                                    SalePrice = salePrice
                                }, by);
                        }

                        if (sale != null)
                        {
                            //    if (size.NewSalePrice.HasValue)
                            //    {
                            //        log.Info("Updated sale price, saleId=" + sale.Id + ", to=" + size.NewSalePrice + ", SFP=" + size.NewSFPSalePrice);
                            //        MarketPriceEditViewModel.UpdateSalePrices(db,
                            //            size.StyleItemId,
                            //            sale.Id,
                            //            size.NewSalePrice.Value,
                            //            size.NewSFPSalePrice);
                            //    }

                            sale.SaleStartDate = size.SaleStartDate;
                            sale.SaleEndDate = size.SaleEndDate;
                            sale.MaxPiecesMode = MaxPiecesOnSale.HasValue
                                ? (int) MaxPiecesOnSaleMode.ByStyle
                                : (int) MaxPiecesOnSaleMode.BySize;
                            sale.MaxPiecesOnSale = size.MaxPiecesOnSale ?? MaxPiecesOnSale;

                            db.Commit();

                            styleItemHistory.AddRecord(StyleItemHistoryTypes.AddSale, styleItem.Id,
                                new HistorySaleData()
                                {
                                    SalePrice = size.NewSalePrice,
                                    SaleStartDate = size.SaleStartDate,
                                    SaleEndDate = size.SaleEndDate

                                }, by);
                        }

                        minMaxPriceChanged = styleItem.MinPrice != size.MinPrice
                                            || styleItem.MaxPrice != size.MaxPrice;

                        if (minMaxPriceChanged)
                        {
                            styleItem.MinPrice = size.MinPrice;
                            styleItem.MaxPrice = size.MaxPrice;
                            db.Commit();
                        }

                        wasAnyMinMaxChanges = wasAnyMinMaxChanges || minMaxPriceChanged;
                    }
                }

                //NOTE: update all listing, ex. change price, start/end date, e.t.c.
                var styleListingIds = db.Listings.GetViewListingsAsDto(true)
                    .Where(l => l.StyleId == StyleId)
                    .Select(l => l.Id)
                    .ToList();
                var dbListings = db.Listings.GetAll().Where(l => styleListingIds.Contains(l.Id)).ToList();
                foreach (var dbListing in dbListings)
                {
                    dbListing.PriceUpdateRequested = true;
                    if (wasAnyMinMaxChanges)
                    {
                        actionService.AddAction(db,
                            SystemActionType.UpdateOnMarketProductPriceRule,
                            dbListing.SKU,
                            null,
                            null,
                            by);
                    }
                }
                db.Commit();
            }

            cache.RequestStyleIdUpdates(db,
                new List<long> { StyleId },
                UpdateCacheMode.IncludeChild,
                AccessManager.UserId);

            return StyleId;
        }


        private IList<SizePriceViewModel> BuildSizes(
            IList<StyleItemDTO> styleItems,
            IList<StyleItemCacheDTO> styleItemCaches,
            IList<StyleItemSaleDTO> styleItemSales,
            IList<StyleItemSaleToMarketDTO> saleToMarkets,
            IList<StyleItemSaleToListingDTO> saleToListings,
            IList<StyleItemActionHistoryDTO> recentPermanentPriceChangeByStyleItem,
            IList<ItemDTO> itemPriceByStyleItem)
        {
            var resultSizes = new List<SizePriceViewModel>();

            foreach (var styleItem in styleItems)
            {
                var styleItemSale = styleItemSales.OrderByDescending(si => si.CreateDate).FirstOrDefault(sic => sic.StyleItemId == styleItem.StyleItemId);
                var styleItemCache = styleItemCaches.FirstOrDefault(sc => sc.Id == styleItem.StyleItemId);
                var recentPermanentPriceChange = recentPermanentPriceChangeByStyleItem.FirstOrDefault(sc => sc.StyleItemId == styleItem.StyleItemId);
                var itemPrice = itemPriceByStyleItem.FirstOrDefault(i => i.StyleItemId == styleItem.StyleItemId);
                
                var size = new SizePriceViewModel();
                size.StyleItemId = styleItem.StyleItemId;
                size.SizeGroupName = styleItem.SizeGroupName;
                size.Size = styleItem.Size;
                size.Color = styleItem.Color;
                size.Weight = styleItem.Weight;
                size.MinPrice = styleItem.MinPrice;
                size.MaxPrice = styleItem.MaxPrice;
                size.LastChangeDate = recentPermanentPriceChange?.CreateDate;
                size.MinListingPrice = itemPrice?.MinPrice;
                size.MaxListingPrice = itemPrice?.MaxPrice;

                if (styleItemCache != null)
                {
                    size.RemainingQuantity = styleItemCache.RemainingQuantity;
                }

                if (styleItemSale != null)
                {
                    size.SaleId = styleItemSale.Id;
                    size.SaleStartDate = styleItemSale.SaleStartDate;
                    size.SaleEndDate = styleItemSale.SaleEndDate;
                    size.MaxPiecesOnSale = styleItemSale.MaxPiecesOnSale;
                    size.MaxPiecesMode = styleItemSale.MaxPiecesMode;
                    size.PiecesSoldOnSale = styleItemSale.PiecesSoldOnSale;

                    size.SaleToMarkets = saleToMarkets
                        .Where(m => m.SaleId == styleItemSale.Id)
                        .Select(m => new MarketPriceViewViewModel(m))
                        .ToList();
                }

                resultSizes.Add(size);
            }

            return resultSizes
                .OrderBy(s => SizeHelper.GetSizeIndex(s.Size))
                .ThenBy(s => s.Color)
                .ToList();
        }
    }
}