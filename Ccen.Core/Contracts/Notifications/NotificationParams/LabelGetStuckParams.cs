using System;

namespace Amazon.Core.Contracts.Notifications.NotificationParams
{
    public class LabelGetStuckParams : INotificationParams
    {
        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }
        public string ShippingName { get; set; }

        public string OrderNumber { get; set; }
        public string Carrier { get; set; }
        public long ShippingInfoId { get; set; }
        public int LabelFromTypeId { get; set; }
        public int ReasonId { get; set; }
    }
}
