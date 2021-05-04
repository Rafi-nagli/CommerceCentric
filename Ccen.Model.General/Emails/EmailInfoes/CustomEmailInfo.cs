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
    public class CustomEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.CustomEmail; }
        }
        
        public string Subject
        {
            get { return String.Format("Request information (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market)); }
        }


        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},</p>
                                <p></p>
                                <br/> 
                                <p>Best Regards,<br/>{1}</p></div>",
                                BuyerFirstName,
                                Signature);
            }
        }

        public CustomEmailInfo(IAddressService addressService,
            string byName, 
            string orderNumber,
            MarketType market,
            string buyerName,
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
