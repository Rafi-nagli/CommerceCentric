using System;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.ViewModels.Mailing
{
    public class ReturnQuantityItemViewModel
    {
        public string ItemOrderId { get; set; }

        public string StyleString { get; set; }
        public string ParentASIN { get; set; }
        public string ASIN { get; set; }
        public string SKU { get; set; }

        public string ItemTitle { get; set; }

        public string PictureUrl { get; set; }

        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public string PriceCurrency { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemPriceInUSD { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal ShippingDiscount { get; set; }
        public decimal ShippingTax { get; set; }

        public string Size { get; set; }
        public int Quantity { get; set; }

        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }


        public int InputQuantity { get; set; }
        public int InputDamagedQuantity { get; set; }

        public string ExchangeStyleString { get; set; }
        public int? ExchangeStyleId { get; set; }
        public int? ExchangeStyleItemId { get; set; }

        public decimal RefundItemPrice { get; set; }
        public decimal RefundShippingPrice { get; set; }
        public decimal DeductShippingPrice { get; set; }
        public decimal DeductPrepaidLabelCost { get; set; }

        public double? Weight { get; set; }

        [ToStringIgnore]
        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(PictureUrl, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }

        [ToStringIgnore]
        public string ProductUrl
        {
            get { return UrlHelper.GetProductUrl(ParentASIN, (MarketType)Market, MarketplaceId); }
        }

        [ToStringIgnore]
        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public StyleLocationDTO DefaultLocation { get; set; }

        public ReturnQuantityItemViewModel()
        {
            
        }

        public ReturnQuantityItemViewModel(ListingOrderDTO item)
        {
            ItemOrderId = item.ItemOrderId;
            var image = item.ItemPicture;
            if (String.IsNullOrEmpty(image))
                image = item.StyleImage;
            PictureUrl = ImageHelper.GetFirstOrDefaultPicture(image);

            Market = (MarketType)item.Market;
            MarketplaceId = item.MarketplaceId;

            ASIN = item.ASIN;
            SKU = item.SKU;
            ParentASIN = item.ParentASIN;
            StyleString = item.StyleID;
            ItemTitle = item.Title;

            Quantity = item.QuantityOrdered;
            Size = item.Size;
            DefaultLocation = item.Locations != null && item.Locations.Any() ? item.Locations.OrderByDescending(l => l.IsDefault).First() : null;
            
            PriceCurrency = PriceHelper.FormatCurrency(item.ItemPriceCurrency);
            ItemPrice = item.ItemPrice;
            ItemTax = item.ItemTax ?? 0;
            ItemPriceInUSD = PriceHelper.RougeConvertToUSD(PriceCurrency, item.ItemPrice);
            ShippingPrice = item.ShippingPrice;
            ShippingDiscount = item.ShippingDiscount ?? 0;
            ShippingTax = item.ShippingTax ?? 0;

            Weight = item.Weight ?? 0;

            StyleId = item.StyleId;
            StyleItemId = item.StyleItemId;

            InputQuantity = Quantity;
            InputDamagedQuantity = 0;
        }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}