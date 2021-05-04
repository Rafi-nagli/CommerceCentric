using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Web.Models.SearchFilters;

namespace Amazon.Web.ViewModels
{
    public class OrderEmailViewModel
    {
        public long Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string AmazonEmail { get; set; }
        public string OrderNumber { get; set; }
        public string OrderId { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string ShippingService { get; set; }
        public string ShippingCountry { get; set; }

        public DateTime? PackageDeliveryDate { get; set; }
        public bool IsSent { get; set; }
        public DateTime? FeedbackRequestDate { get; set; }
        public string Timezone { get; set; }
        //public DateTime? DeliveryDate { get; set; }

        public string TrackingStateEvent { get; set; }
        public DateTime? TrackingStateDate { get; set; }

        public string FormattedShippingService
        {
            get { return ShippingUtils.MethodNameToAmazonService(ShippingService); }
        }

        public bool IsInternational
        {
            get
            {
                return ShippingUtils.IsInternational(ShippingCountry);
            }
        }

        public string FormattedBuyerEmail
        {
            get { return String.IsNullOrEmpty(AmazonEmail) ? (BuyerEmail ?? "") : AmazonEmail; }
        }

        public string FeedbackRequestDateString
        {
            get
            {
                return FeedbackRequestDate.HasValue
                    ? FeedbackRequestDate.Value.ToString("ddd, MM.dd.yyyy")
                    : string.Empty;
            }
        }

        public static IEnumerable<OrderEmailViewModel> GetAll(IUnitOfWork db,
            FeedbackFilterViewModel filter)
        {
            var orders = db.Orders.GetListForFeedback()
                .Select(o => new OrderEmailViewModel
                {
                    Id = o.Id,
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderId,
                    AmazonEmail = o.AmazonEmail,
                    BuyerEmail = o.BuyerEmail,
                    OrderDate = o.OrderDate,
                    ShippingService = o.ShippingService,
                    BuyerName = o.PersonName,
                    ShippingDate = o.ShipDate,
                    ShippingCountry = o.ShippingCountry,
                    IsSent = o.IsEmailed,
                    FeedbackRequestDate = o.FeedbackRequestDate,
                    Timezone = o.Timezone,
                    PackageDeliveryDate = o.ActualDeliveryDate,
                    TrackingStateEvent = o.TrackingStateEvent,
                    TrackingStateDate = o.TrackingStateDate,
                });

            if (filter.DateFrom.HasValue)
                orders = orders.Where(o => o.OrderDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                orders = orders.Where(o => o.OrderDate <= filter.DateTo.Value);

            if (!String.IsNullOrEmpty(filter.BuyerName))
                orders = orders.Where(o => o.BuyerName.Contains(filter.BuyerName));

            if (!String.IsNullOrEmpty(filter.OrderNumber))
                orders = orders.Where(o => o.OrderId == filter.OrderNumber);

            if (!filter.IsIncludeNotDelivered)
                orders = orders.Where(o => o.PackageDeliveryDate.HasValue);

            if (!filter.IsIncludeSent)
                orders = orders.Where(o => !o.FeedbackRequestDate.HasValue);


            return orders.OrderByDescending(o => o.OrderDate);
        }
    }
}