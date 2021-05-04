using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using Amazon.Core.Helpers;
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemInfoViewModel
    {
        public string ASIN { get; set; }

        public string Size { get; set; }
        public string ColorVariation { get; set; }

        public float SizeIndex { get; set; }
        public string FormattedSize { get; set; }

        public string Color { get; set; }

        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }

        public string StyleColor { get; set; }
        public string StyleSize { get; set; }


        public bool IsFBA { get; set; }

        public decimal? CurrentPrice { get; set; }
        public decimal? AmazonCurrentPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        public decimal? FBAFee { get; set; }

        public int RealQuantity { get; set; }
        public int? DisplayQuantity { get; set; }
        public int? AmazonRealQuantity { get; set; }

        public int? InventoryQuantity { get; set; }
        public int? MarketSoldQuantity { get; set; }
        public int? ScannedSoldQuantity { get; set; }
        public int? SpecialCaseSoldQuantity { get; set; }
        public int? TotalMarketSoldQuantity { get; set; }
        public int? TotalScannedSoldQuantity { get; set; }
        public int? TotalSpecialCaseSoldQuantity { get; set; }
        public int? ListingSoldQuantity { get; set; }

        public DateTime? LastOrder { get; set; }
        public int LinkedListingCount { get; set; }

        public DateTime? OpenDate { get; set; }

        public int? RemainingQuantity { get; set; }

        public IList<ListingDefectViewModel> ListingDefects { get; set; }

        public bool HasDefects
        {
            get { return ListingDefects != null && ListingDefects.Any(); }
        }

        //public bool IsNeedToBuy
        //{
        //    get
        //    {
        //        return RealQuantity <= 0
        //               && LastOrder.HasValue
        //               && LastOrder.Value > DateTime.UtcNow.AddDays(-7);
        //    }
        //}

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public ItemInfoViewModel()
        {
            
        }

        public ItemInfoViewModel(ItemInfoDTO itemInfo)
        {
            ASIN = itemInfo.ASIN;
            
            Size = itemInfo.Size;
            ColorVariation = itemInfo.ColorVariation;

            FormattedSize = SizeHelper.FormatSize(Size);
            Color = itemInfo.Color;

            StyleId = itemInfo.StyleId;
            StyleString = itemInfo.StyleString;
            StyleItemId = itemInfo.StyleItemId;

            StyleSize = itemInfo.StyleSize;
            StyleColor = itemInfo.StyleColor;


            IsFBA = itemInfo.IsFBA;

            CurrentPrice = itemInfo.CurrentPrice;
            AmazonCurrentPrice = itemInfo.AmazonCurrentPrice;
            FBAFee = itemInfo.FBAFee;

            SalePrice = itemInfo.SalePrice;
            SaleStartDate = itemInfo.SaleStartDate;
            SaleEndDate = itemInfo.SaleEndDate;

            RealQuantity = itemInfo.RealQuantity;
            DisplayQuantity = itemInfo.DisplayQuantity;
            AmazonRealQuantity = itemInfo.AmazonRealQuantity;

            InventoryQuantity = itemInfo.InventoryQuantity;
            ListingSoldQuantity = itemInfo.ListingSoldQuantity;
            MarketSoldQuantity = itemInfo.MarketSoldQuantity;
            ScannedSoldQuantity = itemInfo.ScannedSoldQuantity;
            SpecialCaseSoldQuantity = itemInfo.SpecialCaseQuantity;
            RemainingQuantity = itemInfo.RemainingQuantity < 0 ? 0 : itemInfo.RemainingQuantity;

            TotalMarketSoldQuantity = itemInfo.TotalMarketSoldQuantity;
            TotalScannedSoldQuantity = itemInfo.TotalScannedSoldQuantity;
            TotalSpecialCaseSoldQuantity = itemInfo.TotalSpecialCaseQuantity;

            LastOrder = itemInfo.LastOrder;
            LinkedListingCount = itemInfo.LinkedListingCount;
            OpenDate = itemInfo.OpenDate ?? itemInfo.CreateDate;

            SizeIndex = SizeHelper.GetSizeIndex(Size);

            if (itemInfo.ListingDefects != null)
                ListingDefects = itemInfo.ListingDefects
                    .Select(d => new ListingDefectViewModel(d))
                    .ToList();
        }
    }
}