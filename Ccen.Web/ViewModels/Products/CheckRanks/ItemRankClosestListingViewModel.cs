using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemRankClosestListingViewModel
    {
        public string ASIN { get; set; }
        public string SKU { get; set; }
        public long ListingId { get; set; }
        
        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public string StyleSize { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public int? Rank { get; set; }
        public DateTime? RankUpdateDate { get; set; }

        public int? SoldUnits { get; set; }

        public string MarketUrl
        {
            get { return UrlHelper.GetMarketUrl(ASIN, null, (MarketType)Market, MarketplaceId); }
        }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }
    }
}