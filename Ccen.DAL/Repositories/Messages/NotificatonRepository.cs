using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Messages;
using Amazon.DTO.Messages;

namespace Amazon.DAL.Repositories.Messages
{
    public class NotificatonRepository : Repository<Notification>, INotificationRepository
    {
        public NotificatonRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public IQueryable<NotificationDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public void RemoveById(long id)
        {
            var notification = Get(id);
            if (notification != null)
                Remove(notification);
        }

        public void MarkAsRead(long id, DateTime when, long? by)
        {
            var notification = Get(id);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadDate = when;
                notification.ReadBy = by;
            }
        }

        private IQueryable<NotificationDTO> AsDto(IQueryable<Notification> query)
        {
            return query.Select(n => new NotificationDTO()
            {
                Id = n.Id,
                Type = n.Type,

                Message = n.Message,
                Tag = n.Tag,
                AdditionalParams = n.AdditionalParams,

                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,

                IsRead = n.IsRead,
                ReadDate = n.ReadDate,
                ReadBy = n.ReadBy,

                CreateDate = n.CreateDate,
                UpdateDate = n.UpdateDate,
            });
        }
    }
}
