using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.ViewModels.Results;

namespace Amazon.Web.ViewModels
{
    public class OrderToTrackViewModel
    {
        public long TrackingId { get; set; }

        public string OrderNumber { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? EstDeliveryDate { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }
        public string PersonName { get; set; }
        public string BuyerName { get; set; }

        public string Comment { get; set; }


        public long? MailInfoId { get; set; }
        public long? ShippingInfoId { get; set; }

        public string TrackingStateEvent { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        public string TrackingUrl
        {
            get
            {
                return MarketUrlHelper.GetTrackingUrl(TrackingNumber, Carrier);
            }
        }

        public string SellerOrderUrl
        {
            get { return MarketUrlHelper.GetSellarCentralOrderUrl((MarketType)Market, MarketplaceId, OrderNumber, null); }
        }

        public bool FromMailPage
        {
            get { return MailInfoId.HasValue; }
        }

        public IList<MessageString> Validate(IUnitOfWork db)
        {
            var messages = new List<MessageString>();

            var order = db.Orders.GetByOrderIdAsDto(OrderNumber);

            if (!String.IsNullOrEmpty(TrackingNumber))
            {
                var shipping = db.OrderShippingInfos.GetAll().FirstOrDefault(sh => sh.TrackingNumber == TrackingNumber);
                MailLabelInfo mail = null;
                if (shipping == null)
                    mail = db.MailLabelInfos.GetAll().FirstOrDefault(m => m.TrackingNumber == TrackingNumber);

                if (shipping == null && mail == null)
                    messages.Add(MessageString.From("TrackingNumber", "Tracking number not found"));

                return messages;
            }

            if (order == null)
                 messages.Add(MessageString.From("OrderNumber", "Order not found"));

            return messages;
        }

        public void Submit(IUnitOfWork db, DateTime when, long? by)
        {
            var trackings = new List<OrderShippingInfoDTO>();

            if (String.IsNullOrEmpty(TrackingNumber))
            {
                var order = db.Orders.GetByOrderIdAsDto(OrderNumber);
                var shippingTrackings = db.OrderShippingInfos.GetByOrderIdAsDto(order.Id)
                    .Where(sh => sh.IsActive && !String.IsNullOrEmpty(sh.TrackingNumber))
                    .ToList();
                trackings.AddRange(shippingTrackings);

                var mailTrackings = db.MailLabelInfos.GetByOrderIdsAsDto(new List<long>() {order.Id});
                trackings.AddRange(mailTrackings);
            }
            else
            {
                trackings.Add(new OrderShippingInfoDTO()
                {
                    TrackingNumber = TrackingNumber,
                    ShippingMethod = new ShippingMethodDTO()
                    {
                        CarrierName = Carrier
                    }
                });
            }

            foreach (var tracking in trackings)
            {
                db.TrackingOrders.Add(new TrackingOrder()
                {
                    TrackingNumber = tracking.TrackingNumber,
                    Carrier = tracking.ShippingMethod.CarrierName,
                    Comment = Comment,
                    CreateDate = when,
                    CreatedBy = by,
                });
            }

            db.Commit();
        }

        public static IEnumerable<OrderToTrackViewModel> GetAll(IUnitOfWork db)
        {
            var orders = db.Orders.GetOrdersToTrack();

            return orders.Select(o => new OrderToTrackViewModel
            {
                TrackingId = o.TrackingId,
                
                OrderNumber = o.OrderNumber,
                Market = o.Market,
                MarketplaceId = o.MarketplaceId,
                TrackingNumber = o.TrackingNumber,
                
                EstDeliveryDate = o.EstDeliveryDate,
                OrderDate = o.OrderDate,
                ActualDeliveryDate = o.ActualDeliveryDate,
                PersonName = o.PersonName,
                BuyerName = o.BuyerName,
                Comment = o.Comment,

                TrackingStateEvent = o.TrackingStateEvent,
                TrackingStateDate = o.TrackingStateDate,
                LastUpdateDate = o.LastTrackingRequestDate,

                MailInfoId = o.MailInfoId,
                ShippingInfoId = o.ShipmentInfoId,
            });
        }
    }
}