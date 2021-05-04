using System;
using System.Collections.Generic;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Emails;
using Amazon.Model.Implementation.Emails.Rules;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateEmailsThread : ThreadBase
    {
        public UpdateEmailsThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateEmails", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var log = GetLogger();
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }
            var time = new TimeService(dbFactory);
            var addressService = AddressService.Default;
            var emailImapSettings = SettingsBuilder.GetImapSettingsFromCompany(company,
                Int32.Parse(AppSettings.Support_MaxProcessMessageErrorsCount),
                Int32.Parse(AppSettings.Support_ProcessMessageThreadTimeoutSecond));
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailReaderService = new EmailReaderService(
                emailImapSettings,
                GetLogger(),
                dbFactory,
                time);
            var emailService = new EmailService(log, emailSmtpSettings, addressService);
            var systemActions = new SystemActionService(log, time);
            

            var emailProcessingService = new EmailProcessingService(
                log,
                dbFactory,
                emailService,
                systemActions,
                company,
                time);

            //Inbox Folder
            emailReaderService.ReadEmails(EmailHelper.InboxFolder, null, emailImapSettings.AcceptingToAddresses, null, CancellationToken);
            IList<IEmailRule> inboxRules = new List<IEmailRule>()
            {
                new SetSystemTypesEmailRule(log, time),
                new AddMatchIdsEmailRule(log, time),
                new SetAnswerIdEmailRule(log, time), //NOTE: also using in Inbox for processing Amazon autocommunicate emails
                new CancellationEmailRule(log, time, emailService, systemActions, company),
                new RemoveSignatureEmailRule(log, time, emailService, systemActions),
                new FeedbackBlackListEmailRule(log, time),
                new ReturnRequestEmailRule(log, time, emailService, systemActions, company, true, false),
                new OrderDeliveryInquiryEmailRule(log, time, emailService, systemActions),
                new AddressNotChangedEmailRule(log, time, emailService, systemActions),
                new DhlInvoiceEmailRule(log, time, dbFactory),
                new AddCommentEmailRule(log, systemActions, time),
                new PrepareBodyEmailRule(log, time),
            };
            emailProcessingService.ProcessEmails(emailReaderService.EmailProcessResultList, inboxRules);

            //Sent Folder
            emailReaderService.ReadEmails(EmailHelper.SentFolder, null, null, emailImapSettings.AcceptingToAddresses, CancellationToken);
            IList<IEmailRule> sentRules = new List<IEmailRule>()
            {
                new SetSystemTypesEmailRule(log, time),
                new AddMatchIdsEmailRule(log, time),
                new SetAnswerIdEmailRule(log, time),
                new SetSentByEmailRule(log, time)
            };
            emailProcessingService.ProcessEmails(emailReaderService.EmailProcessResultList, sentRules);

            //Update Settings
            var settingService = new SettingsService(dbFactory);
            using (var db = dbFactory.GetRWDb())
            {
                settingService.SetUnansweredMessageCount(db.Emails.GetUnansweredEmailCount());
            }
        }
    }
}
