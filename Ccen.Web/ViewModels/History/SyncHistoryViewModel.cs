using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels.History
{
    public class SyncHistoryViewModel
    {
        public long Id { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }

        public int ProcessedTotal { get; set; }
        public int ProcessedWithError { get; set; }
        public int ProcessedWithWarning { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PingDate { get; set; }

        public string FormattedType
        {
            get { return (SyncType)Type == SyncType.Orders ? "Orders sync" : "Listings sync"; }
        }

        public string FormattedStatus
        {
            get
            {
                switch ((SyncStatus)Status)
                {
                    case SyncStatus.InProgresss:
                        return "In-progress";
                    case SyncStatus.Complete:
                        return "Complete";
                    case SyncStatus.CompleteWithWarnings:
                        return "Complete with warnings";
                    case SyncStatus.CompleteWithErrors:
                        return "Complete with errors";
                }
                return "Unknown";
            }
        }

        public DateTime? FormattedEndDate
        {
            get { return DateHelper.ConvertUtcToApp(EndDate); }
        }

        public DateTime? FormattedStartDate
        {
            get { return DateHelper.ConvertUtcToApp(StartDate); }
        }

        public static IQueryable<SyncHistoryViewModel> GetAll(IUnitOfWork context)
        {
            var startDate = DateHelper.GetAppNowTime().AddHours(-48);

            return context.SyncHistory.GetAll().Where(s => s.StartDate >= startDate).Select(s =>
                new SyncHistoryViewModel
                {
                    Id = s.Id,
                    Type = s.Type,
                    Status = s.Status,

                    ProcessedTotal = s.ProcessedTotal,
                    ProcessedWithWarning = s.ProcessedWithWarning,
                    ProcessedWithError = s.ProcessedWithError,

                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    PingDate = s.PingDate
                });
        }
    }
}