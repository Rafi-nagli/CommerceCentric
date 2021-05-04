using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Notifications.SupportNotifications;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Notifications
{
    public class CheckSupportNotificationsThread : ThreadBase
    {
        public CheckSupportNotificationsThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("CheckSupportNotifications", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            //Checking email service, sent test message
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var supportNotificatons = new List<ISupportNotification>()
            {
                new ListingCreationIssuesNotification(dbFactory, emailService, log, time),
                new QtyDistributionSupportNotification(dbFactory, emailService, log, time),
                new SubstractQtySupportNotification(dbFactory, emailService, log, time),
                new UnprocessedRefundSupportNotification(dbFactory, emailService, log, time)
            };

            log.Info("Notification count: " + supportNotificatons.Count());

            var currentDate = time.GetAppNowTime();
            var nowTime = new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second);
            var notificationToLaunch = supportNotificatons.Where(n => n.When.Any(w => Math.Abs((w - nowTime).TotalSeconds) < 30)).ToList();
            if (notificationToLaunch.Count > 0)
            {
                log.Info("Notification count: " + notificationToLaunch.Count());

                foreach (var notification in notificationToLaunch)
                {
                    try
                    {
                        log.Info("Begin check " + notification.Name);
                        notification.Check();
                        log.Info("End check " + notification.Name);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Notification error: " + notification.Name, ex);
                    }
                }
            }
        }
    }
}