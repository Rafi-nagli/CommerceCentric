using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;

namespace Amazon.InventoryUpdateManual.Models
{
    public class SettingsBuilder
    {
        static public IEmailSmtpSettings GetSmtpSettingsFromAppSettings()
        {
            var settings = new EmailSmtpSettings();
            settings.SmtpHost = AppSettings.SMTP_ServerHost;
            settings.SmtpPort = Int32.Parse(AppSettings.SMTP_ServerPort);
            settings.SmtpUsername = AppSettings.SMTP_EmailUsername;
            settings.SmtpPassword = AppSettings.SMTP_EmailPassword;

            settings.SmtpFromEmail = AppSettings.SMTP_EmailUsername;
            settings.SmtpFromDisplayName = AppSettings.SMTP_DisplayFromEmail;

            settings.IsDebug = AppSettings.IsDebug;

            return settings;
        }

        static public IEmailImapSettings GetImapSettingsFromAppSettings()
        {
            var settings = new EmailImapSettings();
            settings.ImapHost = AppSettings.Support_ServerHost;
            settings.ImapPort = Int32.Parse(AppSettings.Support_ServerPort);
            settings.ImapUsername = AppSettings.Support_EmailUsername;
            settings.ImapPassword = AppSettings.Support_EmailPassword;

            settings.MaxProcessMessageErrorsCount = Int32.Parse(AppSettings.Support_MaxProcessMessageErrorsCount);
            settings.ProcessMessageThreadTimeout = TimeSpan.FromSeconds(Int32.Parse(AppSettings.Support_ProcessMessageThreadTimeoutSecond));

            settings.AttachmentFolderPath = AppSettings.Support_AttachmentDirectory;
            settings.AttachmentFolderRelativeUrl = AppSettings.Support_AttachmentFolderRelativeUrl;

            settings.IsDebug = AppSettings.IsDebug;

            return settings;
        }

        static public IEmailImapSettings GetImapSettingsFromCompany(CompanyDTO company)
        {
            var settings = new EmailImapSettings();

            if (company == null)
                return settings;

            var smtpSettings = company.EmailAccounts.FirstOrDefault(e => e.Type == (int)EmailAccountType.Imap);
            if (smtpSettings == null)
                return settings;

            settings.ImapHost = smtpSettings.ServerHost;
            settings.ImapPort = smtpSettings.ServerPort;
            settings.ImapUsername = smtpSettings.UserName;
            settings.ImapPassword = smtpSettings.Password;

            settings.AcceptingToAddresses = (smtpSettings.AcceptingToAddresses ?? "").Split(";,".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            settings.MaxProcessMessageErrorsCount = Int32.Parse(AppSettings.Support_MaxProcessMessageErrorsCount);
            settings.ProcessMessageThreadTimeout = TimeSpan.FromSeconds(Int32.Parse(AppSettings.Support_ProcessMessageThreadTimeoutSecond));

            settings.AttachmentFolderPath = smtpSettings.AttachmentDirectory;
            settings.AttachmentFolderRelativeUrl = smtpSettings.AttachmentFolderRelativeUrl;

            settings.IsDebug = AppSettings.IsDebug;

            return settings;
        }

        static public IEmailSmtpSettings GetSmtpSettingsFromCompany(CompanyDTO company)
        {
            var settings = new EmailSmtpSettings();

            if (company == null)
                return settings;

            var smtpSettings = company.EmailAccounts.FirstOrDefault(e => e.Type == (int)EmailAccountType.Smtp);
            if (smtpSettings == null)
                return settings;

            settings.SmtpHost = smtpSettings.ServerHost;
            settings.SmtpPort = smtpSettings.ServerPort;
            settings.SmtpUsername = smtpSettings.UserName;
            settings.SmtpPassword = smtpSettings.Password;

            settings.SmtpFromEmail = smtpSettings.FromEmail;
            settings.SmtpFromDisplayName = smtpSettings.DisplayName;

            settings.IsDebug = AppSettings.IsDebug;

            return settings;
        }
    }
}
