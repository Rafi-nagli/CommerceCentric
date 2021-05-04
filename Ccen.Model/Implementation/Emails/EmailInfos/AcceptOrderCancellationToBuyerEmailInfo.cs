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
    public class AcceptOrderCancellationToBuyerEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AcceptOrderCancellationToBuyer; }
        }
        
        private string ReplyToSubject { get; set; }

        public string Subject
        {
            get
            {
                if (String.IsNullOrEmpty(ReplyToSubject))
                    return String.Format("Order cancelled (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
                return EmailHelper.PrepareReplySubject(ReplyToSubject);
            }
        }

        public string Body
        {
            get
            {
                switch (Market)
                {
                    case MarketType.Walmart:
                        return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                    <p>Dear {0},</p>
                                    <p>We have cancelled your order per your request. We hope to see you again in our store soon at <a href='https://www.walmart.com/search/?facet=retailer:Premium+Apparel'>https://www.walmart.com/search/?facet=retailer:Premium+Apparel</a></p>
                                    <br/> 
                                    <p>Best Regards,<br/>{1}</p></div>",
                            BuyerFirstName,
                            Signature);
                    case MarketType.WalmartCA:
                        return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                    <p>Dear {0},</p>
                                    <p>We have cancelled your order per your request. We hope to see you again in our store soon at <a href='https://www.walmart.ca/search/?facet=retailer:Premium+Apparel'>https://www.walmart.ca/search/?facet=retailer:Premium+Apparel</a></p>
                                    <br/> 
                                    <p>Best Regards,<br/>{1}</p></div>",
                            BuyerFirstName,
                            Signature);
                    default:
                        return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},</p>
                                <p>We have cancelled your order per your request. We hope to see you again in our store soon at <a href='www.amazon.com/shops/premiumapparel'>www.amazon.com/shops/premiumapparel</a></p>
                                <br/> 
                                <p>Best Regards,<br/>{1}</p></div>",
                            BuyerFirstName,
                            Signature);
                }
            }
        }

        public AcceptOrderCancellationToBuyerEmailInfo(IAddressService addressService,
            string byName,
            string orderNumber,
            MarketType market,
            string buyerName,
            string buyerEmail,
            string replyToSubject) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            ReplyToSubject = replyToSubject;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
