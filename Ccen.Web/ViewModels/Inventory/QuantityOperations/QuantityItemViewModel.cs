using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Common.Helpers;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Inventory
{
    public class QuantityItemViewModel
    {
        public string ItemOrderId { get; set; }

        public string StyleString { get; set; }
        public string ParentASIN { get; set; }
        public string ASIN { get; set; }
        public string SKU { get; set; }

        public string PictureUrl { get; set; }

        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public string Size { get; set; }
        public int Quantity { get; set; }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(PictureUrl, 0, 75, false, ImageHelper.NO_IMAGE_URL);
            }
        }

        public string ProductUrl
        {
            get { return UrlHelper.GetProductUrl(ParentASIN, (MarketType)Market, MarketplaceId); }
        }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }
        
        public QuantityItemViewModel(ListingOrderDTO item)
        {
            ItemOrderId = item.ItemOrderId;
            PictureUrl = ImageHelper.GetFirstOrDefaultPicture(item.ItemPicture); //NOTE: only Listing image, w/o style image
            Market = (MarketType)item.Market;
            MarketplaceId = item.MarketplaceId;

            ASIN = item.ASIN;
            SKU = item.SKU;
            ParentASIN = item.ParentASIN;
            StyleString = item.StyleID;
            Quantity = item.QuantityOrdered;
            Size = item.Size;
        }
    }
}