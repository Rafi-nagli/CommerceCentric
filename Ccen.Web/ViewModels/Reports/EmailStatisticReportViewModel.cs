using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels
{
    public class EmailStatisticReportViewModel
    {
        public DateTime Date { get; set; }
        public int Hour { get; set; }
        public long IncomingEmails { get; set; }
        public long OutgoingEmails { get; set; }
        public long User1Emails { get; set; }
        public long User1Dismissed { get; set; }
        public long User2Emails { get; set; }
        public long User2Dismissed { get; set; }
        public long User3Emails { get; set; }
        public long User3Dismissed { get; set; }
        public long User4Emails { get; set; }
        public long User4Dismissed { get; set; }
        public long OtherEmails { get; set; }
        public long OtherDismissed { get; set; }
        public long SystemEmails { get; set; }


        public EmailStatisticReportViewModel()
        {

        }

        public static IEnumerable<EmailStatisticReportViewModel> GetAll(IUnitOfWork db,
            ITime time,
            DateTime? fromDate,
            DateTime? toDate)
        {
            fromDate = fromDate ?? time.GetAppNowTime().Date.AddDays(-1);
            var userIds = new long?[] { 52, 58, 48, 56 };

            var incomingEmailQuery = (from v in db.Emails.GetAll()
                                  where v.FolderType == (int)EmailFolders.Inbox
                                    && v.Type != (int)IncomeEmailTypes.Test
                                  select v);

            var outgoingEmailQuery = (from v in db.Emails.GetAll()
                                  where v.FolderType == (int)EmailFolders.Sent
                                    && v.Type != (int)IncomeEmailTypes.Test
                                  select v);

            var dismissedStatus = ((int)EmailResponseStatusEnum.NoResponseNeeded).ToString();
            var dismissedEventQuery = (from oh in db.OrderChangeHistories.GetAll()
                                   where oh.FieldName == OrderHistoryHelper.EmailStatusChangedKey
                                    && oh.ToValue == dismissedStatus
                                    && oh.ChangedBy.HasValue
                                   select new
                                   {
                                       Date = DbFunctions.TruncateTime(oh.ChangeDate),
                                       UserId = oh.ChangedBy,                                       
                                   });

            if (fromDate.HasValue)
            {
                incomingEmailQuery = incomingEmailQuery.Where(i => i.CreateDate >= fromDate);
                outgoingEmailQuery = outgoingEmailQuery.Where(i => i.CreateDate >= fromDate);
                dismissedEventQuery = dismissedEventQuery.Where(i => i.Date >= fromDate);
            }

            if (toDate.HasValue)
            {
                incomingEmailQuery = incomingEmailQuery.Where(i => i.CreateDate < toDate);
                outgoingEmailQuery = outgoingEmailQuery.Where(i => i.CreateDate < toDate);
                dismissedEventQuery = dismissedEventQuery.Where(i => i.Date < toDate);
            }

            var incomingEmailByHour = (from e in incomingEmailQuery
                                       group e by new { Date = DbFunctions.TruncateTime(e.CreateDate) } into byHour
                                      select new
                                      {
                                          Date = byHour.Key.Date,
                                          Count = byHour.Select(e => e.Id).Count()
                                      }).ToList();

            var outgoingEmailByHourAndUser = (from e in outgoingEmailQuery
                                       group e by new { UserId = e.CreatedBy, Date = DbFunctions.TruncateTime(e.CreateDate) } into byHour
                                       select new
                                       {
                                           UserId = byHour.Key.UserId,
                                           Date = byHour.Key.Date,
                                           Count = byHour.Select(e => e.Id).Count()
                                       }).ToList();

            var dismissedEvents = dismissedEventQuery.Where(e => userIds.Contains(e.UserId)).ToList();
                        
            var usersToShow = db.Users.GetAll().Where(u => userIds.Contains(u.Id)).ToList();

            var results = new List<EmailStatisticReportViewModel>();
            for (var i = 0; i < (toDate.Value - fromDate.Value).TotalDays; i++)
            {
                var date = fromDate.Value.AddDays(i);
                results.Add(new EmailStatisticReportViewModel()
                {
                    Date = date,
                    IncomingEmails = incomingEmailByHour.FirstOrDefault(e => e.Date == date)?.Count ?? 0,
                    OutgoingEmails = outgoingEmailByHourAndUser.Where(e => e.Date == date).Sum(e => (int?)e.Count) ?? 0,
                    User1Emails = outgoingEmailByHourAndUser.Where(e => e.Date == date && e.UserId == userIds[0]).Sum(e => (int?)e.Count) ?? 0,
                    User2Emails = outgoingEmailByHourAndUser.Where(e => e.Date == date && e.UserId == userIds[1]).Sum(e => (int?)e.Count) ?? 0,
                    User3Emails = outgoingEmailByHourAndUser.Where(e => e.Date == date && e.UserId == userIds[2]).Sum(e => (int?)e.Count) ?? 0,
                    User4Emails = outgoingEmailByHourAndUser.Where(e => e.Date == date && e.UserId == userIds[3]).Sum(e => (int?)e.Count) ?? 0,
                    OtherEmails = outgoingEmailByHourAndUser.Where(e => e.Date == date && e.UserId.HasValue && !userIds.Contains(e.UserId)).Sum(e => (int?)e.Count) ?? 0,

                    User1Dismissed = dismissedEvents.Where(e => e.Date == date && e.UserId == userIds[0]).Count(),
                    User2Dismissed = dismissedEvents.Where(e => e.Date == date && e.UserId == userIds[1]).Count(),
                    User3Dismissed = dismissedEvents.Where(e => e.Date == date && e.UserId == userIds[2]).Count(),
                    User4Dismissed = dismissedEvents.Where(e => e.Date == date && e.UserId == userIds[3]).Count(),
                    OtherDismissed = dismissedEvents.Where(e => e.Date == date && e.UserId.HasValue && !userIds.Contains(e.UserId)).Count(),

                    SystemEmails = outgoingEmailByHourAndUser.Where(e => e.Date == date && !e.UserId.HasValue).Sum(e => (int?)e.Count) ?? 0,
                });
            }

            return results;
        }
    }
}