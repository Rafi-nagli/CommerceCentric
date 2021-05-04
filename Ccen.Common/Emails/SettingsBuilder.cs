using System;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;

namespace Amazon.Common.Emails
{
    public class SettingsBuilder
    {
        public static IEmailSmtpSettings GetSmtpSettingsFromCompany(CompanyDTO company,
            bool isDebug,
            bool isSampleMode)
        {
            var emailAccount = company.EmailAccounts.FirstOrDefault(a => a.Type == (int) EmailAccountType.Smtp);

            if (emailAccount == null)
                return null;

            var settings = new EmailSmtpSettings();
            settings.SystemEmailPrefix = company.CompanyName;

            settings.SmtpHost = emailAccount.ServerHost;
            settings.SmtpPort = emailAccount.ServerPort;
            settings.SmtpUsername = emailAccount.UserName;
            settings.SmtpPassword = emailAccount.Password;

            settings.SmtpFromEmail = emailAccount.FromEmail;
            settings.SmtpFromDisplayName = emailAccount.DisplayName;

            settings.IsDebug = isDebug;
            settings.IsSampleMode = isSampleMode;

            return settings;
        }

        public static IEmailImapSettings GetImapSettingsFromCompany(CompanyDTO company,
            int maxProcessMessageErrorsCount,
            int processMessageThreadTimeoutSecond)
        {
            var emailAccount = company.EmailAccounts.FirstOrDefault(a => a.Type == (int)EmailAccountType.Imap);

            if (emailAccount == null)
                return null;

            var settings = new EmailImapSettings();
            settings.ImapHost = emailAccount.ServerHost;
            settings.ImapPort = emailAccount.ServerPort;
            settings.ImapUsername = emailAccount.UserName;
            settings.ImapPassword = emailAccount.Password;

            settings.AcceptingToAddresses = (emailAccount.AcceptingToAddresses ?? "").Split(";,".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            settings.MaxProcessMessageErrorsCount = maxProcessMessageErrorsCount;
            settings.ProcessMessageThreadTimeout = TimeSpan.FromSeconds(processMessageThreadTimeoutSecond);

            settings.AttachmentFolderPath = emailAccount.AttachmentDirectory;
            settings.AttachmentFolderRelativeUrl = emailAccount.AttachmentFolderRelativeUrl;

            return settings;
        }
    }
}
