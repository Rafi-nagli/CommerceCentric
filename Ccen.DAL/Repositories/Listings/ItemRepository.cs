using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Listings;
using Amazon.DTO;
using Amazon.DTO.Graphs;
using Amazon.DTO.Listings;
using System.Linq.Expressions;
using Amazon.Core.Contracts;

namespace Amazon.DAL.Repositories
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        public ItemRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public void SetItemPublishingStatus(int itemId, 
            int newStatus,
            string reason,
            DateTime? when)
        {           
            var item = new Item()
            {
                Id = itemId,
                ItemPublishedStatus = newStatus,                
                ItemPublishedStatusReason = reason,
                ItemPublishedStatusDate = when,
            };

            TrackItem(item, new List<Expression<Func<Item, object>>>()
            {
                l => l.ItemPublishedStatus,
                l => l.ItemPublishedStatusReason,
                l => l.ItemPublishedStatusDate,
            });
        }

        public IQueryable<ViewItem> GetAllViewActual()
        {
            return unitOfWork.GetSet<ViewItem>();
        }

        public IQueryable<ItemExDTO> GetAllActualExAsDto()
        {
           var query = from vi in unitOfWork.GetSet<ViewItem>()
                       join i in unitOfWork.GetSet<Item>() on vi.Id equals i.Id
                       select new ItemExDTO()
                       {
                           Id = vi.Id,
                           Market = vi.Market,
                           MarketplaceId = vi.MarketplaceId,

                           ASIN = vi.ASIN,
                           ParentASIN = vi.ParentASIN,
                           Barcode = vi.Barcode,
                           Size = vi.Size,
                           Color = i.Color,
                           ColorVariation = i.ColorVariation,

                           Name = vi.Title, //=Title
                           SKU = vi.SKU,
                           IsDefault = vi.IsDefault,
                           IsFBA = vi.IsFBA,
                           IsPrime = vi.IsPrime,

                           CreateDate = vi.CreateDate,
                           UpdateDate = vi.UpdateDate,

                           CompanyId = vi.CompanyId,

                           StyleId = vi.StyleId,
                           StyleString = vi.StyleString,
                           StyleItemId = vi.StyleItemId,

                           StyleSize = vi.StyleSize,
                           StyleColor = vi.StyleColor,

                           Weight = vi.Weight,

                           CurrentPrice = vi.CurrentPrice,

                           EstimatedOrderHandlingFeePerOrder = vi.EstimatedOrderHandlingFeePerOrder,
                           EstimatedPickPackFeePerUnit = vi.EstimatedPickPackFeePerUnit,
                           EstimatedWeightHandlingFeePerUnit = vi.EstimatedWeightHandlingFeePerUnit,

                           Description = i.Description,

                           BrandName = i.BrandName,
                           Type = i.Type,
                           ListPrice = i.ListPrice,
                           
                           Department = i.Department,
                           Features = i.Features,
                           AdditionalImages = i.AdditionalImages,
                           SearchKeywords = i.SearchKeywords,
                           
                           ListingEntityId = vi.ListingEntityId,

                           RealQuantity = vi.RealQuantity,
                           DisplayQuantity = vi.DisplayQuantity,
                           RestockDate = vi.RestockDate,

                           SalePrice = vi.SalePrice,
                           SaleStartDate = vi.SaleStartDate,
                           SaleEndDate = vi.SaleEndDate,
                           MaxPiecesOnSale = vi.MaxPiecesOnSale,
                           
                           ImageUrl = vi.ItemPicture,
                           LargeImageUrl = vi.LargePicture,

                           LowestPrice = vi.LowestPrice,

                           SpecialSize = i.SpecialSize,
                           IsCheckedSpecialSize = i.IsCheckedSpecialSize,

                           SourceMarketId = i.SourceMarketId,
                           SourceMarketUrl = i.SourceMarketUrl,

                           PublishedStatus = i.ItemPublishedStatus,
                           PuclishedStatusDate = i.ItemPublishedStatusDate,
                           IsExistOnAmazon = i.IsExistOnAmazon,
                           OnMarketTemplateName = i.OnMarketTemplateName,
                       };
            return query;
        }
        
        public IQueryable<ItemDTO> GetAllActualWithSold()
        {
            var query = from vi in unitOfWork.GetSet<ViewItem>()
                        join i in unitOfWork.GetSet<Item>() on vi.Id equals i.Id

                        join stCacheSold in unitOfWork.GetSet<StyleItemCache>() on
                            i.StyleItemId equals stCacheSold.Id into withStCacheSold
                        from stCacheSold in withStCacheSold.DefaultIfEmpty()

                        join bBox in unitOfWork.GetSet<BuyBoxStatus>() on new { vi.ASIN, vi.Market, vi.MarketplaceId }
                                    equals new { bBox.ASIN, bBox.Market, bBox.MarketplaceId } into withBBox
                        from bBox in withBBox.DefaultIfEmpty()

                select new ItemDTO
                {
                    Id = i.Id,
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,

                    ParentASIN = i.ParentASIN,

                    PublishedStatus = i.ItemPublishedStatus,
                    PublishedStatusReason = i.ItemPublishedStatusReason,
                    SourceMarketId = i.SourceMarketId,
                    PublishedStatusFromMarket = i.ItemPublishedStatusFromMarket,

                    StyleString = i.StyleString,
                    StyleId = i.StyleId,
                    StyleOnHold = vi.StyleOnHold,
                    StyleItemOnHold = vi.StyleItemOnHold,

                    SKU = vi.SKU,
                    IsFBA = vi.IsFBA,
                    ListingEntityId = vi.ListingEntityId,

                    ASIN = i.ASIN,
                    Size = i.Size,
                    ColorVariation = i.ColorVariation,

                    StyleItemId = vi.StyleItemId,
                    StyleSize = vi.StyleSize,
                    StyleColor = vi.StyleColor,

                    Color = i.Color,
                    RealQuantity = vi.RealQuantity,
                    DisplayQuantity = vi.DisplayQuantity,
                    OnHold = vi.OnHold,
                    RestockDate = vi.RestockDate,

                    AutoQuantityUpdateDate = vi.AutoQuantityUpdateDate,

                    BusinessPrice = vi.BusinessPrice,
                    CurrentPrice = vi.CurrentPrice,
                    EstimatedOrderHandlingFeePerOrder = vi.EstimatedOrderHandlingFeePerOrder,
                    EstimatedWeightHandlingFeePerUnit = vi.EstimatedWeightHandlingFeePerUnit,
                    EstimatedPickPackFeePerUnit = vi.EstimatedPickPackFeePerUnit,

                    SalePrice = vi.SalePrice,
                    SaleStartDate = vi.SaleStartDate,
                    SaleEndDate = vi.SaleEndDate,
                    MaxPiecesOnSale = vi.MaxPiecesOnSale,
                    PiecesSoldOnSale = vi.PiecesSoldOnSale,

                    SaleId = vi.SaleId,


                    Weight = vi.Weight,
                    Barcode = vi.Barcode,

                    ImageUrl = vi.ItemPicture,
                    LargeImageUrl = vi.LargePicture,

                    UseStyleImage = i.UseStyleImage,

                    LowestPrice = bBox.WinnerPrice,
                    BuyBoxStatus = bBox.Status,

                    SoldByAmazon = stCacheSold.MarketsSoldQuantityFromDate,
                    SoldByInventory = stCacheSold.ScannedSoldQuantityFromDate,
                    SoldByFBA = stCacheSold.SentToFBAQuantityFromDate,
                    SoldBySpecialCase = stCacheSold.SpecialCaseQuantityFromDate,

                    TotalSoldByAmazon = stCacheSold.TotalMarketsSoldQuantity,
                    TotalSoldByInventory = stCacheSold.TotalScannedSoldQuantity,
                    TotalSoldByFBA = stCacheSold.TotalSentToFBAQuantity,
                    TotalSoldBySpecialCase = stCacheSold.TotalSpecialCaseQuantity,

                    TotalQuantity = stCacheSold.InventoryQuantity,
                    RemainingQuantity = stCacheSold.RemainingQuantity,

                    IsAmazonParentASIN = i.IsAmazonParentASIN,
                    OpenDate = vi.OpenDate,
                    CreateDate = i.CreateDate,
                    AmazonCurrentPrice = vi.AmazonCurrentPrice,
                    AmazonRealQuantity = vi.AmazonRealQuantity,
                };

            return query;
        }

        public IQueryable<ItemExDTO> GetAllActualForPrices()
        {
            var query = from i in unitOfWork.GetSet<ViewItem>()

                        join stCacheSold in unitOfWork.GetSet<StyleItemCache>() on
                            i.StyleItemId equals stCacheSold.Id into withStCacheSold
                        from stCacheSold in withStCacheSold.DefaultIfEmpty()

                        join bBox in unitOfWork.GetSet<BuyBoxStatus>() on i.ASIN equals bBox.ASIN into withBBox
                        from bBox in withBBox.DefaultIfEmpty()

                        select new ItemExDTO
                        {
                            Id = i.Id,
                            Market = i.Market,
                            MarketplaceId = i.MarketplaceId,

                            ParentASIN = i.ParentASIN,

                            SourceMarketId = i.SourceMarketId,

                            StyleString = i.StyleString,
                            StyleId = i.StyleId,
                            StyleOnHold = i.StyleOnHold,
                            SKU = i.SKU,
                            IsFBA = i.IsFBA,
                            ListingEntityId = i.ListingEntityId,

                            ASIN = i.ASIN,
                            Size = i.Size,
                            ColorVariation = i.ColorVariation,

                            StyleItemId = i.StyleItemId,
                            StyleSize = i.StyleSize,
                            StyleColor = i.StyleColor,

                            Color = i.Color,
                            RealQuantity = i.RealQuantity,
                            DisplayQuantity = i.DisplayQuantity,
                            OnHold = i.OnHold,
                            RestockDate = i.RestockDate,

                            AutoQuantityUpdateDate = i.AutoQuantityUpdateDate,

                            CurrentPrice = i.CurrentPrice,
                            
                            SalePrice = i.SalePrice,
                            SaleStartDate = i.SaleStartDate,
                            SaleEndDate = i.SaleEndDate,
                            MaxPiecesOnSale = i.MaxPiecesOnSale,
                            PiecesSoldOnSale = i.PiecesSoldOnSale,

                            SaleId = i.SaleId,

                            Weight = i.Weight,
                            Barcode = i.Barcode,

                            ImageUrl = i.ItemPicture,

                            LowestPrice = bBox.WinnerSalePrice.HasValue ? bBox.WinnerSalePrice : bBox.WinnerPrice, // i.LowestPrice,
                            BuyBoxStatus = bBox.Status,

                            WinnerPrice = bBox.WinnerPrice,
                            WinnerPriceLastChangeDate = bBox.WinnerPriceLastChangeDate,
                            WinnerPriceLastChangeValue = bBox.WinnerSalePriceLastChangeValue,

                            WinnerSalePrice = bBox.WinnerSalePrice,
                            WinnerSalePriceLastChangeDate = bBox.WinnerSalePriceLastChangeDate,
                            WinnerSalePriceLastChangeValue = bBox.WinnerSalePriceLastChangeValue,
                            
                            TotalQuantity = stCacheSold.InventoryQuantity,
                            RemainingQuantity = stCacheSold.RemainingQuantity,

                            IsAmazonParentASIN = i.IsAmazonParentASIN,
                            OpenDate = i.OpenDate,
                            CreateDate = i.CreateDate,
                            AmazonCurrentPrice = i.AmazonCurrentPrice,
                            AmazonRealQuantity = i.AmazonRealQuantity,
                        };

            return query;
        }

        public List<ItemDTO> GetByParentASINAsDto(MarketType market, string marketplaceId, string parentAsin)
        {
            var listingsQuery = unitOfWork.Listings.GetAll();
            if (market != MarketType.None)
                listingsQuery = listingsQuery.Where(l => l.Market == (int)market);
            if (!String.IsNullOrEmpty(marketplaceId))
                listingsQuery = listingsQuery.Where(l => l.MarketplaceId == marketplaceId);

            var query = from i in unitOfWork.GetSet<ViewItem>()
                join l in listingsQuery on i.Id equals l.ItemId
                where i.ParentASIN == parentAsin
                      && l.IsRemoved == false
                      && l.IsFBA == false
                select new ItemDTO
                    {
                        ASIN = i.ASIN,
                        SKU = l.SKU,
                        Size = i.Size,
                        Color = i.Color,
                        StyleColor = i.StyleColor,
                        StyleSize = i.StyleSize,
                        Weight = i.Weight,
                        StyleItemId = i.StyleItemId,
                        StyleString = i.StyleString,
                        StyleId = i.StyleId,
                        RealQuantity = i.RealQuantity,
                        CurrentPrice = i.CurrentPrice,
                    };

            return query.ToList();
        }

        public List<Item> GetItemsByParentASIN(MarketType market, string marketplaceId, string parentAsin)
        {
            var listingQuery = unitOfWork.Listings.GetAll();
            if (market != MarketType.None)
                listingQuery = listingQuery.Where(l => l.Market == (int) market);
            if (!String.IsNullOrEmpty(marketplaceId))
                listingQuery = listingQuery.Where(l => l.MarketplaceId == marketplaceId);

            var query = from i in GetAll()
                        join l in listingQuery on i.Id equals l.ItemId
                        where i.ParentASIN == parentAsin
                              && l.IsRemoved == false
                        select i;
            
            return query.ToList();
        }

        public List<Listing> GetListingsByParentASIN(MarketType market, string marketplaceId, string parentAsin)
        {
            var query = from i in GetAll()
                join l in unitOfWork.Listings.GetAll() on i.Id equals l.ItemId
                where i.ParentASIN == parentAsin
                      && l.IsRemoved == false
                select l;
            if (market != MarketType.None)
                query = query.Where(l => l.Market == (int) market);
            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(l => l.MarketplaceId == marketplaceId);

            return query.ToList();
        }

        public Item StoreItemIfNotExist(IItemHistoryService itemHistory,
            string calledFrom,
            ItemDTO dto, 
            MarketType market, 
            string marketplaceId, 
            long companyId,
            DateTime? when)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var item = GetByASIN(dto.ASIN, market, marketplaceId);

            if (item == null)
            {
                item = new Item
                {
                    ASIN = dto.ASIN,
                    Market = (int)market,
                    MarketplaceId = marketplaceId,

                    SourceMarketId = dto.SourceMarketId,

                    Title = dto.Name,
                    ParentASIN = dto.ParentASIN ?? String.Empty,
                    Size = StringHelper.Substring(dto.Size, 50),
                    Barcode = dto.Barcode,

                    //SKU = dto.SKU,
                    StyleString = dto.StyleString,
                    StyleId = dto.StyleId,
                    StyleItemId = dto.StyleItemId,
                    
                    IsAmazonParentASIN = dto.IsAmazonParentASIN,
                    LastUpdateFromAmazon = dto.LastUpdateFromAmazon,
                    IsExistOnAmazon = dto.IsExistOnAmazon,

                    CreateDate = when,
                    CompanyId = companyId,
                    Description = dto.Description,

                    //Additional fields
                    PrimaryImage = dto.ImageUrl,
                    BrandName = dto.BrandName,
                    Type = StringHelper.Substring(dto.Type, 50),
                    ListPrice = dto.ListPrice,
                    Color = StringHelper.Substring(dto.Color, 50),
                    Department = dto.Department,
                    Features = dto.Features,
                    AdditionalImages = dto.AdditionalImages,
                    SearchKeywords = dto.SearchKeywords
                };
                Add(item);

                unitOfWork.Commit();

                if (itemHistory != null)
                {
                    itemHistory.AddRecord(item.Id,
                        ItemHistoryHelper.SizeKey,
                        null,
                        calledFrom,
                        item.Size,
                        null,
                        null);
                }
            }
            
            return item;
        }

        public IQueryable<ItemDTO> GetAllWithSizeMappingIssues()
        {
            var query = from i in unitOfWork.GetSet<ViewItem>()
                join m in unitOfWork.GetSet<SizeMapping>() on i.Size equals m.ItemSize into withSize
                from m in withSize.DefaultIfEmpty()
                where i.Size != i.StyleSize
                      && m.ItemSize == null
                      && (i.MarketplaceId != MarketplaceKeeper.AmazonMxMarketplaceId) //NOTE: Exclude MX
                select i;
            return AsDto(query);
        }
        
        public Item GetByASIN(string asin, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            return GetFiltered(i => 
                i.ASIN == asin
                && i.Market == (int)market
                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)).FirstOrDefault();
        }

        public Item GetBySKU(string asin, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            return GetFiltered(i =>
                i.ASIN == asin
                && i.Market == (int)market
                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)).FirstOrDefault();
        }

        public ItemDTO GetByIdAsDto(long id)
        {
            return GetAllViewAsDto().FirstOrDefault(i => i.Id == id);
        }

        public IList<ItemDTO> GetBySKUAsDto(string sku)
        {
            return GetAllViewAsDto().Where(i => i.SKU == sku).ToList();
        }

        public IList<ItemDTO> GetBySKUAsDto(IList<string> skuList)
        {
            return GetAllViewAsDto().Where(i => skuList.Contains(i.SKU)).ToList();
        }
        
        public bool CheckForExistenceBarcode(string barcode, MarketType market, string marketplaceId)
        {
            var query = GetAllViewAsDto().Where(i => i.Market == (int)market
                && i.Barcode == barcode);

            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(l => l.MarketplaceId == marketplaceId);

            return query.FirstOrDefault() != null;
        }

        public IQueryable<ItemDTO> GetAllViewAsDto(MarketType market, string marketplaceId)
        {
            var query = GetAllViewAsDto();

            if (market != MarketType.None)
                query = query.Where(i => i.Market == (int)market);

            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(i => i.MarketplaceId == marketplaceId);

            return query;
        }

        public IQueryable<ItemDTO> GetAllViewAsDto()
        {
            return AsDto(unitOfWork.GetSet<ViewItem>());
        }

        public IQueryable<ItemDTO> AsDto(IQueryable<ViewItem> query)
        {
            return query.Select(i => new ItemDTO
            {
                Id = i.Id,
                ASIN = i.ASIN,
                SourceMarketId = i.SourceMarketId,
                ParentASIN = i.ParentASIN,
                ListingEntityId = i.ListingEntityId,
                ListingId = i.ListingId,

                Size = i.Size,
                Color = i.Color,
                ColorVariation = i.ColorVariation,

                StyleColor = i.StyleColor,
                StyleSize = i.StyleSize,
                StyleItemId = i.StyleItemId,
                StyleId = i.StyleId,
                StyleString = i.StyleString,
                StyleOnHold = i.StyleOnHold,
                UseStyleImage = i.UseStyleImage,

                Weight = i.Weight,
                SKU = i.SKU,
                IsDefault = i.IsDefault,
                IsFBA = i.IsFBA,
                IsPrime = i.IsPrime,
                Market = i.Market,
                MarketplaceId = i.MarketplaceId,
                Barcode = i.Barcode,

                ImageUrl = i.ItemPicture,
                LargeImageUrl = i.LargePicture,

                AmazonRealQuantity = i.AmazonRealQuantity,
                RealQuantity = i.RealQuantity,
                DisplayQuantity = i.DisplayQuantity,

                OnHold = i.OnHold,
                RestockDate = i.RestockDate,

                AmazonCurrentPrice = i.AmazonCurrentPrice,
                CurrentPrice = i.CurrentPrice,

                SalePrice = i.SalePrice,
                SaleStartDate = i.SaleStartDate,
                SaleEndDate = i.SaleEndDate,
                PiecesSoldOnSale = i.PiecesSoldOnSale,
                MaxPiecesOnSale = i.MaxPiecesOnSale,

                SaleId = i.SaleId,

                OpenDate = i.OpenDate,

                PublishedStatus = i.ItemPublishedStatus,

                IsAmazonParentASIN = i.IsAmazonParentASIN,
                IsExistOnAmazon = i.IsExistOnAmazon,

                CreateDate = i.CreateDate,
            });
        }

        public IQueryable<SoldSizeInfo> GetMarketsSoldQuantityByStyleItem()
        {
            var query = unitOfWork.GetSet<ViewMarketsSoldQuantityByStyleItem>().Select(v => new SoldSizeInfo
            {
                StyleItemId = v.StyleItemId,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                StyleId = v.StyleId,
                MinOrderDate = v.MinOrderDate,
                MaxOrderDate = v.MaxOrderDate,
                OrderCount = v.OrderCount,
            });
            return query;
        }

        public IQueryable<SoldSizeInfo> GetMarketsSoldQuantityIncludePreOrderByStyleItem()
        {
            var query = unitOfWork.GetSet<ViewMarketsSoldQuantityIncludePreOrderByStyleItem>().Select(v => new SoldSizeInfo
            {
                StyleItemId = v.StyleItemId,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                StyleId = v.StyleId,
                MinOrderDate = v.MinOrderDate,
                MaxOrderDate = v.MaxOrderDate,
                OrderCount = v.OrderCount,
            });
            return query;
        }

        public IQueryable<SoldSizeInfo> GetMarketsSoldThisYearQuantityIncludePreOrderByStyleItem()
        {
            var query = unitOfWork.GetSet<ViewMarketsSoldThisYearQuantityIncludePreOrderByStyleItem>().Select(v => new SoldSizeInfo
            {
                StyleItemId = v.StyleItemId,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                StyleId = v.StyleId,
                MinOrderDate = v.MinOrderDate,
                MaxOrderDate = v.MaxOrderDate,
                OrderCount = v.OrderCount,
            });
            return query;
        }

        public IQueryable<SoldSizeInfo> GetMarketsSoldQuantityByListing()
        {
            var query = unitOfWork.GetSet<ViewMarketsSoldQuantity>().Select(v => new SoldSizeInfo
            {
                ListingId = v.ListingId,
                StyleItemId = v.StyleItemId,
                Market = v.Market,
                MarketplaceId = v.MarketplaceId,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                Size = v.Size,
                StyleId = v.StyleId,
                MinOrderDate = v.MinOrderDate,
                MaxOrderDate = v.MaxOrderDate,
                OrderCount = v.OrderCount,
            });
            return query;
        }
        
        public IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndMarket()
        {
            return unitOfWork.GetSet<ViewMarketsSoldByDateAndMarket>().Select(i => new PurchaseByDateDTO()
            {
                Date = i.Date,
                Market = i.Market,
                MarketplaceId = i.MarketplaceId,
                Price = i.Price ?? 0,
                Quantity = i.Quantity ?? 0,
            });
        }

        public IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndItemStyle()
        {
            return unitOfWork.GetSet<ViewMarketsSoldByDateAndItemStyle>().Select(i => new PurchaseByDateDTO()
            {
                Date = i.Date,
                ItemStyle = i.ItemStyle,
                Price = i.Price ?? 0,
                Quantity = i.Quantity ?? 0,
            });
        }

        public IQueryable<PurchaseByDateDTO> GetSalesInfoBySKU()
        {
            var query = unitOfWork.GetSet<ViewMarketsSoldByDateAndSKU>().Select(i => new PurchaseByDateDTO()
            {
                Date = i.Date,
                Market = i.Market,
                MarketplaceId = i.MarketplaceId,
                SKU = i.SKU,
                StyleItemId = i.StyleItemId,
                Price = i.Price ?? 0,
                Quantity = i.Quantity ?? 0,
            });

            return query;
        }
        

        public void UpdateItemsForParentItem(
            IItemHistoryService itemChangeHistory,
            string calledFrom,
            string parentAsin,
            int market,
            string marketplaceId,
            IList<ItemDTO> items,
            DateTime when,
            long? by)
        {
            var dbExistItems = GetFiltered(l => l.ParentASIN == parentAsin
                && l.Market == market
                && l.MarketplaceId == marketplaceId).ToList();
            
            var existItemIdList = dbExistItems.Select(i => i.Id).ToList();
            var dbExistListings = unitOfWork.GetSet<Listing>()
                .Where(l => existItemIdList.Contains(l.ItemId))
                .ToList();

            var existListingIdList = dbExistListings.Select(l => l.Id).ToList();

            var newItems = items.Where(l => !l.ListingEntityId.HasValue || l.ListingEntityId == 0 || !existListingIdList.Contains(l.ListingEntityId.Value)).ToList();
            
            foreach (var dbListing in dbExistListings)
            {
                var existItem = items.FirstOrDefault(l => l.ListingEntityId == dbListing.Id);
                var dbItem = dbExistItems.FirstOrDefault(l => l.Id == dbListing.ItemId);

                if (existItem != null)
                {
                    var hasChanges = dbItem.StyleItemId != existItem.StyleItemId
                                     || dbItem.StyleId != existItem.StyleId
                                     || dbItem.Size != existItem.Size
                                     || dbItem.Color != existItem.Color
                                     || dbItem.Title != existItem.Name
                                     || dbItem.Barcode != existItem.Barcode
                                     || dbItem.PrimaryImage != existItem.ImageUrl
                                     || dbItem.ParentASIN != existItem.ParentASIN;

                    if (hasChanges)
                    {
                        dbItem.ParentASIN = existItem.ParentASIN;

                        dbItem.StyleString = existItem.StyleString;
                        dbItem.StyleId = existItem.StyleId;
                        dbItem.StyleItemId = existItem.StyleItemId;

                        if (dbItem.Size != existItem.Size)
                        {
                            if (itemChangeHistory != null)
                                itemChangeHistory.AddRecord(dbItem.Id,
                                    ItemHistoryHelper.SizeKey,
                                    dbItem.Size,
                                    calledFrom,
                                    existItem.Size,
                                    null,
                                    by);
                        }
                        
                        dbItem.Size = existItem.Size;
                        dbItem.Color = existItem.Color;
                        dbItem.Title = existItem.Name;

                        if (!String.IsNullOrEmpty(existItem.Barcode)) //NOTE: may came empty when disable mode
                        {
                            if (dbItem.Barcode != existItem.Barcode)
                            {
                                if (itemChangeHistory != null)
                                    itemChangeHistory.AddRecord(dbItem.Id,
                                        ItemHistoryHelper.BarcodeKey,
                                        dbItem.Barcode,
                                        calledFrom,
                                        existItem.Barcode,
                                        null,
                                        by);

                                dbItem.Barcode = existItem.Barcode;
                            }
                            
                        }

                        dbItem.PrimaryImage = existItem.ImageUrl;

                        dbItem.UpdateDate = when;
                        dbItem.UpdatedBy = by;
                    }

                    if (dbItem.ItemPublishedStatus != (int) PublishedStatuses.None
                        && dbItem.ItemPublishedStatus != (int) PublishedStatuses.New
                        && dbItem.ItemPublishedStatus != (int) PublishedStatuses.Unpublished
                        && dbItem.ItemPublishedStatus != (int) PublishedStatuses.HasUnpublishRequest)
                    {
                        dbItem.ItemPublishedStatus = (int) PublishedStatuses.HasChanges;
                        dbItem.ItemPublishedStatusDate = when;
                    }
                    //NOTE: override with manuall if not empty
                    if (existItem.PublishedStatus != (int)PublishedStatuses.None)
                    {
                        dbItem.ItemPublishedStatus = existItem.PublishedStatus;
                        dbItem.ItemPublishedStatusDate = when;
                    }
                                        
                    if (dbListing != null)
                    {
                        var hasListingChanges = dbListing.CurrentPrice != existItem.CurrentPrice
                            || dbListing.IsPrime != existItem.IsPrime
                            || dbListing.SKU != existItem.SKU
                            || dbListing.IsDefault != existItem.IsDefault;
                        if (hasListingChanges)
                        {
                            if (dbListing.CurrentPrice != existItem.CurrentPrice)
                            {
                                itemChangeHistory.LogListingPrice(PriceChangeSourceType.EnterNewPrice,
                                    dbListing.Id,
                                    dbListing.SKU,
                                    existItem.CurrentPrice,
                                    dbListing.CurrentPrice,
                                    when,
                                    by);

                                dbListing.CurrentPrice = existItem.CurrentPrice;
                            }
                            
                            dbListing.IsPrime = existItem.IsPrime;

                            if (!String.IsNullOrEmpty(existItem.SKU)) //NOTE: may came empty when disable mode
                            {
                                if (dbListing.SKU != existItem.SKU)
                                {
                                    if (itemChangeHistory != null)
                                        itemChangeHistory.AddRecord(dbItem.Id,
                                            ItemHistoryHelper.SKUKey,
                                            dbListing.SKU,
                                            calledFrom,
                                            existItem.SKU,
                                            null,
                                            by);

                                    dbListing.SKU = existItem.SKU;
                                }                                
                            }
                            dbListing.IsDefault = existItem.IsDefault;

                            dbListing.UpdateDate = when;
                            dbListing.UpdatedBy = by;
                        }

                        dbListing.QuantityUpdateRequested = true;
                        dbListing.QuantityUpdateRequestedDate = when;
                        dbListing.PriceUpdateRequested = true;
                        dbListing.PriceUpdateRequestedDate = when;
                    }
                }
                else
                {
                    dbListing.IsRemoved = true;
                    unitOfWork.Commit();
                }
            }

            unitOfWork.Commit();

            foreach (var newItem in newItems)
            {
                var dbItem = new Item()
                {
                    ParentASIN = newItem.ParentASIN,
                    Market = market,
                    MarketplaceId = marketplaceId,

                    ASIN = StringHelper.Substring(newItem.ASIN, 50),

                    StyleString = newItem.StyleString,
                    StyleId = newItem.StyleId,
                    StyleItemId = newItem.StyleItemId,
                    Size = StringHelper.Substring(newItem.Size, 50),
                    Color = StringHelper.Substring(newItem.Color, 50),

                    Barcode = newItem.Barcode,
                    PrimaryImage = newItem.ImageUrl,

                    Title = newItem.Name,
                    
                    ItemPublishedStatus = (int)PublishedStatuses.New,
                    ItemPublishedStatusDate = when,

                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbItem);
                unitOfWork.Commit();

                if (itemChangeHistory != null)
                    itemChangeHistory.AddRecord(dbItem.Id,
                        ItemHistoryHelper.SizeKey,
                        null,
                        calledFrom,
                        dbItem.Size,
                        null,
                        by);

                var dbListing = new Listing()
                {
                    ItemId = dbItem.Id,
                    Market = market,
                    MarketplaceId = marketplaceId,

                    ListingId = newItem.ListingId,
                    SKU = StringHelper.Substring(newItem.SKU, 100),
                    IsDefault = newItem.IsDefault,

                    CurrentPrice = newItem.CurrentPrice,
                    RealQuantity = newItem.RealQuantity,

                    PriceUpdateRequested = true,
                    QuantityUpdateRequested = true,

                    CreateDate = when,
                    CreatedBy = by
                };

                unitOfWork.Listings.Add(dbListing);
                unitOfWork.Commit();

                newItem.ListingEntityId = dbListing.Id;
                newItem.Id = dbItem.Id;
            }
        }
        
        public void DeleteAnyLinksToStyleId(long styleId)
        {
            var itemQuery = from i in unitOfWork.GetSet<Item>()
                            where i.StyleId == styleId
                            select i;

            var items = itemQuery.ToList();
            foreach (var item in items)
            {
                item.StyleString = null;
                item.StyleId = null;
                item.StyleItemId = null;
            }
        }

        public IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndBrand()
        {
            throw new NotImplementedException();
        }
    }
}
