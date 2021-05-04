using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IEmailSmtpSettings
    {
        string SystemEmailPrefix { get; set; }

        string SmtpHost { get; set; }
        int SmtpPort { get; set; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }

        string SmtpFromEmail { get; set; }
        string SmtpFromDisplayName { get; set; }

        bool IsSampleMode { get; set; }
        bool IsDebug { get; set; }
    }
}
