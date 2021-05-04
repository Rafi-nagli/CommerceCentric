using System.Linq;
using System.Net;
using System.Net.Mail;
using Amazon.Core.Contracts;

namespace Amazon.Common.Emails
{
    public class SmtpAccessor
    {
        private readonly string host;
        private readonly int port;

        private readonly string username;
        private readonly string password;
        private ILogService _log;

        public SmtpAccessor(ILogService log,
            string host, 
            int port, 
            string username, 
            string password)
        {
            this._log = log;
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
        }

        public void SendEmail(MailMessage messageToSend)
        {
            var fromAddress = messageToSend.From;
            var toCollection = messageToSend.To;
            var ccCollection = messageToSend.CC;
            var bccCollection = messageToSend.Bcc;
            var toAddress = toCollection.First();
            var subject = messageToSend.Subject;
            var body = messageToSend.Body;
            var headers = messageToSend.Headers;

            using (var smtp = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body
                })
                {
                    foreach (string key in headers)
                    {
                        if (message.Headers[key] == null)
                        {
                            message.Headers.Add(key, headers[key]);
                        }
                    }

                    toCollection.Remove(toCollection.First());
                    foreach (var mailAddress in toCollection)
                    {
                        message.To.Add(mailAddress);
                    }
                    foreach (var mailAddress in ccCollection)
                    {
                        message.CC.Add(mailAddress);
                    }
                    foreach (var mailAddress in bccCollection)
                    {
                        message.Bcc.Add(mailAddress);
                    }
                    foreach (var attach in messageToSend.Attachments)
                    {
                        message.Attachments.Add(attach);
                    }
                    smtp.Send(message);
                    _log.Info("Email was sent, subject=" + message.Subject + ", to=" + toAddress.Address);
                }
            }
        }
    }
}
