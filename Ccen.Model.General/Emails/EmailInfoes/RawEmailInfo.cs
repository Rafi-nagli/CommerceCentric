using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class RawEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.RawEmail; }
        }

        private string _subject;
        public string Subject
        {
            get { return _subject; }
        }

        private string _body;
        public string Body
        {
            get
            {
                return _body;
            }
        }

        public RawEmailInfo(IAddressService addressService, 
            string subject,
            string body,
            string[] attachments,
            string toName,
            string toEmail,
            string ccName,
            string ccEmail,
            string bccName ,
            string bccEmail) : base(addressService)
        {
            _subject = subject;
            _body = body;
            if (attachments != null && attachments.Any())
                _attachments = attachments.Select(a => new Attachment(a)).ToArray();

            ToName = StringHelper.MakeEachWordFirstLetterUpper(toName);
            ToEmail = toEmail;

            CcName = StringHelper.MakeEachWordFirstLetterUpper(ccName);
            CcEmail = ccEmail;

            BccName = StringHelper.MakeEachWordFirstLetterUpper(bccName);
            BccEmail = bccEmail;
        }

        public RawEmailInfo(IAddressService addressService,
            string subject,
            string body,
            Attachment[] attachments,
            string toName,
            string toEmail,
            string ccName,
            string ccEmail,
            string bccName,
            string bccEmail) : base(addressService)
        {
            _subject = subject;
            _body = body;

            if (attachments != null && attachments.Any())
                _attachments = attachments;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(toName);
            ToEmail = toEmail;

            CcName = StringHelper.MakeEachWordFirstLetterUpper(ccName);
            CcEmail = ccEmail;

            BccName = StringHelper.MakeEachWordFirstLetterUpper(bccName);
            BccEmail = bccEmail;
        }
    }
}
