using System;

namespace Amazon.Core.Contracts.Notifications.NotificationParams
{
    public class LabelNeverShippedParams : INotificationParams
    {
        public DateTime? BuyDate { get; set; }
        public string ShippingName { get; set; }

        public string OrderNumber { get; set; }
        public string Carrier { get; set; }
        public long ShippingInfoId { get; set; }
        public int LabelFromTypeId { get; set; }
        public int ReasonId { get; set; }
    }
}
