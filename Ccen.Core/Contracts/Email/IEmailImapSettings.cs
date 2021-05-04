using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IEmailImapSettings
    {
        string ImapHost { get; set; }
        int ImapPort { get; set; }
        string ImapUsername { get; set; }
        string ImapPassword { get; set; }

        IList<string> AcceptingToAddresses { get; set; }

        bool IsDebug { get; set; }

        TimeSpan ProcessMessageThreadTimeout { get; set; }
        int MaxProcessMessageErrorsCount { get; set; }

        string AttachmentFolderPath { get; set; }
        string AttachmentFolderRelativeUrl { get; set; }
    }
}
