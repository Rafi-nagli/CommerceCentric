using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.General.ViewModels.Inventory
{
    public class StyleItemPublishViewModel
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public decimal? Price { get; set; }

        public bool IsPublished { get; set; }

        public string MarketName
        {
            get { return MarketHelper.GetMarketName(Market, MarketplaceId); }
        }

        public StyleItemPublishViewModel()
        {

        }
    }
}