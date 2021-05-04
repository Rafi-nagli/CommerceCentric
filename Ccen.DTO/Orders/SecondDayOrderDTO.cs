using System;

namespace Amazon.DTO
{
    public class SecondDayOrderDTO
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }


        public DateTime? OrderDate { get; set; }
        public DateTime? EstDeliveryDate { get; set; }
        public string TrackingNumber { get; set; }
        public int ShipmentProviderType { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string PersonName { get; set; }
        public string BuyerName { get; set; }
    }
}
