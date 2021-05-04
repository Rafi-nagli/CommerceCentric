using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;

namespace Amazon.Model.Implementation
{
    public class EmailSmtpSettings : IEmailSmtpSettings
    {
        public string SystemEmailPrefix { get; set; }

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpFromEmail { get; set; }
        public string SmtpFromDisplayName { get; set; }

        public bool IsDebug { get; set; }
        public bool IsSampleMode { get; set; }
    }
}
