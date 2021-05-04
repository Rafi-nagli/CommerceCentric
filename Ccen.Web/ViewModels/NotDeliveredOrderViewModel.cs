using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models.SearchFilters;

namespace Amazon.Web.ViewModels
{
    public class NotDeliveredOrderViewModel
    {
        public long Id { get; set; }
        public long ShippingInfoId { get; set; }
        public int ShippingInfoType { get; set; }
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
        public string Timezone { get; set; }

        public bool IsDismiss { get; set; }
        public bool IsSubmittedClaim { get; set; }
        public bool IsHighlight { get; set; }

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


        public static IEnumerable<NotDeliveredOrderViewModel> GetAll(IUnitOfWork db,
            NotDeliveredFilterViewModel filter)
        {
            var orders = db.Orders.GetNotDeliveredList()
                .Select(o => new NotDeliveredOrderViewModel
                {
                    Id = o.Id,
                    ShippingInfoId = o.ShippingInfoId,
                    ShippingInfoType = o.ShippingInfoType,

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
                    
                    Timezone = o.Timezone,
                    PackageDeliveryDate = o.ActualDeliveryDate,
                    Carrier = o.Carrier,
                    TrackingNumber = o.TrackingNumber,
                    TrackingStateEvent = o.TrackingStateEvent,
                    TrackingStateDate = o.TrackingStateDate,

                    IsDismiss = o.NotDeliveredDismiss,
                    IsSubmittedClaim = o.NotDeliveredSubmittedClaim,
                    IsHighlight = o.NotDeliveredHighlight,
                });

            if (filter.DateFrom.HasValue)
                orders = orders.Where(o => o.OrderDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                orders = orders.Where(o => o.OrderDate <= filter.DateTo.Value);

            if (!String.IsNullOrEmpty(filter.BuyerName))
                orders = orders.Where(o => o.BuyerName.Contains(filter.BuyerName));

            if (!String.IsNullOrEmpty(filter.OrderNumber))
                orders = orders.Where(o => o.OrderNumber == filter.OrderNumber);

            if (filter.Status == NotDeliveredFilterViewModel.AllWithoutDismissedStatus)
                orders = orders.Where(o => !o.IsDismiss);

            if (filter.Status == NotDeliveredFilterViewModel.DismissStatus)
                orders = orders.Where(o => o.IsDismiss);

            if (filter.Status == NotDeliveredFilterViewModel.SubmittedClaimStatus)
                orders = orders.Where(o => o.IsSubmittedClaim);

            if (filter.Status == NotDeliveredFilterViewModel.HighlightStatus)
                orders = orders.Where(o => o.IsHighlight);

            return orders.OrderByDescending(o => o.OrderDate);
        }

        public static void Dismiss(IUnitOfWork db, long shippingId, ShippingInfoTypes shippingType)
        {
            if (shippingType == ShippingInfoTypes.Batch)
            {
                var shipping = db.OrderShippingInfos.Get(shippingId);
                shipping.NotDeliveredDismiss = true;
                db.Commit();
            }
            if (shippingType == ShippingInfoTypes.Mail)
            {
                var shipping = db.MailLabelInfos.Get(shippingId);
                shipping.NotDeliveredDismiss = true;
                db.Commit();
            }
        }

        public static void SubmitClaim(IUnitOfWork db, long shippingId, ShippingInfoTypes shippingType)
        {
            if (shippingType == ShippingInfoTypes.Batch)
            {
                var shipping = db.OrderShippingInfos.Get(shippingId);
                shipping.NotDeliveredSubmittedClaim = true;
                db.Commit();
            }
            if (shippingType == ShippingInfoTypes.Mail)
            {
                var shipping = db.MailLabelInfos.Get(shippingId);
                shipping.NotDeliveredSubmittedClaim = true;
                db.Commit();
            }
        }

        public static void Highlight(IUnitOfWork db, long shippingId, ShippingInfoTypes shippingType, bool isHighlight)
        {
            if (shippingType == ShippingInfoTypes.Batch)
            {
                var shipping = db.OrderShippingInfos.Get(shippingId);
                shipping.NotDeliveredHighlight = isHighlight;
                db.Commit();
            }
            if (shippingType == ShippingInfoTypes.Mail)
            {
                var shipping = db.MailLabelInfos.Get(shippingId);
                shipping.NotDeliveredHighlight = isHighlight;
                db.Commit();
            }
        }
    }
}