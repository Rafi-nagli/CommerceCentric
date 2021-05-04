using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Api;
using Amazon.Api.AmazonECommerceService;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ListingPriceEditViewModel
    {
        public long ListingId { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }


        public bool IsChecked { get; set; }


        public bool IsFBA { get; set; }
        public bool IsPrime { get; set; }
        public double? Weight { get; set; }
        
        public string ListingType
        {
            get
            {
                return AmazonTemplateHelper.GetListingType(IsFBA, IsPrime, Market);
            }
        }

        public string SKU { get; set; }
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string SourceMarketId { get; set; }

        public string StyleSize { get; set; }
        public string StyleColor { get; set; }

        public string ListingSize { get; set; }
        public string ListingColor { get; set; }

        public bool OnHold { get; set; }

        public decimal Price { get; set; }
        public decimal? LowestPrice { get; set; }
        public DateTime? LowestPriceUpdateDate { get; set; }

        public decimal? OverrideSalePrice { get; set; }

        public string FormattedLowestPriceUpdateDate
        {
            get { return DateHelper.ConvertToReadableStringAgo(LowestPriceUpdateDate, true); }
        }

        public string Currency
        {
            get
            {
                return PriceHelper.GetCurrencySymbol(Market, MarketplaceId);
            }
        }


        public float SizeIndex
        {
            get
            {
                return SizeHelper.GetSizeIndex(StyleSize);
            }
        }

        public string MarketName
        {
            get
            {
                return MarketHelper.GetMarketName((int)Market, MarketplaceId);
            }
        }

        public string ProductUrl
        {
            get { return UrlHelper.GetProductUrl(ParentASIN, Market, MarketplaceId); }
        }

        public string MarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, (MarketType)Market, MarketplaceId);
            }
        }

        public string SellerMarketUrl
        {
            get
            {
                return UrlHelper.GetSellarCentralInventoryUrl(ASIN, Market, MarketplaceId);
            }
        }

        public override string ToString()
        {
            return "ListingId=" + ListingId
                   + ", Market=" + Market
                   + ", MarketplaceId=" + MarketplaceId
                   + ", IsChecked=" + IsChecked
                   + ", SKU=" + SKU;
        }

        public ListingPriceEditViewModel()
        {
            
        }

        public ListingPriceEditViewModel(ListingDTO listing)
        {
            ListingId = listing.Id;
            Market = (MarketType)listing.Market;
            MarketplaceId = listing.MarketplaceId;
            //ListingId = listing.ListingId;

            OnHold = listing.OnHold;

            IsFBA = listing.IsFBA;
            IsPrime = listing.IsPrime;
            Weight = listing.Weight;
            SKU = listing.SKU;

            ASIN = listing.ASIN;
            ParentASIN = listing.ParentASIN;
            SourceMarketId = listing.SourceMarketId;

            StyleSize = listing.StyleSize;
            StyleColor = listing.StyleColor;

            ListingSize = listing.ListingSize;
            ListingColor = listing.ListingColor;

            Price = listing.Price;
            LowestPrice = listing.LowestPrice;
            LowestPriceUpdateDate = listing.LowestPriceUpdateDate;
        }
    }
}