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
    public class RejectOrderCancellationToBuyerEmailInfo : BaseEmailInfo,  IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.RejectOrderCancellationToBuyer; }
        }
        
        public string ReplyToSubject { get; set; }
        public string Subject
        {
            get
            {
                if (!String.IsNullOrEmpty(ReplyToSubject))
                    return EmailHelper.PrepareReplySubject(ReplyToSubject);
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                    return String.Format("Order cancellation request from Amazon customer (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
                return String.Format("Order cancellation request (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},</p>
                                <p>Your order already shipped and can't be cancelled at this time.</p>
                                <br/> 
                                <p>Best Regards,<br/>{1}</p></div>", 
                                BuyerFirstName,
                                Signature);
            }
        }

        public RejectOrderCancellationToBuyerEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            string buyerName,
            string buyerEmail,
            string replyToSubject,
            string replyToEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;

            ReplyToSubject = replyToSubject;

            if (String.IsNullOrEmpty(ToEmail))
                ToEmail = replyToEmail;
        }
    }
}
