using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class SmsEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.SmsEmail; }
        }

        private string _subject;
        public string Subject
        {
            get { return _subject; }
        }

        private string _body;
        public string Body
        {
            get { return _body; }
        }

        public SmsEmailInfo(IAddressService addressService, 
            string text,
            string toEmail,
            string toName) : base(addressService)
        {
            Tag = "SmsEmail";
            _subject = text;
            _body = text;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(toName);
            ToEmail = toEmail;
        }
    }
}
