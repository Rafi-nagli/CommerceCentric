using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Products.Edits
{
    public class ItemMarketViewModel
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public bool IsAlreadyExists { get; set; }
        public bool IsSelected { get; set; }

        public string Name
        {
            get { return MarketHelper.GetMarketName(Market, MarketplaceId); }
        }
    }
}