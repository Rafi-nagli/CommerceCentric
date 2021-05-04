
using System.Collections.Generic;
using System.Net.Mail;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;

namespace Amazon.Core.Contracts
{
    public interface IEmailInfo
    {
        EmailTypes EmailType { get; }

        string Tag { get; }
        MarketType Market { get; set; }

        MailAddress From { get; set; }

        List<MailAddress> ToList { get; }
        List<MailAddress> CcList { get;  }
        List<MailAddress> BccList { get; }

        IList<Attachment> Attachments { get; }

        string Subject { get; }
        string Body { get; }
    }
}
