using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Web.Models;

namespace Amazon.Model.Implementation.Notifications.SupportNotifications
{
    public class QtyDistributionSupportNotification : ISupportNotification
    {
        public string Name { get { return "QuantityDistribution"; } }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(3) }; }
        }


        public QtyDistributionSupportNotification(IDbFactory dbFactory,
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
            var warningPeriod = TimeSpan.FromHours(12);
            var settings = new SettingsService(_dbFactory);
            var lastQtyDistributeDate = settings.GetQuantityDistributeDate();
            if (!lastQtyDistributeDate.HasValue
                || lastQtyDistributeDate < _time.GetAppNowTime().Subtract(warningPeriod))
            {
                var message = "Quantities were redistributed more than " + warningPeriod + ", at: " +
                              lastQtyDistributeDate;
                _log.Info(message);
                _emailService.SendSystemEmailToAdmin("Support Notification: " + Name + " - To Review",
                    message);
            }
            else
            {
                var message = "Redistributed at " + lastQtyDistributeDate;
                _emailService.SendSystemEmailToAdmin("Support Notification: " + Name + " - Success",
                    message);
            }
        }
    }
}
