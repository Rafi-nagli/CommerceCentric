using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Notifications.SupportNotifications
{
    public class ListingCreationIssuesNotification : ISupportNotification
    {
        public string Name { get { return "Listing Creation Issues"; } }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(9) }; }
        }

        public ListingCreationIssuesNotification(IDbFactory dbFactory,
            IEmailService emailService,
            ILogService log,
            ITime time)
        {
            _time = time;
            _log = log;
            _emailService = emailService;
            _dbFactory = dbFactory;
        }

        public void Check()
        {
            if (!_time.IsBusinessDay(_time.GetAppNowTime().Date))
                return;

            var warningPeriod = _time.AddBusinessDays(_time.GetAppNowTime().Date, -1);
            var minDate = DateTime.Now.AddMonths(-3);
            _log.Info("warningPeriod=" + warningPeriod);

            var messages = new List<string>();
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAll().Where(i => i.CreateDate > warningPeriod
                    && i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                    && i.CreatedBy.HasValue)
                    .ToList();

                var users = db.Users.GetAllAsDto().ToList();

                var userIds = items.Select(i => i.CreatedBy).Distinct().ToList();

                foreach (var userId in userIds)
                {
                    var user = users.FirstOrDefault(u => u.Id == userId);
                    if (user == null)
                        continue;

                    var userItems = items.Where(i => i.CreatedBy == userId)
                        .OrderBy(i => i.CreateDate)
                        .ToList();

                    var message = String.Format("The following listings (created by {0}) have publishing errors: <br/>", user.Name)
                        + String.Join("<br/>", userItems.Select(i => DateHelper.ToDateTimeString(i.CreateDate) + " - " + i.ASIN));// + " - " + i.ItemPublishedStatusReason));

                    _log.Info(user.Email + " - " + message);
                    _emailService.SendSystemEmail("System Notification: " + Name + " - To Review (" + userItems.Count() + ")",
                        message,
                        user.Email,
                        EmailHelper.RafiEmail + ", " + EmailHelper.SupportDgtexEmail);
                }
            }
        }
    }
}
