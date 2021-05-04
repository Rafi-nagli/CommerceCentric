using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class SystemEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.System; }
        }
        
        public string Subject { get; set; }

        public string Body { get; set; }

        public SystemEmailInfo(IAddressService addressService, 
            string subject,
            string toName,
            string toEmail) : base(addressService)
        {
            Subject = subject;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(toName);
            ToEmail = toEmail;
        }
    }
}
