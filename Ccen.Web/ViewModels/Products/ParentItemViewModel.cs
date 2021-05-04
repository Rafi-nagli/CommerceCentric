using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Cache;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Utils;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchResults;
using Amazon.Web.ViewModels.Orders;

namespace Amazon.Web.ViewModels.Products
{
    public class ParentItemViewModel
    {
        public int Id { get; set; }

        public MarketType? Market { get; set; }
        public string MarketplaceId { get; set; }

        public string ImageSource { get; set; }
        public string AmazonName { get; set; }
        public string ManualImage { get; set; }

        public string FirstStyleString { get; set; }
        public string ASIN { get; set; }
        public string SKU { get; set; }
        public bool OnHold { get; set; }
        public bool LockMarketUpdate { get; set; }
        public DateTime? PublishRequestedDate { get; set; }

        public string Currency 
        {
            get
            {
                return PriceHelper.GetCurrencySymbol((Market ?? MarketType.Amazon), MarketplaceId);
            }
        }


        public IList<ItemShortInfoViewModel> ChildItems { get; set; }
        public IList<StyleItemInfoViewModel> StyleItems { get; set; }
        
        public float MarketIndex
        {
            get { return MarketHelper.GetMarketIndex((MarketType)(Market ?? (int)MarketType.None), MarketplaceId); }
        }

        public string MarketShortName
        {
            get { return MarketHelper.GetShortName((int)(Market ?? MarketType.None), MarketplaceId); }
        }

        public int? Rank { get; set; }
        public string Positions { get; set; }
        public string Comment { get; set; }
        public string CommentByName { get; set; }
        public DateTime? CommentDate { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public DateTime? LastChildOpenDate { get; set; }

        public int TotalRemaining
        {
            get
            {
                if (ChildItems != null)
                    return ChildItems.Sum(ci => ci.RemainingQuantity ?? 0);
                return 0;
            }
        }


        public decimal? ChangePriceOffset { get; set; }

        public decimal? MinChildPrice { get; set; }
        public decimal? MaxChildPrice { get; set; }

        public string PriceRange
        {
            get
            {
                var from = MinChildPrice;
                var to = MaxChildPrice;

                if (from == null && to == null)
                    return "-";
                if (from == null)
                    return Currency + to;
                if (to == null)
                    return Currency + from;

                if (from == to)
                    return Currency + from;
                else
                    return Currency + from + " - " + Currency + to;
            }
        }

        public int PublishedStatus
        {
            get
            {
                return (ChildItems != null && ChildItems.Any())
                    ? (int) ChildItems.Min(i => i.PublishedStatus)
                    : (int)PublishedStatuses.None;
            }
        }

        public string FormattedPublishedStatus
        {
            get { return PublishedStatusesHelper.GetName((PublishedStatuses)PublishedStatus); }
        }

        public bool? IsAmazonUpdated { get; set; }
        public bool? HasChildWithFakeParentASIN { get; set; }

        public bool HasChildWithDefect
        {
            get { return ChildItems != null && ChildItems.Any(ch => ch.HasDefects); }
        }
        
        public bool? HasPriceDifferencesWithAmazon { get; set; }
        public bool? HasQuantityDifferencesWithAmazon { get; set; }

        
        public bool HasImage
        {
            get { return !String.IsNullOrEmpty(ImageSource); }
        }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(ImageSource, 75, 75, false, ImageHelper.NO_IMAGE_URL, false, convertInDomainUrlToThumbnail: true);
            }
        }


        public string ProductUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(ASIN) && Market.HasValue)
                    return UrlHelper.GetProductUrl(ASIN, (MarketType)Market, MarketplaceId);
                return "";
            }
        }

        public string[] SourceMarketIdList
        {
            get
            {
                if (ChildItems != null)
                {
                    return ChildItems.Select(i => i.SourceMarketId).Where(i => i != null).Distinct().ToArray();
                }
                return new string[] {};
            }
        }


        public string SourceMarketId { get; set; }

        public string SourceMarketIdIncludeChild
        {
            get
            {
                if (!String.IsNullOrEmpty(SourceMarketId))
                    return SourceMarketId;

                if (ChildItems != null && ChildItems.Any())
                {
                    return ChildItems.Select(i => i.SourceMarketId).FirstOrDefault(i => !String.IsNullOrEmpty(i));
                }
                return null;
            }
        }

        public string MarketUrl
        {
            get
            {
                if (Market == MarketType.Walmart
                    || Market == MarketType.WalmartCA)
                {
                    return UrlHelper.GetMarketUrl(ASIN, SourceMarketIdIncludeChild, (MarketType)Market, MarketplaceId);
                }
                if (Market.HasValue)
                    return UrlHelper.GetMarketUrl(ASIN, SourceMarketId,(MarketType)Market, MarketplaceId);
                return "";
            }
        }

        public string SellerMarketUrl
        {
            get
            {
                if (Market == MarketType.Magento 
                    || Market == MarketType.Walmart
                    || Market == MarketType.WalmartCA
                    || Market == MarketType.Jet 
                    || Market == MarketType.eBay)
                    return UrlHelper.GetSellarCentralInventoryUrl(SourceMarketId, (MarketType)Market, MarketplaceId);
                return UrlHelper.GetSellarCentralInventoryUrl(ASIN, (MarketType)Market, MarketplaceId);
            }
        }

        public override string ToString()
        {
            return "Id=" + Id 
                + ", ASIN=" + ASIN 
                + ", ChildSizes=" + (ChildItems != null ? ChildItems.Count.ToString() : "[null]"); // + ", StyleId=" + StyleId + ", StyleString=" + StyleString;
        }

        //public static ProductSearchResult GetIdListByFilters(IUnitOfWork db,
        //    ProductSearchFilterViewModel filter)
        //{
        //    var result = new ProductSearchResult();

        //    if (filter.NoneSoldPeriod.HasValue && filter.NoneSoldPeriod > 0)
        //    {
        //        //NOTE: using styleId to hide parent item with that style but without sold
        //        //получаем список продающихся styleId
        //        //выводим список item у которых styleId not in Sold Style List
        //        var fromDate = DateTime.Today.AddDays(-filter.NoneSoldPeriod.Value);
        //        var soldStyleIds = db.StyleCaches.GetAll()
        //            .Where(sc => sc.LastSoldDateOnMarket < fromDate)
        //            .GroupBy(sc => sc.Id)
        //            .Select(gsc => gsc.Key).ToList();

        //        result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, soldStyleIds);
        //    }

        //    if (filter.Gender.HasValue)
        //    {
        //        var genderStyleIds = db.StyleCaches
        //            .GetAll()
        //            .Where(s => s.Gender == filter.Gender.Value.ToString())
        //            .Select(s => s.Id).ToList();

        //        result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, genderStyleIds);
        //    }

        //    if (filter.MainLicense.HasValue)
        //    {
        //        var mainLicenseStyleIds = db.StyleCaches
        //            .GetAll()
        //            .Where(s => s.MainLicense == filter.MainLicense.Value.ToString())
        //            .Select(s => s.Id)
        //            .ToList();

        //        result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, mainLicenseStyleIds);
        //    }

        //    if (filter.SubLicense.HasValue)
        //    {
        //        var subLicenseStyleIds = db.StyleCaches
        //            .GetAll()
        //            .Where(s => s.SubLicense == filter.SubLicense.Value.ToString())
        //            .Select(s => s.Id)
        //            .ToList();

        //        result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, subLicenseStyleIds);
        //    }

        //    if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
        //    {
        //        var priceQuery = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)filter.Market);
        //        if (!String.IsNullOrEmpty(filter.MarketplaceId))
        //            priceQuery = priceQuery.Where(p => p.MarketplaceId == filter.MarketplaceId);

        //        if (filter.MinPrice.HasValue)
        //            priceQuery = priceQuery.Where(p => p.CurrentPrice >= filter.MinPrice.Value);

        //        if (filter.MaxPrice.HasValue)
        //            priceQuery = priceQuery.Where(p => p.CurrentPrice <= filter.MaxPrice.Value);

        //        result.ChildItemIdList = priceQuery.Select(p => p.Id).ToList();
        //    }

        //    return result;
        //}

        //public static IEnumerable<ListingDefectDTO> GetListingDefects(ILogService log,
        //    IDbFactory dbFactory)
        //{
        //    using (var db = dbFactory.GetRDb())
        //    {
        //        log.Debug("GetAll Info begin");
        //        var defectList = db.ListingDefects.GetAllAsDto()
        //            .Where()
        //        log.Debug("GetAll Info end");
        //    }
        //}

        public static GridResponse<ParentItemViewModel> GetAll(ILogService log, 
            IDbCacheService cacheService,
            IDbFactory dbFactory, 
            bool clearCache,
            ProductSearchFilterViewModel filter)
        {
            if (filter.LimitCount == 0)
                filter.LimitCount = 50;

            using (var db = dbFactory.GetRDb())
            {
                log.Debug("GetAll begin");

                var forceStyleImage = filter.MarketplaceId == MarketplaceKeeper.AmazonAuMarketplaceId
                    || filter.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId;
                var gridResponse = db.ParentItems.GetWithChildSizesWithPaging(log, 
                    cacheService,
                    clearCache,
                    forceStyleImage,
                    filter.GetDto());

                var results = (from p in gridResponse.Items
                    select new ParentItemViewModel 
                    {
                        Id = p.Id,
                        Market = (MarketType)p.Market,
                        MarketplaceId = p.MarketplaceId,

                        SourceMarketId = p.SourceMarketId,

                        ImageSource = p.ImageSource,
                        AmazonName = p.AmazonName,

                        FirstStyleString = GetFirstStyle(p.ChildItems, p.ASIN),
                        ASIN = p.ASIN,
                        SKU = p.SKU,
                        OnHold = p.OnHold,
                        LockMarketUpdate = p.LockMarketUpdate,
                        PublishRequestedDate = p.PublishRequestedDate,

                        ChildItems = p.ChildItems
                            .Select(i => new ItemShortInfoViewModel(p.Market, p.MarketplaceId, i))
                            .ToList(),

                        StyleItems = p.StyleItems.Select(i => new StyleItemInfoViewModel(i))
                            .ToList(),

                        Rank = p.Rank,
                        Positions = p.PositionsInfo != null ? p.PositionsInfo.Replace(";", ",<br/>") : String.Empty,
                        Comment = p.LastComment?.Message,
                        CommentByName = p.LastComment?.CreatedByName,
                        CommentDate = p.LastComment?.CreateDate,

                        HasPriceDifferencesWithAmazon = p.HasPriceDifferencesWithAmazon,
                        HasQuantityDifferencesWithAmazon = p.HasQuantityDifferencesWithAmazon,
                        HasChildWithFakeParentASIN = p.HasChildWithFakeParentASIN,

                        LastChildOpenDate = p.LastChildOpenDate,
                        MinChildPrice = p.MinChildPrice,
                        MaxChildPrice = p.MaxChildPrice,

                        IsAmazonUpdated = p.IsAmazonUpdated,
                    }).ToList();

                log.Debug("GetAll end");

                return new GridResponse<ParentItemViewModel>(results.OrderBy(r => r.FirstStyleString).ToList(), gridResponse.TotalCount);
            }
        }

        public ParentItemViewModel()
        {
            Comments = new List<CommentViewModel>();
        }

        public ParentItemViewModel(IUnitOfWork db, ParentItemDTO item)
        {
            Id = item.Id;

            ASIN = item.ASIN;
            Market = (MarketType)item.Market;
            MarketplaceId = item.MarketplaceId;

            SourceMarketId = item.SourceMarketId;

            SKU = item.SKU;
            OnHold = item.OnHold;
            LockMarketUpdate = item.LockMarketUpdate;

            AmazonName = item.AmazonName;
            ImageSource = item.ImageSource;
            ManualImage = item.ManualImage;
            FirstStyleString = GetFirstStyle(item.ChildItems, item.ASIN);
            Comment = item.LastComment?.Message;

            ChildItems = item.ChildItems != null ? 
                item.ChildItems.Select(i => new ItemShortInfoViewModel(item.Market, item.MarketplaceId, i)).ToList() : new List<ItemShortInfoViewModel>();

            Comments = db.ProductComments.GetCommentsForProductId(item.Id).OrderBy(c => c.UpdateDate)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Comment = c.Message
                }).ToList();

            IsAmazonUpdated = item.IsAmazonUpdated;
        }

        private static string GetFirstStyle(IList<ItemShortInfoDTO> items, string asin)
        {
            if (String.IsNullOrEmpty(asin)) //For [empty] record
                return "";
            return items != null && items.Count > 0 ? items.First().StyleString : "";
        }
        
        public void Update(IUnitOfWork db, 
            ILogService log, 
            ISystemActionService actionService,
            IPriceManager priceManager,
            MarketType market,
            string marketplaceId, 
            DateTime when, 
            long? by)
        {
            var parentItem = db.ParentItems.Get(Id);
            if (parentItem != null)
            {
                var childItems = db.Items.GetListingsByParentASIN(market, marketplaceId, parentItem.ASIN);

                if (ChangePriceOffset.HasValue)
                {
                    foreach (var childItem in childItems)
                    {
                        log.Info("Child price was changed, from=" + childItem.CurrentPrice + ", to=" +
                                 (childItem.CurrentPrice + ChangePriceOffset.Value));

                        var oldPrice = childItem.CurrentPrice;
                        childItem.CurrentPrice += ChangePriceOffset.Value;
                        childItem.CurrentPriceInUSD = PriceHelper.RougeConvertToUSD(childItem.CurrentPriceCurrency, childItem.CurrentPrice);
                        childItem.PriceUpdateRequested = true;

                        priceManager.LogListingPrice(db,
                            PriceChangeSourceType.ParentItemOffset,
                            childItem.Id,
                            childItem.SKU,
                            childItem.CurrentPrice,
                            oldPrice,
                            when,
                            by);
                    }
                    db.Commit();
                }
                
                parentItem.SKU = SKU;
                parentItem.OnHold = OnHold;
                parentItem.LockMarketUpdate = LockMarketUpdate;

                if (parentItem.ManualImage != ManualImage)
                {
                    log.Info("Image changed: " + parentItem.ManualImage + " => " + ManualImage);
                    parentItem.ManualImage = ManualImage;

                    if (!MarketHelper.IsAmazon((MarketType)parentItem.Market))
                    {
                        foreach (var child in childItems)
                        {
                            var newAction = new SystemActionDTO()
                            {
                                ParentId = null,
                                Status = (int)SystemActionStatus.None,
                                Type = (int)SystemActionType.UpdateOnMarketProductImage,
                                Tag = child.Id.ToString(),
                                InputData = null,

                                CreateDate = when,
                                CreatedBy = null,
                            };
                            db.SystemActions.AddAction(newAction);
                        }
                    }
                }

                parentItem.UpdateDate = when;
                parentItem.UpdatedBy = by;

                db.Commit();

                var lastComment = db.ProductComments.UpdateComments(
                    Comments.Select(c => new CommentDTO()
                    {
                        Id = c.Id,
                        Message = c.Comment
                    }).ToList(),
                    Id,
                    when, 
                    by);

                Comment = lastComment != null ? lastComment.Message : "";
            }
        }
    }
}