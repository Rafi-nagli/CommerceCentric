using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Api.AmazonECommerceService;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ListingQuantityViewModel
    {
        public long Id { get; set; }
        public string ListingId { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public bool IsFBA { get; set; }
        public string SKU { get; set; }
        public int ItemId { get; set; }
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string SourceMarketId { get; set; }
        
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }

        public string ListingSize { get; set; }
        public string ListingColor { get; set; }

        public int Quantity { get; set; }
        public int? DisplayQuantity { get; set; }
        public bool OnHold { get; set; }
        public DateTime? RestockDate { get; set; }
        public decimal Price { get; set; }

        public int? Rank { get; set; }

        public string ItemPicture { get; set; }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(ItemPicture,
                    75,
                    75,
                    false,
                    ImageHelper.NO_IMAGE_URL,
                    convertInDomainUrlToThumbnail: true);
            }
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
            get { return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, Market, MarketplaceId); }
        }

        public string SellerMarketUrl
        {
            get
            {
                return UrlHelper.GetSellarCentralInventoryUrl(ASIN, Market, MarketplaceId);
            }
        }

        public ListingQuantityViewModel()
        {
            
        }

        public ListingQuantityViewModel(ListingDTO listing)
        {
            Id = listing.Id;
            Market = (MarketType)listing.Market;
            MarketplaceId = listing.MarketplaceId;
            ListingId = listing.ListingId;

            OnHold = listing.OnHold;
            RestockDate = listing.RestockDate;

            ItemPicture = listing.ItemPicture;

            IsFBA = listing.IsFBA;
            SKU = listing.SKU;
            ItemId = listing.ItemId;
            ASIN = listing.ASIN;
            ParentASIN = listing.ParentASIN;
            SourceMarketId = listing.SourceMarketId;
            
            StyleSize = listing.StyleSize;
            StyleColor = listing.StyleColor;

            ListingSize = listing.ListingSize;
            ListingColor = listing.ListingColor;

            Quantity = listing.Quantity;
            DisplayQuantity = listing.DisplayQuantity;
            Price = listing.Price;
            Rank = listing.Rank;
        }
    }
}