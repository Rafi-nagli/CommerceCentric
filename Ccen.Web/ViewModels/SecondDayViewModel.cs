using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels
{
    public class SecondDayViewModel
    {
        public string OrderNumber { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? EstDeliveryDate { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string PersonName { get; set; }
        public string BuyerName { get; set; }
        

        public string MarketOrderUrl
        {
            get { return UrlHelper.GetSellerCentralOrderUrl((MarketType)Market, MarketplaceId, OrderNumber); }
        }

        public bool IsLate
        {
            get
            {
                return EstDeliveryDate != null && ActualDeliveryDate != null &&
                       EstDeliveryDate.Value.Date < ActualDeliveryDate.Value.Date;
            }
        }

        public string TrackingToDisplay
        {
            get { return TrackingNumber ?? ""; }
        }

        public string TrackingUrl
        {
            get
            {
                return !string.IsNullOrEmpty(TrackingNumber)
                    ? "https://tools.usps.com/go/TrackConfirmAction!input.action?tRef=qt&tLc=1&tLabels=" +
                      TrackingNumber
                    : "";
            }
        }

        public static IEnumerable<SecondDayViewModel> GetAll(IUnitOfWork db, 
            OrderSearchFilterViewModel search)
        {
            var orders = db.Orders.GetSecondDayOrders(search.DateFrom, search.DateTo).OrderByDescending(o => o.OrderDate).ToList();

            return orders.Select(o => new SecondDayViewModel
            {
                OrderNumber = o.OrderNumber,
                Market = o.Market,
                MarketplaceId = o.MarketplaceId,

                TrackingNumber = o.TrackingNumber,
                EstDeliveryDate = o.EstDeliveryDate,
                OrderDate = o.OrderDate,
                ActualDeliveryDate = o.ActualDeliveryDate,
                PersonName = o.PersonName,
                BuyerName = o.BuyerName,
            });
        }
    }
}