using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemShortInfoViewModel
    {
        public long Id { get; set; }

        public string ASIN { get; set; }
        public string SourceMarketId { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string SKU { get; set; }
        public string Barcode { get; set; }
        

        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }

        public string StyleColor { get; set; }
        public string StyleSize { get; set; }
        
        public bool IsFBA { get; set; }
        public bool OnHold { get; set; }
        public bool StyleOnHold { get; set; }
        public bool StyleItemOnHold { get; set; }

        public decimal? CurrentPrice { get; set; }

        public int RealQuantity { get; set; }
        public int? DisplayQuantity { get; set; }
        public int? AmazonRealQuantity { get; set; }

        
        public int LinkedListingCount { get; set; }


        public int? RemainingQuantity { get; set; }

        public int PublishedStatus { get; set; }

        public IList<ListingDefectViewModel> ListingDefects { get; set; }

        public bool HasDefects
        {
            get { return ListingDefects != null && ListingDefects.Any(); }
        }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public string MarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, (MarketType)Market, MarketplaceId);
            }
        }

        public ItemShortInfoViewModel()
        {
            
        }

        public ItemShortInfoViewModel(int market, string marketplaceId, ItemShortInfoDTO itemInfo)
        {
            Id = itemInfo.Id;

            ASIN = itemInfo.ASIN;
            SKU = itemInfo.SKU;
            Barcode = itemInfo.Barcode;

            SourceMarketId = itemInfo.SourceMarketId;
            Market = market;
            MarketplaceId = marketplaceId;

            StyleId = itemInfo.StyleId;
            StyleString = itemInfo.StyleString;
            StyleItemId = itemInfo.StyleItemId;
            StyleOnHold = itemInfo.StyleOnHold;

            StyleSize = itemInfo.StyleSize;
            StyleColor = itemInfo.StyleColor;
            StyleItemOnHold = itemInfo.StyleItemOnHold;

            IsFBA = itemInfo.IsFBA;
            OnHold = itemInfo.OnHold;
            
            RealQuantity = itemInfo.RealQuantity;
            DisplayQuantity = itemInfo.DisplayQuantity;
            AmazonRealQuantity = itemInfo.AmazonRealQuantity;
            
            RemainingQuantity = itemInfo.RemainingQuantity < 0 ? 0 : itemInfo.RemainingQuantity;

            LinkedListingCount = itemInfo.LinkedListingCount;

            PublishedStatus = itemInfo.PublishedStatus;
            
            if (itemInfo.ListingDefects != null)
                ListingDefects = itemInfo.ListingDefects
                    .Select(d => new ListingDefectViewModel(d))
                    .ToList();
        }
    }
}