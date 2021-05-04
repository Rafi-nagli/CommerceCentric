using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Entities.Messages;
using Amazon.Core.Models;
using Amazon.DTO.Messages;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation
{
    public class NotificationService : INotificationService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public NotificationService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        //private IList<NotificationDTO> _unreadNotificationList;

        //public void Init()
        //{
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        _unreadNotificationList = db.Notifications
        //            .GetAllAsDto()
        //            .Where(n => !n.IsRead)
        //            .ToList();
        //    }
        //}

        private bool CanHasDuplicateNotification(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.AmazonProductImageChanged:
                    return true;
                case NotificationType.LabelGotStuck:
                case NotificationType.LabelNeverShipped:
                    return false;
            }
            return true;
        }

        public void RemoveExist(string relatedEntityId, NotificationType type)
        {
            using (var db = _dbFactory.GetRWDb())
            { 
                var entity = db.Notifications.GetAll().FirstOrDefault(n => n.Type == (int)type && n.RelatedEntityId == relatedEntityId && !n.IsRead);
                if (entity != null)
                {
                    _log.Info("RemoveExist. Notification was marked as read, id=" + entity.Id);
                    db.Notifications.MarkAsRead(entity.Id, _time.GetAppNowTime(), null);
                    db.Commit();
                }
            }
        }

        public long? Add(NotificationDTO notification)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var canHasDuplicateNotification = CanHasDuplicateNotification((NotificationType)notification.Type);

                Notification exist = null;
                if (!canHasDuplicateNotification)
                {
                    exist = db.Notifications.GetFiltered(n => n.RelatedEntityId == notification.RelatedEntityId
                                                              && n.RelatedEntityType == (int) notification.RelatedEntityType
                                                              && n.Type == (int) notification.Type).FirstOrDefault();
                }

                if (exist == null)
                {
                    var notificaton = new Notification()
                    {
                        Type = notification.Type,

                        RelatedEntityId = notification.RelatedEntityId,
                        RelatedEntityType = (int) notification.RelatedEntityType,

                        AdditionalParams = notification.AdditionalParams,
                        Message = notification.Message,
                        Tag = notification.Tag,

                        CreateDate = notification.CreateDate ?? _time.GetAppNowTime(),
                        UpdateDate = notification.UpdateDate ?? _time.GetAppNowTime(),
                    };

                    db.Notifications.Add(notificaton);
                    db.Commit();

                    _log.Info("Notification was added, id=" + notificaton.Id + ", type=" + notificaton.Type +
                              ", entityId=" + notificaton.RelatedEntityId);

                    return notificaton.Id;
                }
                else
                {
                    exist.UpdateDate = _time.GetAppNowTime();
                    db.Commit();
                }
            }

            return null;
        }

        public long? Add(string entityId,
            EntityType entityType,
            string message,
            INotificationParams additionalParams,
            string tag,
            NotificationType type,
            DateTime? createDate = null)
        {
            var inputParams = JsonConvert.SerializeObject(additionalParams);

            return Add(new NotificationDTO()
            {
                Type = (int)type,

                RelatedEntityId = entityId,
                RelatedEntityType = (int)entityType,

                Message = message,
                AdditionalParams = inputParams,
                Tag = tag,

                CreateDate = createDate
            });
        }
    }
}
