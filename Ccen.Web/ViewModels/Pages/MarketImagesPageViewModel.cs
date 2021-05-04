using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.Pages
{
    public class MarketImagesPageViewModel
    {
        public SelectList MarketList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Amazon US", ((int)MarketType.Amazon) + "_" + MarketplaceKeeper.AmazonComMarketplaceId),
                    new KeyValuePair<string, string>("Walmart", ((int)MarketType.Walmart) + "_" + "")
                }, "Value", "Key");
            }
        }
    }
}