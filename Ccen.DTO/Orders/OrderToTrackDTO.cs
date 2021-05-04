using System;

namespace Amazon.DTO
{
    public class OrderToTrackDTO
    {
        public long OrderId { get; set; }

        public long? ShipmentInfoId { get; set; }
        public long? MailInfoId { get; set; }

        public int ReasonId { get; set; }

        public DateTime? LastTrackingRequestDate { get; set; }
        public int? TrackingRequestAttempts { get; set; }

        public int TrackingStateSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateEvent { get; set; }
        public string TrackingLocation { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }
        public int? DeliveredStatus { get; set; }
        public bool IsDelivered { get; set; }


        public long TrackingId { get; set; }
        public string OrderNumber { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string OrderStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? EstDeliveryDate { get; set; }

        public string ShippingName { get; set; }
        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public bool LabelCanceled { get; set; }
        public bool CancelLabelRequested { get; set; }

        public int ShipmentProviderType { get; set; }
        public int? ShippingMethodId { get; set; }

        public string PersonName { get; set; }
        public string BuyerName { get; set; }
        public string Comment { get; set; }

        public DateTime? BuyDate { get; set; }

        public AddressDTO ShippingAddress { get; set; }
    }
}
