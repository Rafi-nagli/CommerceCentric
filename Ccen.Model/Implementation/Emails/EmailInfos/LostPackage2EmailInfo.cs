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
    public class LostPackage2EmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.LostPackage2; }
        }
        
        /*Dear %Name%,
We are sorry there is an issue with your order. The reimbursement of lost/misdelivered orders is handled by Amazon directly through A-Z claims.
Please kindly file a claim by following these steps:
https://www.amazon.com/gp/help/customer/display.html?nodeId=200783750

Bets Regards, 
Customer Service

        */

        public string Subject
        {
            get
            {
                return string.Format("Lost Package (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},</p>
                                <p>We are sorry there is an issue with your order. The reimbursement of lost/misdelivered orders is handled by Amazon directly through A-Z claims.<br/>
                                   Please kindly file a claim by following these steps:<br/>
                                   <a href='https://www.amazon.com/gp/help/customer/display.html?nodeId=200783750'>https://www.amazon.com/gp/help/customer/display.html?nodeId=200783750</a>
                                </p>
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

        public LostPackage2EmailInfo(IAddressService addressService, 
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
