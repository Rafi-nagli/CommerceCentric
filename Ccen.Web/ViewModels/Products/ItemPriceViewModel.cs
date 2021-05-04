using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Utils;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchResults;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemPriceViewModel
    {
        public int ItemId { get; set; }

        public int? Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? ListingEntityId { get; set; }
        public string ParentASIN { get; set; }
        public string SKU { get; set; }

        public bool IsFBA { get; set; }
        public decimal? EstimatedOrderHandlingFeePerOrder { get; set; }
        public decimal? EstimatedPickPackFeePerUnit { get; set; }
        public decimal? EstimatedWeightHandlingFeePerUnit { get; set; }

        public decimal? FBAFee
        {
            get { return EstimatedOrderHandlingFeePerOrder + EstimatedPickPackFeePerUnit + EstimatedWeightHandlingFeePerUnit; }
        }

        public string PictureUrl { get; set; }
        public string LargePictureUrl { get; set; }

        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }
        
        public DateTime? AutoQuantityUpdateDate { get; set; }
        public string AutoQuantityUpdateDateAgoDateFormatted
        {
            get
            {
                return DateHelper.ConvertToReadableStringAgo(AutoQuantityUpdateDate, true);
            }
        }
        public bool OnHold { get; set; }
        public bool StyleOnHold { get; set; }
        public DateTime? RestockDate { get; set; }

        
        public int? TotalQuantity { get; set; }

        public int? RemainingQuantity { get; set; }

        public string Size { get; set; }
        public string ColorVariation { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }


        public string Color { get; set; }
        public string ASIN { get; set; }
        public string Title { get; set; }


        public string Currency
        {
            get
            {
                return PriceHelper.GetCurrencySymbol((MarketType)(Market ?? (int)MarketType.Amazon), MarketplaceId);
            }
        }

        public decimal ItemCurrentPrice { get; set; }
        public string ItemCurrentPriceCurrency { get; set; }
        

        //Sale
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }
        public long? SaleId { get; set; }


        public decimal? WinnerPrice { get; set; }
        public DateTime? WinnerPriceLastChangeDate { get; set; }
        public decimal? WinnerPriceLastChangeValue { get; set; }

        public decimal? WinnerSalePrice { get; set; }
        public DateTime? WinnerSalePriceLastChangeDate { get; set; }
        public decimal? WinnerSalePriceLastChangeValue { get; set; }
        public BuyBoxStatusCode BuyBoxStatus { get; set; }


        public string ShippingPrice { get; set; }
        public string Weight { get; set; }
        [RegularExpression(@"^([0-9]*)$", ErrorMessage = "Invalid count")]
        public string PiecesInWarehouse { get; set; }

        [Display(Name = "UPC")]
        public string Barcode { get; set; }

        
        public bool? IsAmazonParentASIN { get; set; }
        public DateTime? OpenDate { get; set; }
        public int? AmazonRealQuantity { get; set; }
        public decimal? AmazonCurrentPrice { get; set; }

        public IList<ItemMarketInfoViewModel> RelatedMarkets { get; set; }


        public float MarketIndex
        {
            get { return MarketHelper.GetMarketIndex((MarketType)(Market ?? (int)MarketType.None), MarketplaceId); }
        }

        public string MarketShortName
        {
            get { return MarketHelper.GetShortName(Market ?? (int)MarketType.None, MarketplaceId); }
        }

        public bool HasImage
        {
            get { return !String.IsNullOrEmpty(PictureUrl); }
        }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(PictureUrl, 
                    75, 
                    75, 
                    false, 
                    ImageHelper.NO_IMAGE_URL,
                    convertInDomainUrlToThumbnail: true);
            }
        }

        public string LargeThumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(LargePictureUrl, 
                    75, 
                    75, 
                    false, 
                    ImageHelper.NO_IMAGE_URL,
                    convertInDomainUrlToThumbnail: true);
            }
        }


        public string StyleUrl
        {
            get
            {
                return UrlHelper.GetStyleUrl(StyleString);
            }
        }

        public float SizeIndex
        {
            get
            {
                return SizeHelper.GetSizeIndex(Size);
            }
        }

        public string SourceMarketId { get; set; }
        public string MarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, (MarketType)Market, MarketplaceId);
            }
        }

        public string ParentMarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ParentASIN, SourceMarketId,(MarketType)Market, MarketplaceId);
            }
        }

        public string SellerMarketUrl
        {
            get
            {
                if (Market == (int)MarketType.Magento)
                    return UrlHelper.GetSellarCentralInventoryUrl(SourceMarketId, (MarketType)Market, MarketplaceId);
                return UrlHelper.GetSellarCentralInventoryUrl(ASIN, (MarketType)Market, MarketplaceId);
            }
        }
        
        public override string ToString()
        {
            return "ItemId=" + ItemId 
                + ", ListingId=" + ListingEntityId 
                + ", ParentASIN=" + ParentASIN 
                + ", Barcode=" + Barcode 
                + ", DisplayQuantity=" + DisplayQuantity 
                + ", RealQuantity=" + RealQuantity 
                + ", TotalQuantity=" + TotalQuantity

                + ", OnHold=" + OnHold
                + ", RestockDate=" + RestockDate

                + ", SalePrice=" + SalePrice
                + ", SaleStartDate=" + SaleStartDate
                + ", SaleEndDate=" + SaleEndDate
                + ", MaxPiecesOnSale=" + MaxPiecesOnSale
                
                + ", StyleId=" + StyleId
                + ", StyleItemId=" + StyleItemId;
        }

        public ItemPriceViewModel()
        {
        }

        public ItemPriceViewModel(ItemExDTO item)
        {
            ItemId = item.Id;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;

            ListingEntityId = item.ListingEntityId;
            ParentASIN = item.ParentASIN;
            SKU = item.SKU;

            SourceMarketId = item.SourceMarketId;

            PictureUrl = item.ImageUrl;
            LargePictureUrl = item.LargeImageUrl;

            IsFBA = item.IsFBA;
            EstimatedOrderHandlingFeePerOrder = item.EstimatedOrderHandlingFeePerOrder;
            EstimatedPickPackFeePerUnit = item.EstimatedPickPackFeePerUnit;
            EstimatedWeightHandlingFeePerUnit = item.EstimatedWeightHandlingFeePerUnit;

            StyleId = item.StyleId;
            StyleItemId = item.StyleItemId;
            StyleString = item.StyleString;
            StyleOnHold = item.StyleOnHold ?? false;
            ASIN = item.ASIN;

            Size = item.Size;
            ColorVariation = item.ColorVariation;

            StyleSize = item.StyleSize;
            StyleColor = item.StyleColor;

            Color = item.Color;
            DisplayQuantity = item.DisplayQuantity;
            RealQuantity = item.RealQuantity;

            AutoQuantityUpdateDate = item.AutoQuantityUpdateDate;
            OnHold = item.OnHold;
            RestockDate = item.RestockDate;

            ItemCurrentPrice = item.CurrentPrice;
            ItemCurrentPriceCurrency = PriceHelper.GetCurrencySymbol((MarketType)item.Market, item.MarketplaceId);

            //Sale
            SalePrice = item.SalePrice;
            SaleStartDate = item.SaleStartDate;
            SaleEndDate = item.SaleEndDate;
            MaxPiecesOnSale = item.MaxPiecesOnSale;
            PiecesSoldOnSale = item.PiecesSoldOnSale;
            SaleId = item.SaleId;

            Weight = item.Weight != null ? item.Weight.ToString() : "";
            Barcode = item.Barcode;

            WinnerPrice = item.WinnerPrice;
            WinnerPriceLastChangeDate = item.WinnerPriceLastChangeDate;
            WinnerPriceLastChangeValue = item.WinnerPriceLastChangeValue;

            WinnerSalePrice = item.WinnerSalePrice;
            WinnerSalePriceLastChangeDate = item.WinnerSalePriceLastChangeDate;
            WinnerSalePriceLastChangeValue = item.WinnerSalePriceLastChangeValue;

            BuyBoxStatus = item.BuyBoxStatus ?? BuyBoxStatusCode.None;

            TotalQuantity = item.TotalQuantity;

            RemainingQuantity = item.RemainingQuantity;

            IsAmazonParentASIN = item.IsAmazonParentASIN;
            OpenDate = item.OpenDate ?? item.CreateDate;
            AmazonRealQuantity = item.AmazonRealQuantity;
            AmazonCurrentPrice = item.AmazonCurrentPrice;
        }


        

        /// <summary>
        /// Get items for grid
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ItemPriceViewModel> GetAll(IUnitOfWork db, 
            ItemPriceSearchFilterViewModel filter)
        {
            var query = db.Items.GetAllActualForPrices();

            if (filter.BuyBoxWinMode == BuyBoxWinModes.NotWin)
                query = query.Where(i => i.BuyBoxStatus == BuyBoxStatusCode.NotWin);

            if (filter.Availability == ProductAvailability.InStock)
                query = query.Where(i => i.RealQuantity > 0);

            if (!String.IsNullOrEmpty(filter.StyleString))
                query = query.Where(i => i.StyleString == filter.StyleString);

            if (filter.Market != MarketType.None)
                query = query.Where(i => i.Market == (int)filter.Market);

            if (!String.IsNullOrEmpty(filter.MarketplaceId))
                query = query.Where(i => i.MarketplaceId == filter.MarketplaceId);

            switch (filter.ListingMode)
            {
                case ListingsModeType.OnlyFBA:
                    query = query.Where(l => l.IsFBA);
                    break;
                case ListingsModeType.WithoutFBA:
                    query = query.Where(l => !l.IsFBA);
                    break;
            }
            
            var items = query
                .ToList()
                .Select(i => new ItemPriceViewModel(i))
                .ToList();

            return items;
        }

        public static ProductSearchResult GetIdListByFilters(IUnitOfWork db,
            ItemPriceSearchFilterViewModel filter)
        {
            var result = new ProductSearchResult();

            if (filter.NoneSoldPeriod.HasValue && filter.NoneSoldPeriod > 0)
            {
                //NOTE: using styleId to hide parent item with that style but without sold
                //получаем список продающихся styleId
                //выводим список item у которых styleId not in Sold Style List
                var fromDate = DateTime.Today.AddDays(-filter.NoneSoldPeriod.Value);
                var soldStyleIds = db.StyleCaches.GetAll()
                    .Where(sc => sc.LastSoldDateOnMarket < fromDate)
                    .GroupBy(sc => sc.Id)
                    .Select(gsc => gsc.Key).ToList();

                result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, soldStyleIds);
            }

            if (filter.Gender.HasValue)
            {
                var genderStyleIds = db.StyleCaches
                    .GetAll()
                    .Where(s => s.Gender == filter.Gender.Value.ToString())
                    .Select(s => s.Id).ToList();

                result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, genderStyleIds);
            }

            if (filter.MainLicense.HasValue)
            {
                var mainLicenseStyleIds = db.StyleCaches
                    .GetAll()
                    .Where(s => s.MainLicense == filter.MainLicense.Value.ToString())
                    .Select(s => s.Id)
                    .ToList();

                result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, mainLicenseStyleIds);
            }

            if (filter.SubLicense.HasValue)
            {
                var subLicenseStyleIds = db.StyleCaches
                    .GetAll()
                    .Where(s => s.SubLicense == filter.SubLicense.Value.ToString())
                    .Select(s => s.Id)
                    .ToList();

                result.StyleIdList = GeneralUtils.IntersectIfNotNull(result.StyleIdList, subLicenseStyleIds);
            }

            if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
            {
                var priceQuery = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)filter.Market);
                if (!String.IsNullOrEmpty(filter.MarketplaceId))
                    priceQuery = priceQuery.Where(p => p.MarketplaceId == filter.MarketplaceId);

                if (filter.MinPrice.HasValue)
                    priceQuery = priceQuery.Where(p => p.CurrentPrice >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    priceQuery = priceQuery.Where(p => p.CurrentPrice <= filter.MaxPrice.Value);

                result.ChildItemIdList = priceQuery.Select(p => p.Id).ToList();
            }

            return result;
        }
    }
}