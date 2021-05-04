using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Messages
{
    public class NotificationInfoViewModel
    {
        public int UnreadNotificationUndeliveredCount { get; set; }
        public int UnreadNotificationUnshippedCount { get; set; }
        public int UnansweredMessageCount { get; set; }
        public DateTime? DhlPickupDate { get; set; }
        public TimeSpan? DhlReadyByTime { get; set; }
        public TimeSpan? DhlCloseTime { get; set; }
        public bool DhlPickupIsSuccess { get; set; }

        public static NotificationInfoViewModel GetInfo(IUnitOfWork db, ISettingsService settings, ITime time)
        {
            var notificationUndeliveredCount = db.Notifications.GetAllAsDto().Count(n => n.Type == (int)NotificationType.LabelGotStuck && !n.IsRead);
            var notificationUnshippedCount = db.Notifications.GetAllAsDto().Count(n => n.Type == (int)NotificationType.LabelNeverShipped && !n.IsRead);
            var messageCount = settings.GetUnansweredMessageCount() ?? 0; 

            var model = new NotificationInfoViewModel()
            {
                UnreadNotificationUndeliveredCount = notificationUndeliveredCount,
                UnreadNotificationUnshippedCount = notificationUnshippedCount,
                UnansweredMessageCount = messageCount,
            };

            var lastPickup = db.ScheduledPickups.GetLast(ShipmentProviderType.Dhl);
            if (lastPickup != null 
                && (lastPickup.RequestPickupDate > time.GetAppNowTime().Date
                || (lastPickup.RequestPickupDate == time.GetAppNowTime().Date
                && lastPickup.RequestCloseTime >= time.GetAppNowTime().TimeOfDay)))
            {
                model.DhlPickupDate = lastPickup.RequestPickupDate;
                model.DhlReadyByTime = lastPickup.RequestReadyByTime;
                model.DhlCloseTime = lastPickup.RequestCloseTime;
                model.DhlPickupIsSuccess = !String.IsNullOrEmpty(lastPickup.ConfirmationNumber);
            }
            else
            {
                model.DhlPickupIsSuccess = true;
            }

            return model;
        }
    }
}