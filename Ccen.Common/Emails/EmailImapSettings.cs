using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;

namespace Amazon.Model.Implementation
{
    public class EmailImapSettings : IEmailImapSettings
    {
        public string ImapHost { get; set; }
        public int ImapPort { get; set; }
        public string ImapUsername { get; set; }
        public string ImapPassword { get; set; }

        public IList<string> AcceptingToAddresses { get; set; }

        public bool IsDebug { get; set; }

        public TimeSpan ProcessMessageThreadTimeout { get; set; }
        public int MaxProcessMessageErrorsCount { get; set; }

        public string AttachmentFolderPath { get; set; }
        public string AttachmentFolderRelativeUrl { get; set; }
    }
}
