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
    public class AutoResonseWalmartEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AutoResponseWalmart; }
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
                /*Dear XXX,
We received a message from Walmart that you have a question/comment regarding your order %OrderNumber%, %Pajamas%.
Please kindly let us know how can we help you
*/
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},</p>
                                <p>We received a message from Walmart that you have a question/comment regarding your order {1},<br/>{2}<br/>
                                    Please kindly let us know how can we help you</p>                                
                                <p>Best Regards,<br/>{3}</p>
                            </div>",
                    BuyerFirstName,
                    Tag,
                    EmailInfoHelper.GetProductString(Items, "<br/>"),
                    Signature);
            }
        }

        private IList<ListingOrderDTO> Items { get; set; }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public AutoResonseWalmartEmailInfo(IAddressService addressService, 
            string orderNumber,
            IList<ListingOrderDTO> items,
            MarketType market,
            string buyerName,
            string buyerEmail,
            string replyToSubject,
            string replyToEmail) : base(addressService)
        {
            Tag = orderNumber;
            Items = items;
            Market = market;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;

            ReplyToSubject = replyToSubject;
        }
    }
}
