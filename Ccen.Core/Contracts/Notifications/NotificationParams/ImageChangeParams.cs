namespace Amazon.Core.Contracts.Notifications.NotificationParams
{
    public class ImageChangeParams : INotificationParams
    {
        public int MarketType { get; set; }
        public string MarketplaceId { get; set; }
        public string ParentASIN { get; set; }

        public string PreviousImage { get; set; }
        public string NewImage { get; set; }
        public decimal ImageDiff { get; set; }
    }
}
