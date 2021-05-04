using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;

namespace Amazon.Web.Models.SearchFilters
{
    public class FeedFilterViewModel
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int? Type { get; set; }

        public SelectList TypeList
        {
            get
            {
                return new SelectList(FeedTypes, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> FeedTypes = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(AmazonFeedTypeHelper.ToString(AmazonFeedType.Product), (int)AmazonFeedType.Product),
            new KeyValuePair<string, int>(AmazonFeedTypeHelper.ToString(AmazonFeedType.Inventory), (int)AmazonFeedType.Inventory),
            new KeyValuePair<string, int>(AmazonFeedTypeHelper.ToString(AmazonFeedType.Price), (int)AmazonFeedType.Price),
        };

        public static FeedFilterViewModel Default
        {
            get
            {
                return new FeedFilterViewModel()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId,
                    Type = (int)AmazonFeedType.Product
                };
            }
        }
    }
}