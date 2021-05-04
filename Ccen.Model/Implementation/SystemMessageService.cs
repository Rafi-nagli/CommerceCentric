using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models.Calls;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation
{
    public class SystemMessageService : ISystemMessageService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public SystemMessageService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void AddOrUpdateError(string name,
            string tag,
            string message,
            ISystemMessageData data)
        {
            AddOrUpdate(name, tag, message, data, MessageStatus.Error);
        }

        public void AddOrUpdateWarning(string name,
            string tag,
            string message,
            ISystemMessageData data)
        {
            AddOrUpdate(name, tag, message, data, MessageStatus.Warning);
        }

        public void AddOrUpdate(string name,
            string tag,
            string message,
            ISystemMessageData data,
            MessageStatus status)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var query = db.SystemMessages.GetAll().Where(m => m.Name == name);
                if (String.IsNullOrEmpty(tag))
                    query = query.Where(m => String.IsNullOrEmpty(tag));
                else
                    query = query.Where(m => m.Tag == tag);

                var existMissingOrder = query.FirstOrDefault();
                if (existMissingOrder == null)
                {
                    existMissingOrder = new Core.Entities.SystemMessage()
                    {
                        Name = name,
                        Tag = tag,
                        CreateDate = _time.GetAppNowTime()
                    };
                    db.SystemMessages.Add(existMissingOrder);
                }
                existMissingOrder.UpdateDate = _time.GetAppNowTime();
                existMissingOrder.Message = message;
                existMissingOrder.Data = data != null ? JsonHelper.Serialize(data) : null;
                existMissingOrder.Status = (int)status;
                db.Commit();
            }
        }

        public void Remove(string name, string tag)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var query = db.SystemMessages.GetAll().Where(m => m.Name == name);
                if (String.IsNullOrEmpty(tag))
                    query = query.Where(m => String.IsNullOrEmpty(m.Tag));
                else
                    query = query.Where(m => m.Tag == tag);

                var existMissingOrder = query.FirstOrDefault();
                if (existMissingOrder != null)
                {
                    db.SystemMessages.Remove(existMissingOrder);
                    db.Commit();
                }
            }
        }

        public IList<SystemMessageDTO> GetAllByName(string name)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var query = db.SystemMessages.GetAllAsDto().Where(m => m.Name == name);
                return query.ToList();
            }
        }
    }
}
