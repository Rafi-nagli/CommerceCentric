using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Cache;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Entities.Users;
using Amazon.Core.Models.Caches;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class ParentItemRepository : Repository<ParentItem>, IParentItemRepository
    {
        public ParentItemRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void UpdateParent(string currentASIN,
            ParentItemDTO dto,
            MarketType market,
            string marketplaceId,
            DateTime? when)
        {
            var parentItem = unitOfWork.GetSet<ParentItem>().FirstOrDefault(i =>
                i.ASIN == currentASIN
                && i.Market == (int)market
                && i.MarketplaceId == marketplaceId);

            if (parentItem != null)
            {
                UpdateParentItemInfo(parentItem, dto, when);
                unitOfWork.Commit();
            }
        }


        public ParentItem CreateOrUpdateParent(ParentItemDTO dto, 
            MarketType market, 
            string marketplaceId, 
            DateTime? when)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var parentItem = unitOfWork.GetSet<ParentItem>().FirstOrDefault(i => 
                i.ASIN == dto.ASIN
                && i.Market == (int)market
                && i.MarketplaceId == marketplaceId);

            if (parentItem == null)
            {
                parentItem = new ParentItem
                {
                    ASIN = dto.ASIN,
                    Market = (int)market,
                    MarketplaceId = marketplaceId,
                    SourceMarketId = dto.ASIN,

                    AmazonName = dto.AmazonName,
                    SKU = dto.SKU,
                    ImageSource = dto.ImageSource,

                    Rank = dto.Rank,
                    RankUpdateDate = when,

                    //Additional fields
                    BrandName = dto.BrandName,
                    Type = dto.Type,
                    ListPrice = dto.ListPrice,
                    Color = dto.Color,
                    Department = dto.Department,
                    Features = dto.Features,
                    AdditionalImages = dto.AdditionalImages,
                    SearchKeywords = dto.SearchKeywords,

                    //System
                    CreateDate = when,
                    IsAmazonUpdated = dto.IsAmazonUpdated,
                    LastUpdateFromAmazon = dto.LastUpdateFromAmazon
                };
                Add(parentItem);
            }
            else
            {
                UpdateParentItemInfo(parentItem, dto, when);
            }

            unitOfWork.Commit();

            return parentItem;
        }

        private void UpdateParentItemInfo(ParentItem dbParentItem, 
            ParentItemDTO dtoParentItem,
            DateTime? when)
        {
            //if success requested, override all
            if (dtoParentItem != null && dtoParentItem.IsAmazonUpdated == true)
            {
                dbParentItem.ASIN = dtoParentItem.ASIN;
                if (String.IsNullOrEmpty(dbParentItem.SKU))
                    dbParentItem.SKU = dtoParentItem.SKU;

                dbParentItem.AmazonName = dtoParentItem.AmazonName;
                if (!String.IsNullOrEmpty(dtoParentItem.ImageSource))
                    dbParentItem.ImageSource = dtoParentItem.ImageSource;
                dbParentItem.Description = dtoParentItem.Description;

                dbParentItem.SourceMarketId = dtoParentItem.ASIN;

                if (dtoParentItem.Rank != null)
                {
                    dbParentItem.Rank = dtoParentItem.Rank;
                    dbParentItem.RankUpdateDate = when;
                }

                //Additional fields,
                dbParentItem.BrandName = dtoParentItem.BrandName;
                dbParentItem.Type = dtoParentItem.Type;
                dbParentItem.ListPrice = dtoParentItem.ListPrice;
                dbParentItem.Color = dtoParentItem.Color;
                dbParentItem.Department = dtoParentItem.Department;
                dbParentItem.AdditionalImages = dtoParentItem.AdditionalImages;

                if (!String.IsNullOrEmpty(dtoParentItem.Features))
                    dbParentItem.Features = dtoParentItem.Features;
                if (!String.IsNullOrEmpty(dtoParentItem.SearchKeywords))
                    dbParentItem.SearchKeywords = dtoParentItem.SearchKeywords;

                //System
                dbParentItem.IsAmazonUpdated = true;
                dbParentItem.LastUpdateFromAmazon = dtoParentItem.LastUpdateFromAmazon; //Only if success update
            }
            else
            {
                //System
                dbParentItem.IsAmazonUpdated = false;
            }
            dbParentItem.UpdateDate = when;
        }

        public ParentItem FindOrCreateForItem(Item item, DateTime? when)
        {
            if (!ArgumentHelper.CheckMarket((MarketType?)item.Market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId((MarketType?)item.Market, item.MarketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var parentItem = unitOfWork.GetSet<ParentItem>().FirstOrDefault(i => 
                i.ASIN == item.ParentASIN
                && i.Market == (int)item.Market
                && i.MarketplaceId == item.MarketplaceId);

            if (parentItem == null)
            {
                parentItem = new ParentItem();
                parentItem.ASIN = item.ParentASIN;
                parentItem.Market = item.Market;
                parentItem.MarketplaceId = item.MarketplaceId;
                
                parentItem.AmazonName = item.Title;
                parentItem.ImageSource = item.PrimaryImage;

                parentItem.IsAmazonUpdated = false;
                parentItem.CreateDate = when;

                unitOfWork.GetSet<ParentItem>().Add(parentItem);
                unitOfWork.Commit();
            }

            return parentItem;
        }


        public IQueryable<ParentItemDTO> GetAllAsDto(MarketType market, string marketplaceId)
        {
            var query = GetAll().Where(pi => pi.Market == (int)market);
            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(pi => pi.MarketplaceId == marketplaceId);
            return AsDto(query);
        }

        public IQueryable<ParentItemDTO> GetAllAsDto()
        {
            var query = GetAll();
            return AsDto(query);
        }

        private IQueryable<ParentItemDTO> AsDto(IQueryable<ParentItem> query)
        {
            return query.Select(pi => new ParentItemDTO()
            {
                Id = pi.Id,

                Market = pi.Market,
                MarketplaceId = pi.MarketplaceId,

                ASIN = pi.ASIN,
                SKU = pi.SKU,
                SourceMarketId = pi.SourceMarketId,
                GroupId = pi.GroupId,

                OnHold = pi.OnHold,
                ForceEnableColorVariations = pi.ForceEnableColorVariations,

                AmazonName = pi.AmazonName,

                ImageSource = pi.ImageSource,
                ManualImage = pi.ManualImage,

                Rank = pi.Rank,
                BrandName = pi.BrandName,
                IsAutoParentDesc = pi.IsAutoParentDesc,
                Description = pi.Description,
                BulletPoint1 = pi.BulletPoint1,
                BulletPoint2 = pi.BulletPoint2,
                BulletPoint3 = pi.BulletPoint3,
                BulletPoint4 = pi.BulletPoint4,
                BulletPoint5 = pi.BulletPoint5,

                IsAmazonUpdated = pi.IsAmazonUpdated,
                LockMarketUpdate = pi.LockMarketUpdate,
            });
        }

        public bool AnyWithASIN(string asin)
        {
            return GetAll().Any(p => p.ASIN == asin);
        }
        
        public ParentItemDTO GetAsDTO(int id)
        {
            var query = from p in unitOfWork.GetSet<ParentItem>()
                where p.Id == id
                select p;
            return AsDto(query).FirstOrDefault();
        }

        public ParentItemDTO GetAsDTO(string asin, MarketType market, string marketplaceId)
        {
            var query = from p in unitOfWork.GetSet<ParentItem>()
                        where p.ASIN == asin
                            && p.Market == (int)market
                            && p.MarketplaceId == marketplaceId
                        select p;
            return AsDto(query).FirstOrDefault();
        }
        
        public List<ItemDTO> CheckForExistence(List<ItemDTO> parentItems, 
            MarketType market,
            string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var allParentList = GetAll()
                .Where(e => e.Market == (int)market
                   && e.MarketplaceId == marketplaceId)
                .Select(e => e.ASIN).ToList();

            return parentItems.Where(r => !allParentList.Contains(r.ParentASIN)).ToList();
        }

        private IList<ChildItemInfoCacheEntry> GetChildItemInfoList(DateTime? fromDate,
            MarketType market,
            string marketplaceId)
        {
            var childItemsQuery = from i in unitOfWork.GetSet<Item>()
                                  join l in unitOfWork.GetSet<Listing>() on i.Id equals l.ItemId
                                  join st in unitOfWork.GetSet<Style>() on i.StyleId equals st.Id into withStyle
                                  from st in withStyle.DefaultIfEmpty()
                                  join si in unitOfWork.GetSet<StyleItem>() on i.StyleItemId equals si.Id into withStyleItem
                                  from si in withStyleItem.DefaultIfEmpty()
                                  where !l.IsRemoved
                                        && l.Market == (int)market
                                        && (String.IsNullOrEmpty(marketplaceId) || l.MarketplaceId == marketplaceId)
                                        && (fromDate == null
                                            || i.UpdateDate > fromDate
                                            || l.UpdateDate > fromDate
                                            || st.UpdateDate > fromDate
                                            || st.ReSaveDate > fromDate
                                            || si.UpdateDate > fromDate)
                                  select new ChildItemInfoCacheEntry
                                  {
                                      Id = i.Id,

                                      AmazonRealQuantity = l.AmazonRealQuantity,
                                      RealQuantity = l.RealQuantity,
                                      DisplayQuantity = l.DisplayQuantity,

                                      ASIN = i.ASIN, //NOTE: for search filters
                                      ParentASIN = i.ParentASIN,
                                      SKU = l.SKU,
                                      Barcode = i.Barcode,

                                      SourceMarketId = i.SourceMarketId,

                                      StyleItemId = i.StyleItemId,
                                      StyleId = i.StyleId,

                                      StyleString = st.StyleID,
                                      StyleOnHold = st != null ? st.OnHold : false,

                                      StyleSize = si.Size,
                                      StyleColor = si.Color,
                                      StyleItemOnHold = si != null ? si.OnHold : false,

                                      IsFBA = l.IsFBA,
                                      IsDefault = l.IsDefault,
                                      OnHold = l.OnHold,

                                      PublishedStatus = i.ItemPublishedStatus,
                                      PublishedStatusFromMarket = i.ItemPublishedStatusFromMarket,
                                  };
            var childItems = childItemsQuery.AsNoTracking().ToList();

            //GET LISTING DEFECTS
            var listingsDefectsQuery = unitOfWork.ListingDefects.GetAllAsDto().Where(d => d.MarketType == (int)market
               && (String.IsNullOrEmpty(marketplaceId) || d.MarketplaceId == marketplaceId));
            var listingDefects = listingsDefectsQuery.ToList();

            foreach (var childItem in childItems)
            {
                childItem.ListingDefects = listingDefects.Where(d => d.SKU == childItem.SKU).ToList();
            }

            return childItems;
        }

        private IList<ChildItemInfoCacheEntry> GetChildItemInfoList(IList<string> parentASINList,
            MarketType market,
            string marketplaceId)
        {
            var childItemsQuery = from i in unitOfWork.GetSet<Item>()
                                  join l in unitOfWork.GetSet<Listing>() on i.Id equals l.ItemId
                                  join st in unitOfWork.GetSet<Style>() on i.StyleId equals st.Id into withStyle
                                  from st in withStyle.DefaultIfEmpty()
                                  join si in unitOfWork.GetSet<StyleItem>() on i.StyleItemId equals si.Id into withStyleItem
                                  from si in withStyleItem.DefaultIfEmpty()
                                  where !l.IsRemoved
                                        && l.Market == (int)market
                                        && (String.IsNullOrEmpty(marketplaceId) || l.MarketplaceId == marketplaceId)
                                        && (parentASINList.Contains(i.ParentASIN))
                                  select new ChildItemInfoCacheEntry
                                  {
                                      Id = i.Id,

                                      AmazonRealQuantity = l.AmazonRealQuantity,
                                      RealQuantity = l.RealQuantity,
                                      DisplayQuantity = l.DisplayQuantity,

                                      ASIN = i.ASIN, //NOTE: for search filters
                                      ParentASIN = i.ParentASIN,
                                      SKU = l.SKU,
                                      Barcode = i.Barcode,

                                      SourceMarketId = i.SourceMarketId,

                                      StyleItemId = i.StyleItemId,
                                      StyleId = i.StyleId,

                                      StyleString = st.StyleID,
                                      StyleOnHold = st != null ? st.OnHold : false,

                                      StyleSize = si.Size,
                                      StyleColor = si.Color,
                                      StyleItemOnHold = si != null ? si.OnHold : false,

                                      IsFBA = l.IsFBA,
                                      IsDefault = l.IsDefault,
                                      OnHold = l.OnHold,

                                      PublishedStatus = i.ItemPublishedStatus,
                                  };
            var childItems = childItemsQuery.AsNoTracking().ToList();

            return childItems;
        }

        private IList<StyleItemInfoCacheEntry> GetStyleItemInfoList(DateTime? fromDate, bool keepImage)
        {
            var styleItemQuery = from si in unitOfWork.GetSet<StyleItem>()
                                 join st in unitOfWork.GetSet<Style>() on si.StyleId equals st.Id
                                 join siCache in unitOfWork.GetSet<StyleItemCache>() on si.Id equals siCache.Id into withStCache
                                 from siCache in withStCache.DefaultIfEmpty()
                                 where !st.Deleted
                                     && (fromDate == null
                                        || si.UpdateDate > fromDate
                                        || st.UpdateDate > fromDate
                                        || st.ReSaveDate > fromDate
                                        || siCache.UpdateDate > fromDate)
                                 select new StyleItemInfoCacheEntry()
                                 {
                                     Id = si.Id,

                                     StyleId = si.StyleId,
                                     StyleString = st.StyleID,

                                     StyleOnHold = st.OnHold,

                                     StyleSize = si.Size,
                                     StyleColor = si.Color,
                                     StyleItemId = si.Id,
                                     OnHold = si.OnHold,

                                     RemainingQuantity = siCache.RemainingQuantity,

                                     Image = keepImage ? st.Image : null,

                                     MarketplacesInfo = siCache.MarketplacesInfo,
                                 };
            var styleItems = styleItemQuery.AsNoTracking().ToList();
            styleItems = styleItems.OrderBy(si => SizeHelper.GetSizeIndex(si.StyleSize)).ToList();

            return styleItems;
        }

        private IList<StyleItemInfoCacheEntry> GetStyleItemInfoList(IList<long> styleIdList, bool keepImage)
        {
            var styleItemQuery = from si in unitOfWork.GetSet<StyleItem>()
                                 join st in unitOfWork.GetSet<Style>() on si.StyleId equals st.Id
                                 join siCache in unitOfWork.GetSet<StyleItemCache>() on si.Id equals siCache.Id into withStCache
                                 from siCache in withStCache.DefaultIfEmpty()
                                 where !st.Deleted
                                     && styleIdList.Contains(st.Id)
                                 select new StyleItemInfoCacheEntry()
                                 {
                                     Id = si.Id,

                                     StyleId = si.StyleId,
                                     StyleString = st.StyleID,

                                     StyleOnHold = st.OnHold,

                                     StyleSize = si.Size,
                                     StyleColor = si.Color,
                                     StyleItemId = si.Id,
                                     OnHold = si.OnHold,

                                     RemainingQuantity = siCache.RemainingQuantity,

                                     Image = keepImage ? st.Image : null,

                                     MarketplacesInfo = siCache.MarketplacesInfo,
                                 };
            var styleItems = styleItemQuery.AsNoTracking().ToList();
            styleItems = styleItems.OrderBy(si => SizeHelper.GetSizeIndex(si.StyleSize)).ToList();

            return styleItems;
        }

        public GridResponse<ParentItemDTO> GetWithChildSizesWithPaging(ILogService log,
            IDbCacheService cacheService,
            bool clearCache,
            bool useStyleImage,
            ItemSearchFiltersDTO filters)
        {
            log.Debug("begin GetWithChildSizes");

            var baseParentQuery = from p in unitOfWork.GetSet<ParentItem>()
                                  where p.Market == (int)filters.Market
                                        && (String.IsNullOrEmpty(filters.MarketplaceId) || p.MarketplaceId == filters.MarketplaceId)
                                  select p;

            if (!String.IsNullOrEmpty(filters.Keywords)
                || !String.IsNullOrEmpty(filters.StyleName)
                || filters.StyleId.HasValue
                || filters.DropShipperId.HasValue
                || (filters.Genders != null && filters.Genders.Any())
                || !String.IsNullOrEmpty(filters.Brand)
                || filters.MinPrice.HasValue
                || filters.MaxPrice.HasValue
                || filters.PublishedStatus.HasValue
                || filters.Availability != (int)ProductAvailability.All)
            {
                var keywordQuery = from vi in unitOfWork.GetSet<ViewItem>()
                                   where vi.Market == (int)filters.Market
                                         && (String.IsNullOrEmpty(filters.MarketplaceId) || vi.MarketplaceId == filters.MarketplaceId)
                                   select vi;

                if (!String.IsNullOrEmpty(filters.Keywords))
                    keywordQuery = keywordQuery.Where(vi => vi.StyleString.Contains(filters.Keywords)
                                                            || vi.SKU.Contains(filters.Keywords)
                                                            || vi.ASIN.Contains(filters.Keywords)
                                                            || vi.ParentASIN.Contains(filters.Keywords)
                                                            || vi.Title.Contains(filters.Keywords));

                if (filters.StyleId.HasValue)
                    keywordQuery = keywordQuery.Where(vi => vi.StyleId == filters.StyleId);

                if (!String.IsNullOrEmpty(filters.StyleName))
                    keywordQuery = keywordQuery.Where(vi => vi.StyleString.Contains(filters.StyleName));

                if (filters.DropShipperId.HasValue)
                    keywordQuery = keywordQuery.Where(vi => vi.DropShipperId == filters.DropShipperId);

                if (filters.MinPrice.HasValue)
                    keywordQuery = keywordQuery.Where(vi => vi.CurrentPrice >= filters.MinPrice);

                if (filters.MaxPrice.HasValue)
                    keywordQuery = keywordQuery.Where(vi => vi.CurrentPrice <= filters.MaxPrice);

                if (filters.PublishedStatus.HasValue)
                    keywordQuery = keywordQuery.Where(vi => vi.ItemPublishedStatus == filters.PublishedStatus);

                if (filters.Availability == (int)ProductAvailability.InStock)
                    keywordQuery = keywordQuery.Where(vi => vi.RealQuantity > 0);

                if ((filters.Genders != null && filters.Genders.Any())
                    || !String.IsNullOrEmpty(filters.Brand))
                {
                    var featureQuery = from sc in unitOfWork.GetSet<StyleCache>()
                                       select sc;

                    if (filters.Genders != null && filters.Genders.Any())
                    {
                        var genderIds = filters.Genders.Select(g => g.ToString()).ToList();

                        featureQuery = from f in featureQuery
                                       where genderIds.Contains(f.Gender)
                                       select f;
                    }

                    if (!String.IsNullOrEmpty(filters.Brand))
                    {
                        featureQuery = from f in featureQuery
                                       where f.Brand == filters.Brand
                                       select f;
                    }

                    if (filters.MainLicense.HasValue)
                    {
                        var mainLicenseStr = filters.MainLicense.ToString();
                        featureQuery = from f in featureQuery
                                       where f.MainLicense == mainLicenseStr
                                       select f;
                    }

                    keywordQuery = from k in keywordQuery
                                   join f in featureQuery on k.StyleId equals f.Id
                                   select k;
                }

                var parentFilterQuery = keywordQuery.Select(vi => vi.ParentASIN).Distinct();

                baseParentQuery = from p in baseParentQuery
                                  join k in parentFilterQuery on p.ASIN equals k
                                  select p;
            }

            var totalCount = baseParentQuery.Count();

            baseParentQuery = baseParentQuery
                .OrderBy(p => p.Id)
                .Skip(filters.StartIndex)
                .Take(filters.LimitCount);

            var dropShipperInfo = from vi in unitOfWork.GetSet<ViewItem>()
                                  group vi by vi.ParentASIN into byParent
                                  select new
                                  {
                                      ASIN = byParent.Key,
                                      DropShipperId = byParent.Max(i => i.DropShipperId)
                                  };


            var parentItemQuery = from pi in baseParentQuery
                                  join dsInfo in dropShipperInfo on pi.ASIN equals dsInfo.ASIN into withDsInfo
                                  from dsInfo in withDsInfo.DefaultIfEmpty()
                                  join pCache in unitOfWork.GetSet<ParentItemCache>() on pi.Id equals pCache.Id into withPCache
                                  from pCache in withPCache.DefaultIfEmpty()
                                  join c in unitOfWork.GetSet<ViewActualProductComment>() on pi.Id equals c.ProductId into withC
                                  from c in withC.DefaultIfEmpty()
                                  join ds in unitOfWork.GetSet<DropShipper>() on dsInfo.DropShipperId equals ds.Id into withDs
                                  from ds in withDs.DefaultIfEmpty()

                                  select new ParentItemDTO
                                  {
                                      Id = pi.Id,
                                      Market = pi.Market,
                                      MarketplaceId = pi.MarketplaceId,

                                      DropShipperId = ds.Id,
                                      DropShipperName = ds.Name,

                                      AmazonName = pi.AmazonName,
                                      ImageSource = pi.ImageSource,

                                      SourceMarketId = pi.SourceMarketId,
                                      SourceMarketUrl = pi.SourceMarketUrl,

                                      ASIN = pi.ASIN,
                                      SKU = pi.SKU,
                                      OnHold = pi.OnHold,
                                      Rank = pi.Rank,
                                      LastComment = new CommentDTO()
                                      {
                                          Message = c.Message,
                                          CreateDate = c.CreateDate,
                                          CreatedBy = c.CreatedBy,
                                          CreatedByName = c.CreatedByName,
                                      },

                                      HasListings = pCache == null || pCache.HasListings,

                                      IsAmazonUpdated = pi.IsAmazonUpdated,

                                      HasPriceDifferencesWithAmazon = pCache != null ? pCache.HasPriceDifferences : false,
                                      HasQuantityDifferencesWithAmazon = pCache != null ? pCache.HasQtyDifferences : false,
                                      HasChildWithFakeParentASIN = pCache != null ? pCache.HasChildWithFakeParentASIN : false,

                                      LastChildOpenDate = pCache != null ? pCache.LastOpenDate : null,
                                      MinChildPrice = pCache != null ? pCache.MinPrice : null,
                                      MaxChildPrice = pCache != null ? pCache.MaxPrice : null,

                                      PositionsInfo = pCache != null ? pCache.PositionsInfo : null,
                                  };

            var parentItems = parentItemQuery.AsNoTracking().ToList();

            //if (filters.Market == (int)MarketType.Amazon
            //    || filters.Market == (int)MarketType.AmazonEU)
            //{
            //    parentItemQuery = parentItemQuery.Where(pi => pi.HasListings);
            //}



            log.Debug("Get ParentItems end");

            var parentASINList = parentItems.Select(pi => pi.ASIN).ToList();
            var childItemsResult = GetChildItemInfoList(parentASINList, (MarketType)filters.Market, filters.MarketplaceId);

            var childItems = childItemsResult.Select(i => new ItemShortInfoDTO()
            {
                Id = i.Id,

                AmazonRealQuantity = i.AmazonRealQuantity,
                RealQuantity = i.RealQuantity,
                DisplayQuantity = i.DisplayQuantity,

                ASIN = i.ASIN, //NOTE: for search filters
                ParentASIN = i.ParentASIN,
                SKU = i.SKU,
                Barcode = i.Barcode,

                SourceMarketId = i.SourceMarketId,

                StyleItemId = i.StyleItemId,
                StyleId = i.StyleId,

                StyleString = i.StyleString,
                StyleOnHold = i.StyleOnHold,
                StyleItemOnHold = i.StyleItemOnHold,
                StyleColor = i.StyleColor,
                StyleSize = i.StyleSize,

                IsFBA = i.IsFBA,
                IsDefault = i.IsDefault,
                OnHold = i.OnHold,

                PublishedStatus = i.PublishedStatus,

                ListingDefects = i.ListingDefects,
            }).ToList();


            log.Debug("Get ChildItems end");

            var styleIdList = childItems.Where(ci => ci.StyleId.HasValue)
                .Select(ci => ci.StyleId.Value)
                .Distinct().ToList();

            var styleItemResult = GetStyleItemInfoList(styleIdList, true);

            var forceStyleImage = filters.Market == (int)MarketType.Walmart
                || filters.Market == (int)MarketType.WalmartCA
                || filters.Market == (int)MarketType.Shopify
                || filters.Market == (int)MarketType.Magento
                || filters.Market == (int)MarketType.OverStock
                || filters.Market == (int)MarketType.Jet
                || filters.Market == (int)MarketType.eBay
                || filters.Market == (int)MarketType.Groupon
                || filters.Market == (int)MarketType.DropShipper
                || filters.Market == (int)MarketType.OfflineOrders
                || filters.Market == (int)MarketType.WooCommerce;
            var styleItems = styleItemResult.Where(si => si.StyleId.HasValue && styleIdList.Contains(si.StyleId.Value))
                .Select(si => new ItemShortInfoDTO()
                {
                    Id = si.Id,

                    StyleId = si.StyleId,
                    StyleString = si.StyleString,

                    StyleOnHold = si.StyleOnHold,
                    StyleItemOnHold = si.OnHold,

                    StyleSize = si.StyleSize,
                    StyleColor = si.StyleColor,
                    StyleItemId = si.StyleItemId,

                    RemainingQuantity = si.RemainingQuantity,

                    Image = forceStyleImage ? si.Image : null,

                    LinkedListingCount = si.MarketplacesInfo != null ?
                        (si.MarketplacesInfo.Contains(filters.MarketCode) ? 1 : 0) : 0,
                }).ToList();

            var publishRequestInfoes = unitOfWork.GetSet<SystemAction>().Where(i => i.Type == (int)SystemActionType.UpdateOnMarketProductData
                    && i.Status == (int)SystemActionStatus.None)
                .Select(i => new
                {
                    ItemId = i.Tag,
                    CreateDate = i.CreateDate,
                })
                .ToList();

            log.Debug("Get StyleItems end");


            foreach (var parentItem in parentItems)
            {
                var item = parentItem;

                item.ChildItems = childItems
                    .Where(ch => ch.ParentASIN == item.ASIN)
                    .ToList();

                var childItemIdStrs = item.ChildItems.Select(i => i.Id.ToString()).ToList();
                var publishRequestInfo = publishRequestInfoes.FirstOrDefault(i => childItemIdStrs.Contains(i.ItemId));

                item.PublishRequestedDate = publishRequestInfo?.CreateDate;

                var itemStyleIdList = item.ChildItems.Select(ch => ch.StyleId).ToList();

                item.StyleItems = styleItems
                    .Where(si => itemStyleIdList.Contains(si.StyleId))
                    .ToList();

                if (item.StyleItems.Any() && (forceStyleImage || useStyleImage))
                {
                    var defaultItem = item.ChildItems.FirstOrDefault(ch => ch.IsDefault);
                    ItemShortInfoDTO mainStyleItem = null;
                    if (defaultItem != null)
                        mainStyleItem = item.StyleItems.FirstOrDefault(si => si.Id == defaultItem.StyleItemId);
                    if (mainStyleItem == null)
                        mainStyleItem = item.StyleItems[0];
                    item.ImageSource = mainStyleItem.Image;
                }

                foreach (var childItem in item.ChildItems)
                {
                    var styleItem = item.StyleItems.FirstOrDefault(si => si.Id == childItem.StyleItemId);
                    childItem.RemainingQuantity = styleItem != null ? styleItem.RemainingQuantity : 0;
                }
            }

            log.Debug("Get Positions end");

            return new GridResponse<ParentItemDTO>(parentItems, totalCount);
        }
               

        public ParentItem GetByASIN(string parentASIN, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            return GetFiltered(p => p.ASIN == parentASIN
                && p.Market == (int)market
                && (String.IsNullOrEmpty(marketplaceId) || p.MarketplaceId == marketplaceId)).FirstOrDefault();
        }

        public IList<ParentItemDTO> GetSimilarByChildSkuAsDto(int id)
        {
            var parentItem = GetAsDTO(id);
            var childItems = unitOfWork.Items.GetByParentASINAsDto((MarketType)parentItem.Market, 
                parentItem.MarketplaceId, 
                parentItem.ASIN);

            //Set child
            parentItem.ChildItems = childItems.Select(ch => new ItemShortInfoDTO()
            {
                CurrentPrice = ch.CurrentPrice,
                RealQuantity = ch.RealQuantity,
                IsFBA = ch.IsFBA
            }).ToList();

            var skuList = childItems.Select(ch => ch.SKU).ToList();


            var getSimilarChilds = unitOfWork.Items.GetBySKUAsDto(skuList);
            var possibleSimilarParents = getSimilarChilds.Where(ch => !String.IsNullOrEmpty(ch.ParentASIN)).Select(ch => new ParentItemDTO()
            {
                ASIN = ch.ParentASIN,
                Market = ch.Market,
                MarketplaceId = ch.MarketplaceId
            }).ToList();

            var similarParents = new List<ParentItemDTO>() { parentItem };
            foreach (var possibleSimilarParent in possibleSimilarParents)
            {
                if (similarParents.All(p => !(p.ASIN == possibleSimilarParent.ASIN
                                            && p.Market == possibleSimilarParent.Market
                                            && p.MarketplaceId == possibleSimilarParent.MarketplaceId)))
                {
                    var similarParentItem = GetAsDTO(possibleSimilarParent.ASIN,
                        (MarketType) possibleSimilarParent.Market,
                        possibleSimilarParent.MarketplaceId);
                    if (similarParentItem != null)
                    {
                        similarParents.Add(similarParentItem);
                        //Set child
                        childItems = unitOfWork.Items.GetByParentASINAsDto((MarketType)similarParentItem.Market,
                            similarParentItem.MarketplaceId,
                            similarParentItem.ASIN);
                        similarParentItem.ChildItems = childItems.Select(ch => new ItemShortInfoDTO()
                        {
                            CurrentPrice = ch.CurrentPrice,
                            RealQuantity = ch.RealQuantity,
                            IsFBA = ch.IsFBA
                        }).ToList();
                    }
                }
            }

            return similarParents;
        }
    }
}
