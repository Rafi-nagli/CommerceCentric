using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models.SearchFilters;

namespace Amazon.Web.ViewModels
{
    public class OrderFeedbackViewModel
    {
        public long Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string AmazonEmail { get; set; }

        public string OrderNumber { get; set; }
        public string OrderId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }


        public DateTime? ShippingDate { get; set; }
        public string ShippingService { get; set; }
        public string ShippingCountry { get; set; }

        public DateTime? PackageDeliveryDate { get; set; }
        public bool IsSent { get; set; }
        public DateTime? FeedbackRequestDate { get; set; }
        public string Timezone { get; set; }
        //public DateTime? DeliveryDate { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public string TrackingStateEvent { get; set; }
        public DateTime? TrackingStateDate { get; set; }

        public string TrackingUrl
        {
            get { return MarketUrlHelper.GetTrackingUrl(TrackingNumber, Carrier); }
        }

        public string SellerOrderUrl
        {
            get { return MarketUrlHelper.GetSellarCentralOrderUrl((MarketType) Market, MarketplaceId, OrderNumber, null); }
        }

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

        public static IEnumerable<OrderFeedbackViewModel> GetAll(IUnitOfWork db,
            FeedbackFilterViewModel filter)
        {
            var orders = db.Orders.GetListForFeedback()
                .Select(o => new OrderFeedbackViewModel
                {
                    Id = o.Id,

                    OrderNumber = o.OrderId,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,

                    AmazonEmail = o.AmazonEmail,
                    BuyerEmail = o.BuyerEmail,
                    OrderDate = o.OrderDate,
                    ShippingService = o.ShippingService,
                    BuyerName = o.PersonName,
                    ShippingDate = o.ShipDate,
                    ShippingCountry = o.ShippingCountry,
                    IsSent = o.IsFeedbackRequested,
                    FeedbackRequestDate = o.FeedbackRequestDate,
                    Timezone = o.Timezone,
                    PackageDeliveryDate = o.ActualDeliveryDate,
                    Carrier = o.Carrier,
                    TrackingNumber = o.TrackingNumber,
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
                orders = orders.Where(o => o.OrderNumber == filter.OrderNumber);

            if (filter.FeedbackStatus == FeedbackFilterViewModel.DeliveredNotSentStatus)
                orders = orders.Where(o => o.PackageDeliveryDate.HasValue
                    && !o.FeedbackRequestDate.HasValue);

            if (filter.FeedbackStatus == FeedbackFilterViewModel.AlreadySentStatus)
                orders = orders.Where(o => o.FeedbackRequestDate.HasValue);

            if (filter.FeedbackStatus == FeedbackFilterViewModel.NotDeliveredStatus)
                orders = orders.Where(o => !o.PackageDeliveryDate.HasValue);

            return orders.OrderByDescending(o => o.OrderDate);
        }

        public static CallResult<Exception> SendFeedback(IUnitOfWork db,
            IEmailService emailService,
            string orderId,
            DateTime when,
            long? by)
        {
            var emailInfo = emailService.GetEmailInfoByType(db,
                        EmailTypes.RequestFeedback,
                        null,
                        null,
                        orderId,
                        null,
                        null,
                        null);

            var result = emailService.SendEmail(emailInfo, CallSource.UI);

            if (result.Status == CallStatus.Success)
            {
                db.OrderEmailNotifies.Add(new OrderEmailNotify()
                {
                    OrderNumber = orderId,
                    Reason = "User emailed, web page",
                    Type = (int) OrderEmailNotifyType.OutputSendFeedbackEmail,
                    CreateDate = when,
                    CreatedBy = by
                });

                db.Orders.UpdateRequestedFeedback(orderId);
            }

            return result;
        }

    }
}