using System;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Models.EmailInfos;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class SendEmailsThread : ThreadBase
    {
        public SendEmailsThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("SendEmails", companyId, messageService, callbackInterval)
        {

        }

        protected override void Init()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }
            var addressService = AddressService.Default;

            //Checking email service, sent test message
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var info = new SystemEmailInfo(addressService, "[commercentric.com] Launched emails thread", "Support", "support@dgtex.com");
            emailService.SendEmail(info, CallSource.Service);
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            CompanyDTO company = null;
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var time = new TimeService(dbFactory);
            var addressService = AddressService.Default;

            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);

            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);
            var actionService = new SystemActionService(GetLogger(), time);

            emailService.ProcessEmailActions(dbFactory,
                time,
                company,
                actionService);

            emailService.AutoAnswerEmails(dbFactory,
                time,
                company);
        }
    }
}
