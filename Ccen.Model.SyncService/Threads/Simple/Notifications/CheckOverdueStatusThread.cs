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
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Notifications
{
    public class CheckOverdueStatusThread : TimerThreadBase
    {
        public CheckOverdueStatusThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("CheckOverdueStatus", companyId, messageService, callTimeStamps, time)
        {
            
        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var now = time.GetAppNowTime();
            if (!time.IsBusinessDay(now))
                return;

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var autoPurchaseTime = AppSettings.OverdueAutoPurchaseTime;

            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            //Checking email service, sent test message
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var checker = new AlertChecker(log, time, dbFactory, emailService, company);
            using (var db = dbFactory.GetRWDb())
            {
                checker.CheckOverdue(db, now, autoPurchaseTime);
            }
        }
    }
}
