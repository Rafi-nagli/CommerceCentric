using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models
{
    public class AddressNotChangedEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AddressNotChanged; }
        }
        
        public string ReplyToSubject { get; set; }

        public string Subject
        {
            get
            {
                if (String.IsNullOrEmpty(ReplyToSubject))
                    return String.Format("Address can't be changed (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
                else
                    return EmailHelper.PrepareReplySubject(ReplyToSubject);
            }
        }

        public string Body
        {
            get { return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                 <p>Dear {0},</p>
                                 <p>Unfortunately you order already shipped and address can’t be changed at this time.</p>
                                 <br/>
                                 <p>Best Regards,<br/>{1}</p></div>", 
                        BuyerFirstName,
                        Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public AddressNotChangedEmailInfo(IAddressService addressService, 
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
