using System;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Messages;
using Amazon.DTO.Messages;

namespace Amazon.Core.Contracts.Db
{
    public interface INotificationRepository : IRepository<Notification>
    {
        IQueryable<NotificationDTO> GetAllAsDto();
        void RemoveById(long id);
        void MarkAsRead(long id, DateTime when, long? by);
    }
}
