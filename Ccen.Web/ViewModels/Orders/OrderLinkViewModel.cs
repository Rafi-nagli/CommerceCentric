using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderLinkViewModel
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OrderStatus { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string MarketName
        {
            get { return MarketHelper.GetMarketName(Market, MarketplaceId); }
        }

        public string OrderHistoryUrl
        {
            get { return UrlHelper.GetOrderHistoryUrl(OrderNumber); }
        }

        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderNumber); }
        }
    }
}