using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models
{
    public class AutoResonseAmazonEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AutoResponseAmazon; }
        }

        /*Dear %Name%,
We are investigating your inquiry and will get back to you shortly. Thank you for your patience and understanding.

Best Regards,
Customer Service
        */

        public string ReplyToSubject { get; set; }
        public string Subject
        {
            get
            {
                return EmailHelper.PrepareReplySubject(ReplyToSubject);
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},</p>
                                <p>We are investigating your inquiry and will get back to you shortly. Thank you for your patience and understanding.                                </p>
                                <p>Best Regards,<br/>{1}</p>
                            </div>",
                    BuyerFirstName,
                    Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public AutoResonseAmazonEmailInfo(IAddressService addressService, 
            string buyerName,
            string replyToSubject,
            string replyToEmail) : base(addressService)
        {
            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = replyToEmail;

            ReplyToSubject = replyToSubject;
        }
    }
}
