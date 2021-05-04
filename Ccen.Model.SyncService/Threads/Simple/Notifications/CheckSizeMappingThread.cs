using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Notifications
{
    public class CheckSizeMappingThread : TimerThreadBase
    {
        public CheckSizeMappingThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("CheckSizeMapping", companyId, messageService, callTimeStamps, time)
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
            var settings = new SettingsService(dbFactory);

            //Checking email service, sent test message
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var lastSendDate = settings.GetSendDhlInvoiceNotification();

            var sizeMappingService = new SizeMappingService(log, time, dbFactory);
            var newIssues = sizeMappingService.CheckItemsSizeMappingIssue();
            if (newIssues.Any())
            {
                var body = String.Join("<br/>",
                    newIssues.Select(i => i.ASIN + ", marketSize: " + i.Size + ", styleSize: " + i.StyleSize));
                emailService.SendSystemEmailToAdmin("New size mapping issues, count: " + newIssues.Count,
                    body);

                log.Info("New issues: " + body);
            }
            else
            {
                log.Info("No new issues");
            }

            if (time.GetAppNowTime().DayOfWeek == DayOfWeek.Saturday)
            {
                var issueSummary = sizeMappingService.GetItemsSummaryWithSizeMappingIssue();
                var body = String.Join("<br/>",
                        issueSummary.Select(i => i.ASIN + ", marketSize: " + i.Size + ", styleSize: " + i.StyleSize));
                emailService.SendSystemEmailToAdmin("Size mapping issue summary, count: " + issueSummary.Count,
                    body);

                log.Info("Summary issues: " + body);
            }
        }
    }
}
