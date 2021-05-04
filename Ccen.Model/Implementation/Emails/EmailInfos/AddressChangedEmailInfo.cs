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
    public class AddressChangedEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AddressChanged; }
        }
        
        public string Subject
        {
            get
            {
                return String.Format("Address changed (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get { return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                 <p>Dear {0},</p>
                                 <p>Thank you for providing new address. We have updated it in the system for this order, and will mail it shortly. Please disregard other emails which may still show an old address.</p>
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

        public AddressChangedEmailInfo(IAddressService addressService, 
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
