using System;
using System.Collections.Generic;
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
    public class CheckSameDay : TimerThreadBase
    {
        public CheckSameDay(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("CheckSameDay", companyId, messageService, callTimeStamps, time)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var settings = new SettingsService(dbFactory);

            var now = time.GetAppNowTime();
            if (!time.IsBusinessDay(now))
                return;

            log.Info("Checking Same Day");

            CompanyDTO company = null;
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);
                        
            var checker = new AlertChecker(log, time, dbFactory, emailService, company);
            using (var db = dbFactory.GetRWDb())
            {
                checker.CheckSameDay(db);
                settings.SetSameDayLastCheck(time.GetUtcTime());
            }
        }
    }
}
