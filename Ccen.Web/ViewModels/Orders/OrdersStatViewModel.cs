using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.Model.Implementation;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrdersStatViewModel
    {
        public IList<OrdersByMarketplaceInfo> Marketplaces { get; set; }

        public int TotalSecondDay
        {
            get { return Marketplaces.Sum(m => m.SecondDayOrderCount); }
        }

        public int TotalPaidExpedited
        {
            get { return Marketplaces.Sum(m => m.PaidExpeditedOrderCount); }
        }

        public int TotalOrdersCount { get; set; }

        public int OverdueOrdersCount { get; set; } 

        public static OrdersStatViewModel Get(IUnitOfWork db,
            MarketplaceKeeper marketplaceManager)
        {
            var model = new OrdersStatViewModel();

            var today = DateHelper.GetAppNowTime().Date;

            var orders = db.Orders.GetUnshippedOrders();
            model.TotalOrdersCount = orders.Count;
            var marketplaces = marketplaceManager.GetAll();
            model.Marketplaces = marketplaces.Select(m => new OrdersByMarketplaceInfo()
            {
                Market = m.Market,
                MarketplaceId = m.MarketplaceId,
                OrderCount = orders.Count(o => o.Market == m.Market 
                    && (o.MarketplaceId == m.MarketplaceId || String.IsNullOrEmpty(m.MarketplaceId))),
                PaidExpeditedOrderCount = orders.Count(o => o.Market == m.Market
                                                            && (o.MarketplaceId == m.MarketplaceId || String.IsNullOrEmpty(m.MarketplaceId))
                                                            && ShippingUtils.IsServiceExpedited(o.InitialServiceType)),
                SecondDayOrderCount = orders.Count(o => o.Market == m.Market
                                                        && (o.MarketplaceId == m.MarketplaceId || String.IsNullOrEmpty(m.MarketplaceId))
                                                        && (ShippingUtils.IsServiceTwoDays(o.InitialServiceType)
                                                            || ShippingUtils.IsServiceNextDay(o.InitialServiceType))),
            }).ToList();

            model.OverdueOrdersCount = orders.Count(o => o.LatestShipDate.HasValue &&
                ShippingUtils.AlignMarketDateByEstDayEnd(o.LatestShipDate.Value, (MarketType)o.Market) < today);

            return model;
        }
    }
}