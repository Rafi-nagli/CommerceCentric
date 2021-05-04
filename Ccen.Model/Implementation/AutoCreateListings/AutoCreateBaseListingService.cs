using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api.Exports;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Markets;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Walmart.Api;

namespace Amazon.Model.Implementation
{
    public abstract class AutoCreateBaseListingService : IAutoCreateListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IBarcodeService _barcodeService;
        private IEmailService _emailService;
        private IItemHistoryService _itemHistoryService;
        private bool _isDebug;

        public AutoCreateBaseListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            IEmailService emailService,
            IItemHistoryService itemHistoryService,
            bool isDebug)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _barcodeService = barcodeService;
            _emailService = emailService;
            _itemHistoryService = itemHistoryService;
            _isDebug = isDebug;
        }

        public ParentItemDTO CreateFromStyle(IUnitOfWork db,
            long styleId,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            var style = db.Styles.GetAllActiveAsDto().FirstOrDefault(s => s.Id == styleId);
            if (style == null)
            {
                messages.Add(MessageString.Error("Style Id was not found"));
                return null;
            }
            return CreateFromStyle(db,
                style,
                market,
                marketplaceId,
                out messages);
        }

        public ParentItemDTO CreateFromStyle(IUnitOfWork db,
            string styleString,
            decimal? customPrice,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            var style = db.Styles.GetAllActiveAsDto().FirstOrDefault(s => s.StyleID == styleString);
            if (style == null)
            {
                messages.Add(MessageString.Error("Style String was not found"));
                return null;
            }
            var model = CreateFromStyle(db,
                style,
                market,
                marketplaceId,
                out messages);

            if (model != null && customPrice.HasValue)
                model.Variations.ForEach(v => v.CurrentPrice = customPrice.Value);

            return model;
        }

        public ParentItemDTO CreateFromStyle(IUnitOfWork db,
            StyleEntireDto style,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            var styleItems = db.StyleItems.GetByStyleIdWithBarcodesAsDto(style.Id);

            var model = new ParentItemDTO();

            model.ASIN = style.StyleID;

            model.Market = (int)market;
            model.MarketplaceId = marketplaceId;

            model.AmazonName = style.Name;

            var items = new List<ItemDTO>();
            foreach (var styleItem in styleItems)
            {
                var newItem = new ItemDTO();
                newItem.Market = (int)market;
                newItem.MarketplaceId = marketplaceId;

                newItem.StyleString = style.StyleID;
                newItem.StyleId = styleItem.StyleId;
                newItem.StyleItemId = styleItem.StyleItemId;
                newItem.StyleSize = styleItem.Size;
                newItem.StyleColor = styleItem.Color;

                newItem.Size = styleItem.Size;
                newItem.Color = styleItem.Color;

                newItem.Weight = styleItem.Weight;

                var barcode = styleItem.Barcodes?.FirstOrDefault();
                if (barcode != null)
                    newItem.Barcode = barcode.Barcode;

                newItem.CurrentPrice = 0;
                newItem.SKU = style.StyleID + "-" + SizeHelper.PrepareSizeForSKU(styleItem.Size, false);
                if (!String.IsNullOrEmpty(styleItem.Color))
                    newItem.SKU += "-" + styleItem.Color;

                items.Add(newItem);
            }

            model.Variations = items
                .OrderBy(si => SizeHelper.GetSizeIndex(si.StyleSize))
                .ThenBy(si => si.Color)
                .ToList();

            return model;
        }

        public ParentItemDTO CreateAndMergeWithExistFromStyle(IUnitOfWork db, 
            StyleEntireDto style, 
            MarketType market,
            string marketplaceId, 
            out IList<MessageString> messages)
        {
            throw new NotImplementedException();
        }


        public ParentItemDTO CreateFromParentASIN(IUnitOfWork db,
            string asin,
            int market,
            string marketplaceId,
            bool includeAllChild,
            bool includeZeroQtyChild,
            int? minQty,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            var parentItem = db.ParentItems.GetAllAsDto().FirstOrDefault(s => s.ASIN == asin
                && s.Market == market
                && s.MarketplaceId == marketplaceId);
            if (parentItem == null)
            {
                messages.Add(MessageString.Error("Parent ASIN was not found"));
                return null;
            }

            minQty = minQty ?? 0;
            var itemsQuery = from i in db.Items.GetAllViewAsDto()
                             join sic in db.StyleItemCaches.GetAll() on i.StyleItemId equals sic.Id
                             where i.ParentASIN == parentItem.ASIN
                                && sic.RemainingQuantity > minQty //Exclude items w/o qty
                                && i.Market == parentItem.Market
                                && i.MarketplaceId == parentItem.MarketplaceId
                             select i;

            if (!includeAllChild)
                itemsQuery = itemsQuery.Where(i => !i.IsFBA //Exclude FBA
                                                    && !i.SKU.Contains("-FBP") //Exclude FBP
                                                    && !i.StyleString.Contains("-tmp") //Remove items linked to tmp styles
                                                    );

            var items = itemsQuery.ToList();

            var model = new ParentItemDTO();

            var firstStyleString = items.Any() ? items.First().StyleString : "";

            model.ASIN = firstStyleString;

            model.Market = (int)market;
            model.MarketplaceId = marketplaceId;

            model.AmazonName = parentItem.AmazonName;

            foreach (var item in items)
            {
                item.Size = SizeHelper.SizeCorrection(item.Size, item.StyleSize);
                item.RealQuantity = 0;
            }

            model.Variations = items
                .OrderBy(i => i.StyleId)
                .ThenBy(i => SizeHelper.GetSizeIndex(i.StyleSize))
                .ToList();

            return model;
        }

        public void PrepareData(ParentItemDTO parentItemDto)
        {
            parentItemDto.ASIN = StringHelper.TrimWhitespace(StringHelper.ToUpper(parentItemDto.ASIN));
            parentItemDto.AmazonName = StringHelper.TrimWhitespace(parentItemDto.AmazonName);

            foreach (var item in parentItemDto.Variations)
            {
                item.Barcode = StringHelper.TrimWhitespace(item.Barcode);
                item.SKU = StringHelper.ToUpper(StringHelper.TrimWhitespace(item.SKU));

                item.Size = StringHelper.TrimWhitespace(item.Size);
                item.Color = StringHelper.TrimWhitespace(item.Color);

                item.Market = parentItemDto.Market;
                item.MarketplaceId = parentItemDto.MarketplaceId;

                if (String.IsNullOrEmpty(item.Size))
                    item.Size = item.StyleSize;
            }
        }

        public CallResult<string> Save(ParentItemDTO parentItemDto,
            string newComment,
            IUnitOfWork db,
            DateTime when,
            long? by)
        {
            //Prepare StyleId
            foreach (var item in parentItemDto.Variations)
            {
                var style = db.Styles.GetAllAsDto().FirstOrDefault(s => !s.Deleted && s.StyleID == item.StyleString);
                item.StyleId = style != null ? style.Id : (long?)null;
            }

            //Recheck ParentItem Id, prevent to check duplicates
            if (parentItemDto.Id == 0)
            {
                var existParentItem = db.ParentItems
                    .GetAllAsDto()
                    .FirstOrDefault(pi => pi.ASIN == parentItemDto.ASIN
                                          && pi.Market == parentItemDto.Market
                                          && (pi.MarketplaceId == parentItemDto.MarketplaceId ||
                                           String.IsNullOrEmpty(parentItemDto.MarketplaceId)));

                if (existParentItem != null)
                {
                    _log.Info("Parent ASIN already exists, ASIN=" + parentItemDto.ASIN);
                    return CallResult<string>.Fail("ASIN already exists", null);
                }
            }

            if (parentItemDto.Id != 0)
            {
                UpdateProduct(parentItemDto, db, when, by);
            }
            else
            {
                CreateProduct(parentItemDto, db, when, by);
            }

            if (parentItemDto.Id > 0 && !String.IsNullOrEmpty(newComment))
            {
                db.ProductComments.Add(new ProductComment()
                {
                    ProductId = (int)parentItemDto.Id,
                    Message = newComment,
                    CreatedBy = by,
                    CreateDate = when,
                });
                db.Commit();
            }
            return CallResult<string>.Success("");
        }

        public long CreateProduct(ParentItemDTO parentItemDto,
            IUnitOfWork db,
            DateTime when,
            long? by)
        {
            var id = 0;

            var parentItem = new ParentItem()
            {
                ASIN = parentItemDto.ASIN,
                Market = parentItemDto.Market,
                MarketplaceId = parentItemDto.MarketplaceId,

                ForceEnableColorVariations = parentItemDto.ForceEnableColorVariations,
                OnHold = parentItemDto.OnHold,

                SKU = parentItemDto.ASIN,
                AmazonName = parentItemDto.AmazonName,

                CreateDate = when,
                CreatedBy = by
            };

            db.ParentItems.Add(parentItem);
            db.Commit();

            parentItemDto.Id = parentItem.Id;
            id = parentItem.Id;

            var variationDtoList = parentItemDto.Variations
                .Select(v => new ItemDTO()
                {
                    ParentASIN = parentItem.ASIN,
                    Name = parentItem.AmazonName, //Equal parent name

                    StyleString = v.StyleString,
                    StyleId = v.StyleId,
                    StyleItemId = v.StyleItemId,
                    Size = v.Size,
                    Color = v.Color,

                    ASIN = v.SKU,
                    SKU = v.SKU,
                    IsDefault = v.IsDefault,
                    ListingId = v.SKU,

                    Barcode = v.AutoGeneratedBarcode ? BarcodeHelper.GenerateBarcode(_barcodeService, v.SKU, when) : v.Barcode,

                    CurrentPrice = v.CurrentPrice,
                    RealQuantity = 0,

                    PublishedStatus = (int)PublishedStatuses.New,
                }).ToList();

            db.Items.UpdateItemsForParentItem(_itemHistoryService,
                "AutoCreateBaseListingService.CreateProduct",
                parentItem.ASIN,
                parentItem.Market,
                parentItem.MarketplaceId,
                variationDtoList,
                when,
                by);

            _cacheService.RequestParentItemIdUpdates(db,
                new List<long>() { parentItem.Id },
                UpdateCacheMode.IncludeChild,
                by);

            return id;
        }

        public void UpdateProduct(ParentItemDTO parentItemDto,
            IUnitOfWork db,
            DateTime when,
            long? by)
        {
            var parentItem = db.ParentItems.Get(parentItemDto.Id);
            var previousASIN = parentItem.ASIN;

            if (parentItem.ASIN != parentItemDto.ASIN)
            {
                parentItem.ASIN = parentItemDto.ASIN;
            }
            parentItem.SKU = parentItemDto.ASIN;

            parentItem.ForceEnableColorVariations = parentItemDto.ForceEnableColorVariations;
            parentItem.OnHold = parentItemDto.OnHold;
            parentItem.AmazonName = parentItemDto.AmazonName;

            parentItem.UpdateDate = when;
            parentItem.UpdatedBy = by;

            db.Commit();

            var variationDtoList = parentItemDto.Variations
                .Select(v => new ItemDTO()
                {
                    ParentASIN = parentItem.ASIN,
                    Name = parentItemDto.AmazonName,

                    Id = v.Id,
                    StyleString = v.StyleString,
                    StyleId = v.StyleId,
                    StyleItemId = v.StyleItemId,
                    Size = v.Size,
                    Color = v.Color,

                    ASIN = v.SKU,
                    IsDefault = v.IsDefault,
                    ListingId = v.SKU,

                    SKU = v.SKU,
                    Barcode = v.AutoGeneratedBarcode ? BarcodeHelper.GenerateBarcode(_barcodeService, v.SKU, when) : v.Barcode,

                    RealQuantity = v.RealQuantity,

                    CurrentPrice = v.CurrentPrice,

                    PublishedStatus = v.OverridePublishedStatus.HasValue && v.OverridePublishedStatus.Value != v.PublishedStatus ? v.OverridePublishedStatus.Value : (int)PublishedStatuses.None
                }).ToList();

            db.Items.UpdateItemsForParentItem(_itemHistoryService,
                "AutoCreateBaseListingService.UpdateProduct",
                previousASIN,
                parentItem.Market,
                parentItem.MarketplaceId,
                variationDtoList,
                when,
                by);

            _cacheService.RequestParentItemIdUpdates(db,
                new List<long>() { parentItem.Id },
                UpdateCacheMode.IncludeChild,
                by);
        }

        public abstract void CreateListings();

        //public void CreateEBayListings()
        //{
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
        //                           group siCache by siCache.StyleId
        //                           into byStyle
        //                           select new
        //                           {
        //                               StyleId = byStyle.Key,
        //                               Qty = byStyle.Sum(s => s.RemainingQuantity)
        //                           };

        //        var shortSleeveFeatures = db.StyleFeatureValues.GetAll().Where(sfv => sfv.FeatureId == StyleFeatureHelper.SLEEVE
        //                                                        && sfv.FeatureValueId != 14 && sfv.FeatureValueId != 336);

        //        var query = from s in db.Styles.GetAll()
        //                    join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
        //                    //Not long sleeve
        //                    //join sleeve in shortSleeveFeatures on s.Id equals sleeve.StyleId
        //                    join qty in groupByStyle on s.Id equals qty.StyleId
        //                    where qty.Qty > 50
        //                    orderby qty.Qty descending
        //                    where !s.Deleted
        //                    select new
        //                    {
        //                        StyleId = s.Id,
        //                        StyleString = s.StyleID,
        //                        ParentASIN = sCache.AssociatedASIN,
        //                        Market = sCache.AssociatedMarket,
        //                        MarketplaceId = sCache.AssociatedMarketplaceId,
        //                        Qty = qty.Qty
        //                    };

        //        var styleInfoList = query
        //            .Where(p => p.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
        //            .Take(500)
        //            .ToList();

        //        var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.eBay).ToList();

        //        var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

        //        IList<MessageString> messages;
        //        foreach (var styleInfo in newStyleInfoes)
        //        {
        //            _log.Info("Creating styleId=" + styleInfo.StyleString + " (" + styleInfo.StyleId + ")" + ", ASIN=" +
        //                      styleInfo.ParentASIN);
        //            if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
        //            {
        //                _log.Info("Skipped, style already exists");
        //                continue;
        //            }

        //            var model = CreateFromParentASIN(db,
        //                styleInfo.ParentASIN,
        //                styleInfo.Market.Value,
        //                styleInfo.MarketplaceId,
        //                false,
        //                0,
        //                out messages);

        //            if (model == null || model.Variations == null)
        //            {
        //                _log.Info("Skipped, variations is NULL");
        //                continue;
        //            }

        //            if (model.Variations.Count == 0)
        //            {
        //                _log.Info("Skipped, no variations");
        //                continue;
        //            }

        //            if (model.Variations.Select(v => v.StyleId).Distinct().Count() > 1)
        //            {
        //                _log.Info("Skipped multi-color variation");
        //                continue;
        //            }

        //            if (model.Variations.Count > 12)
        //            {
        //                _log.Info("Skipped, a lot of variations");
        //                continue;
        //            }

        //            model.Market = (int)MarketType.eBay;
        //            model.MarketplaceId = null;

        //            PrepareData(model);
        //            Save(model, null, db, _time.GetAppNowTime(), null);

        //            //Add to exist list the new items
        //            existMarketItems.AddRange(model.Variations.Select(i => new Item()
        //            {
        //                StyleId = i.StyleId
        //            }));
        //        }
        //    }
        //}

        //public void CreateWalmartListings(IWalmartOpenApi api, IBarcodeService barcodeService)
        //{
        //    _log.Info("CreateWalmartListings");
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
        //                           group siCache by siCache.StyleId
        //            into byStyle
        //                           select new
        //                           {
        //                               StyleId = byStyle.Key,
        //                               OneHasStrongQty = byStyle.Any(s => s.RemainingQuantity > 20), //NOTE: >20
        //                               Qty = byStyle.Sum(s => s.RemainingQuantity)
        //                           };

        //        var fromDate = new DateTime(2017, 12, 12);
        //        var query = from s in db.Styles.GetAll()
        //                    join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
        //                    join qty in groupByStyle on s.Id equals qty.StyleId
        //                    where qty.Qty > 30
        //                        && qty.OneHasStrongQty
        //                    //where s.CreateDate >= fromDate
        //                    orderby qty.Qty descending
        //                    where !s.Deleted
        //                    select new
        //                    {
        //                        StyleId = s.Id,
        //                        StyleString = s.StyleID,
        //                        ParentASIN = sCache.AssociatedASIN,
        //                        Market = sCache.AssociatedMarket,
        //                        MarketplaceId = sCache.AssociatedMarketplaceId,
        //                        Qty = qty.Qty,
        //                        Name = s.Name,
        //                        MainLicense = sCache.MainLicense,
        //                    };

        //        var styleInfoList = query
        //            .Where(p => p.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
        //                && !p.Name.Contains("Saras")
        //                && !p.Name.Contains("Sara's")
        //                && !p.Name.Contains("Widgeon")
        //                && p.MainLicense != "Widgeon"
        //                && p.MainLicense != "Saras Prints")
        //            .ToList();


        //        var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.Walmart).ToList();

        //        var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

        //        IList<MessageString> messages;
        //        foreach (var styleInfo in newStyleInfoes)
        //        {
        //            _log.Info("Creating styleId=" + styleInfo.StyleString + " (" + styleInfo.StyleId + ")" + ", ASIN=" +
        //                      styleInfo.ParentASIN);
        //            if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
        //            {
        //                _log.Info("Skipped, style already exists");
        //                continue;
        //            }

        //            var model = CreateFromParentASIN(db,
        //                styleInfo.ParentASIN,
        //                styleInfo.Market.Value,
        //                styleInfo.MarketplaceId,
        //                false,
        //                5,
        //                out messages);

        //            if (model == null)
        //            {
        //                var item = db.Items.GetAllViewAsDto(MarketType.Walmart, "")
        //                        .FirstOrDefault(i => i.StyleId == styleInfo.StyleId);
        //                if (item != null)
        //                {
        //                    model = CreateFromStyle(db,
        //                        styleInfo.StyleString,
        //                        null,
        //                        MarketType.Walmart,
        //                        null,
        //                        out messages);

        //                    model.Variations.ToList().ForEach(v => v.CurrentPrice = item.CurrentPrice);
        //                    model.Variations.ToList().ForEach(v => v.Color = item.Color);
        //                }
        //            }

        //            if (model == null || model.Variations == null)
        //            {
        //                _log.Info("Skipped, variations is NULL");
        //                continue;
        //            }

        //            if (model.Variations.Count == 0)
        //            {
        //                _log.Info("Skipped, no variations");
        //                continue;
        //            }

        //            if (model.Variations.Count > 10)
        //            {
        //                _log.Info("Skipped, a lot of variations");
        //                continue;
        //            }

        //            model.Market = (int)MarketType.Walmart;
        //            model.MarketplaceId = null;

        //            PrepareData(model);

        //            var existAnyBarcode = false;
        //            foreach (var variant in model.Variations)
        //            {
        //                var foundItems =((WalmartOpenApi)api).SearchProductsByBarcode(variant.Barcode, WalmartUtils.ApparelCategoryId);
        //                if (foundItems.IsSuccess && foundItems.Total > 0)
        //                {
        //                    existAnyBarcode = true;
        //                }
        //            }

        //            if (!existAnyBarcode)
        //            {
        //                foreach (var variant in model.Variations)
        //                {
        //                    var barcodeInfo = barcodeService.AssociateBarcodes(variant.SKU, _time.GetAppNowTime(), null);
        //                    if (barcodeInfo != null)
        //                    {
        //                        _log.Info("Set custom barcode for: " + variant.SKU + " :" + variant.Barcode + "=>" + barcodeInfo.Barcode);
        //                        variant.Barcode = barcodeInfo.Barcode;
        //                    }
        //                }
        //            }

        //            Save(model, null, db, _time.GetAppNowTime(), null);

        //            _emailService.SendSystemEmailToAdmin("Auto-created Walmart listing, ParentASIN: " + model.ASIN, "");

        //            //Add to exist list the new items
        //            existMarketItems.AddRange(model.Variations.Select(i => new Item()
        //            {
        //                StyleId = i.StyleId
        //            }));
        //        }
        //    }
        //}

        //public void CreateWalmartCAListings()
        //{
        //    _log.Info("CreateWalmartCAListings");
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
        //                           group siCache by siCache.StyleId
        //            into byStyle
        //                           select new
        //                           {
        //                               StyleId = byStyle.Key,
        //                               OneHasStrongQty = byStyle.Any(s => s.RemainingQuantity > 20), //NOTE: >20
        //                               Qty = byStyle.Sum(s => s.RemainingQuantity)
        //                           };

        //        var query = from s in db.Styles.GetAll()
        //                    join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
        //                    join qty in groupByStyle on s.Id equals qty.StyleId
        //                    where qty.Qty > 50
        //                        && qty.OneHasStrongQty
        //                    orderby qty.Qty descending
        //                    where !s.Deleted
        //                    select new
        //                    {
        //                        StyleId = s.Id,
        //                        StyleString = s.StyleID,
        //                        ParentASIN = sCache.AssociatedASIN,
        //                        Market = sCache.AssociatedMarket,
        //                        MarketplaceId = sCache.AssociatedMarketplaceId,
        //                        Qty = qty.Qty,
        //                        Name = s.Name,
        //                        MainLicense = sCache.MainLicense,
        //                    };

        //        var styleInfoList = query
        //            .Where(p => p.Market != (int)MarketType.None //Is online
        //                && !p.Name.Contains("Saras")
        //                && !p.Name.Contains("Sara's")
        //                && !p.Name.Contains("Widgeon")
        //                && p.MainLicense != "Widgeon"
        //                && p.MainLicense != "Saras Prints")
        //            .ToList();

        //        var sourceMarket = MarketType.Walmart;
        //        string sourceMakretplaceId = null;

        //        var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.WalmartCA).ToList();
        //        var existStyleIdList = existMarketItems.Select(i => i.StyleId).ToList();
        //        var existParentASINs = db.ParentItems.GetAll()
        //            .Where(i => i.Market == (int) MarketType.WalmartCA)
        //            .Select(i => i.ASIN)
        //            .ToList();

        //        //var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

        //        IList<MessageString> messages;

        //        var childItemCountQuery = from ch in db.Items.GetAll()
        //            where ch.Market == (int) sourceMarket
        //            group ch by ch.ParentASIN
        //            into byParentASIN
        //            select new
        //            {
        //                ParentASIN = byParentASIN.Key,
        //                ChildCount = byParentASIN.Count()
        //            };

        //        var sourceParentItems = (from pi in db.ParentItems.GetAll()
        //                                join chQty in childItemCountQuery on pi.ASIN equals chQty.ParentASIN
        //                                where pi.Market == (int)sourceMarket
        //                                orderby chQty.ChildCount descending, pi.CreateDate ascending
        //                                select pi).ToList();
        //        var sourceItems = db.Items.GetAll().Where(i => i.Market == (int)sourceMarket).ToList();

        //        foreach (var sourceParenItem in sourceParentItems)
        //        {
        //            _log.Info("Creating, Parent ASIN=" + sourceParenItem.ASIN);
        //            var sourceStyleIdList = sourceItems.Where(i => i.ParentASIN == sourceParenItem.ASIN)
        //                .Select(i => i.StyleId)
        //                .ToList();

        //            if (existParentASINs.Any(pi => pi == sourceParenItem.ASIN))
        //            {
        //                _log.Info("Skipped, ParentASIN already exists");
        //                continue;
        //            }

        //            //If not multilistings, check styleId for existing
        //            if (sourceStyleIdList.Distinct().Count() == 1)
        //            {
        //                var styleId = sourceStyleIdList.First();
        //                if (existStyleIdList.Contains(styleId))
        //                {
        //                    _log.Info("Skipped, Not multilisting, styleId already exists");
        //                    continue;
        //                }
        //            }

        //            var model = CreateFromParentASIN(db,
        //                sourceParenItem.ASIN,
        //                (int)sourceMarket,
        //                sourceMakretplaceId,
        //                false,
        //                10,
        //                out messages);
        //            model.ASIN = sourceParenItem.ASIN;

        //            //if (model == null)
        //            //{
        //            //    var item = db.Items.GetAllViewAsDto(MarketType.WalmartCA, "")
        //            //            .FirstOrDefault(i => i.StyleId == styleInfo.StyleId);
        //            //    if (item != null)
        //            //    {
        //            //        model = CreateFromStyle(db,
        //            //            styleInfo.StyleString,
        //            //            MarketType.WalmartCA,
        //            //            null,
        //            //            out messages);

        //            //        model.Variations.ToList().ForEach(v => v.CurrentPrice = PriceHelper.Convert(item.CurrentPrice, 1 / PriceHelper.CADtoUSD));
        //            //        model.Variations.ToList().ForEach(v => v.Color = item.Color);
        //            //    }
        //            //}

        //            if (model == null || model.Variations == null)
        //            {
        //                _log.Info("Skipped, variations is NULL");
        //                continue;
        //            }

        //            if (model.Variations.Count == 0)
        //            {
        //                _log.Info("Skipped, no variations");
        //                continue;
        //            }

        //            //if (model.Variations.Count > 10)
        //            //{
        //            //    _log.Info("Skipped, a lot of variations");
        //            //    continue;
        //            //}

        //            var resultStyleIdsList = model.Variations.Select(v => v.StyleId).Distinct().ToList();
        //            if (resultStyleIdsList.Count() == 1)
        //            {
        //                var styleId = resultStyleIdsList.First();
        //                if (existStyleIdList.Contains(styleId))
        //                {
        //                    _log.Info("Skipped, After filter by qty, not multilisting, styleId already exists");
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                var existStyleCount = resultStyleIdsList.Count(rs => existStyleIdList.Contains(rs));
        //                if (existStyleCount > resultStyleIdsList.Count()/(decimal)2)
        //                {
        //                    _log.Info("Skipped, After filter by qty, multilitsting has >1/2 styleIds already exists");
        //                    continue;
        //                }
        //            }

        //            model.Market = (int)MarketType.WalmartCA;
        //            model.MarketplaceId = null;

        //            PrepareData(model);
        //            Save(model, null, db, _time.GetAppNowTime(), null);

        //            //if (!_isDebug)
        //            //    _emailService.SendSystemEmailToAdmin("Auto-created Walmart listing, ParentASIN: " + model.ASIN, "");

        //            //Add to exist list the new items
        //            existMarketItems.AddRange(model.Variations.Select(i => new Item()
        //            {
        //                StyleId = i.StyleId
        //            }));

        //            existStyleIdList.AddRange(model.Variations.Select(i => i.StyleId).ToList());
        //        }


        //        //foreach (var styleInfo in newStyleInfoes)
        //        //{
        //        //    _log.Info("Creating styleId=" + styleInfo.StyleString + " (" + styleInfo.StyleId + ")" + ", ASIN=" +
        //        //              styleInfo.ParentASIN);
        //        //    if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
        //        //    {
        //        //        _log.Info("Skipped, style already exists");
        //        //        continue;
        //        //    }

        //        //    var sourceMarket = MarketType.Walmart;
        //        //    var sourceMakretplaceId = "";

        //        //    var model = CreateFromParentASIN(db,
        //        //        ,
        //        //        sourceMarket,
        //        //        sourceMakretplaceId,
        //        //        false,
        //        //        10,
        //        //        out messages);

        //        //    if (model == null)
        //        //    {
        //        //        var item = db.Items.GetAllViewAsDto(MarketType.WalmartCA, "")
        //        //                .FirstOrDefault(i => i.StyleId == styleInfo.StyleId);
        //        //        if (item != null)
        //        //        {
        //        //            model = CreateFromStyle(db,
        //        //                styleInfo.StyleString,
        //        //                MarketType.WalmartCA,
        //        //                null,
        //        //                out messages);

        //        //            model.Variations.ToList().ForEach(v => v.CurrentPrice = PriceHelper.Convert(item.CurrentPrice, 1/PriceHelper.CADtoUSD));
        //        //            model.Variations.ToList().ForEach(v => v.Color = item.Color);
        //        //        }
        //        //    }

        //        //    if (model == null || model.Variations == null)
        //        //    {
        //        //        _log.Info("Skipped, variations is NULL");
        //        //        continue;
        //        //    }

        //        //    if (model.Variations.Count == 0)
        //        //    {
        //        //        _log.Info("Skipped, no variations");
        //        //        continue;
        //        //    }

        //        //    if (model.Variations.Count > 10)
        //        //    {
        //        //        _log.Info("Skipped, a lot of variations");
        //        //        continue;
        //        //    }

        //        //    model.Market = (int)MarketType.WalmartCA;
        //        //    model.MarketplaceId = null;

        //        //    PrepareData(model);
        //        //    Save(model, null, db, _time.GetAppNowTime(), null);

        //        //    if (!_isDebug)
        //        //        _emailService.SendSystemEmailToAdmin("Auto-created Walmart listing, ParentASIN: " + model.ASIN, "");

        //        //    //Add to exist list the new items
        //        //    existMarketItems.AddRange(model.Variations.Select(i => new Item()
        //        //    {
        //        //        StyleId = i.StyleId
        //        //    }));
        //        //}
        //    }
        //}
    }
}
