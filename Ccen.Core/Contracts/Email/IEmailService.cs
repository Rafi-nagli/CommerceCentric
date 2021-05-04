using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts
{
    public interface IEmailService
    {
        IAddressService AddressService { get; }

        string GetMainNotifierStr();

        string GetMainCoWorkerNotifierStr();

        string GetWarehouseNotifierStr();

        string GetSupportNotifierStr();
        

        CallResult<Exception> SendSystemEmail(string subject,
            string body,
            string to,
            string cc,
            string bcc = "");

        CallResult<Exception> SendSystemEmailWithStreamAttachment(string subject,
            string body,
            Stream fileStream,
            string fileName,
            string to,
            string cc,
            string bcc = "");

        CallResult<Exception> SendSystemEmailWithAttachments(string subject,
            string body,
            string[] attachmentFilenames,
            string to,
            string cc,
            string bcc = "");

        CallResult<Exception> SendSystemEmailWithAttachments(string subject,
                string body,
                Attachment[] attachments,
                string to,
                string cc,
                string bcc = "");

        CallResult<Exception> SendSystemEmailToAdmin(string subject,
            string body);

        CallResult<Exception> SendSmsEmail(string message,
            string to);


        CallResult<Exception> SendEmailStatusNotification(IList<EmailOrderDTO> emails);

        CallResult<Exception> SendEmail(IEmailInfo emailInfo, CallSource callSource, long? by);
        CallResult<Exception> SendEmail(IEmailInfo emailInfo, CallSource callSource);
        //void SendEmail(MailMessage message);
        MailMessage ComposeEmail(IEmailInfo model, CallSource callSource, long? by);

        IEmailInfo GetEmailInfoByType(IUnitOfWork db,
            EmailTypes emailType,
            CompanyDTO company,
            string byName,
            string orderId,
            Dictionary<string, string> args,
            string replyToSubject,
            string replyToEmail);
    }
}
