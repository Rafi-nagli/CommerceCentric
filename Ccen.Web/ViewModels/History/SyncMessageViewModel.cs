using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels.Reports
{
    public class SyncMessageViewModel
    {
        public long Id { get; set; }
        public long SyncHistoryId { get; set; }
        public int Status { get; set; }
        public string EntityId { get; set; }
        [AllowHtml]
        public string Message { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public DateTime? CreateDate { get; set; }

        public DateTime? FormattedCreateDate
        {
            get { return DateHelper.ConvertUtcToApp(CreateDate); }
        }
        public string FormattedMessages
        {
            get
            {
                return Messages != null && Messages.Any() ? "-" + string.Join(";<br/>-", Messages) : "-";
            }
        }
        
        public string FormattedStatus
        {
            get
            {
                switch ((SyncMessageStatus)Status)
                {
                    case SyncMessageStatus.Error:
                        return "Error";
                    case SyncMessageStatus.Warning:
                        return "Warning";
                    case SyncMessageStatus.Info:
                        return "Info";
                    case SyncMessageStatus.Success:
                        return "Success";
                }
                return "Info";
            }
        }

        public static IList<SyncMessageViewModel> GetAll(IUnitOfWork context, long syncHistoryId)
        {
            return context.SyncMessages.GetAll()
                .Where(m => m.SyncHistoryId == syncHistoryId)
                .GroupBy(m => m.EntityId)
                .Select(m => new SyncMessageViewModel()
                {
                    //Id = s.Id,
                    SyncHistoryId = m.Max(s => s.SyncHistoryId),
                    Status = m.Max(s => s.Status),
                    CreateDate = m.Max(s => s.CreateDate),
                    EntityId = m.Key,
                    Messages = m.Select(s => s.Message)
                }).ToList();
        }
    }
}