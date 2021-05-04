using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemMarketInfoViewModel
    {
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string SKU { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string MarketShortName
        {
            get { return MarketHelper.GetShortName(Market, MarketplaceId); }
        }

        public string ProductUrl
        {
            get
            {
                return UrlHelper.GetProductUrl(ParentASIN, (MarketType)Market, MarketplaceId);
            }
        }

        public float MarketIndex
        {
            get { return MarketHelper.GetMarketIndex((MarketType)Market, MarketplaceId); }
        }

        public ItemMarketInfoViewModel(ItemMarketInfoDTO item)
        {
            ASIN = item.ASIN;
            ParentASIN = item.ParentASIN;
            SKU = item.SKU;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;
        }
    }
}