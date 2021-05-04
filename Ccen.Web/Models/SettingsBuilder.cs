using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Ccen.Web;

namespace Amazon.Web.Models
{
    public class SettingsBuilder
    {
        //static public IEmailSmtpSettings GetSmtpSettingsFromAppSettings()
        //{
        //    var settings = new EmailSmtpSettings();
        //    settings.SmtpHost = AppSettings.SMTP_ServerHost;
        //    settings.SmtpPort = Int32.Parse(AppSettings.SMTP_ServerPort);
        //    settings.SmtpUsername = AppSettings.SMTP_EmailUsername;
        //    settings.SmtpPassword = AppSettings.SMTP_EmailPassword;

        //    settings.SmtpFromEmail = AppSettings.SMTP_EmailUsername;
        //    settings.SmtpFromDisplayName = AppSettings.SMTP_DisplayFromEmail;

        //    settings.IsDebug = AppSettings.IsDebug;

        //    return settings;
        //}

        static public IEmailSmtpSettings GetSmtpSettingsFromCompany(CompanyDTO company)
        {
            var settings = new EmailSmtpSettings();

            if (company == null)
                return settings;

            var smtpSettings = company.EmailAccounts.FirstOrDefault(e => e.Type == (int) EmailAccountType.Smtp);
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