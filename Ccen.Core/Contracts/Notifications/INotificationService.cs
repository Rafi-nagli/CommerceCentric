using System;
using Amazon.Core.Models;
using Amazon.DTO.Messages;

namespace Amazon.Core.Contracts.Notifications
{
    public interface INotificationService
    {
        long? Add(NotificationDTO notification);

        long? Add(string entityId,
            EntityType entityType,
            string message,
            INotificationParams additionalParams,
            string additionalData,
            NotificationType type,
            DateTime? createDate = null);

        void RemoveExist(string relatedEntityId, 
            NotificationType type);
    }
}
