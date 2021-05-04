using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrdersByMarketplaceInfo
    {
        public string MarketplaceId { get; set; }
        public int Market { get; set; }
        public int OrderCount { get; set; }
        public int SecondDayOrderCount { get; set; }
        public int PaidExpeditedOrderCount { get; set; }

        public string MarketName
        {
            get { return MarketHelper.GetMarketName(Market, MarketplaceId); }
        }
    }
}