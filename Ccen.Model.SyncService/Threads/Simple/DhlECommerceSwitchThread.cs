using System;
using System.Collections.Generic;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class DhlECommerceSwitchThread : TimerThreadBase
    {
        public DhlECommerceSwitchThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("DhlECommerceSwitch", companyId, messageService, callTimeStamps, time)
        {
            
        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var addressService = AddressService.Default;

            var now = time.GetAppNowTime();
            if (!time.IsBusinessDay(now))
                return;

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);
            
            var weightService = new WeightService();
            var messageService = new SystemMessageService(log, time, dbFactory);

            var dhlEcommerceSwitchService = new DhlECommerceSwitchService(log,
                time,
                company,
                dbFactory,
                emailService,
                weightService,
                messageService);
            dhlEcommerceSwitchService.SwitchToECommerce();
        }
    }
}
