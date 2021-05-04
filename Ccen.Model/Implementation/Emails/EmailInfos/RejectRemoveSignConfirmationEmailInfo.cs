using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class RejectRemoveSignConfirmationEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.RejectRemoveSignConfirmation;  }
        }
        
        public string Subject
        {
            get { return String.Format("Signature requirement cannot removed (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market)); }
        }

        /*“Dear %Client Name%,
Unfortunately your order already shipped and we cannot remove signature confirmation, if postman misses you, please consider:
* schedule redelivery at time which is convenient for you at https://redelivery.usps.com/redelivery/ , 
* pick up the package from post office
* or follow directions on the back of the notice, which may allow you to forward it to different address

Best Regards,
Customer Service”
*/
        public string Body
        {
            get
            {
                return String.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                        <p>Dear {0},</p>
                                        <p><div>Unfortunately your order already shipped and we cannot remove signature confirmation, if postman misses you, please consider:</div>
                                            <ul>
                                            <li>schedule redelivery at time which is convenient for you at <a href='https://redelivery.usps.com/redelivery/'>https://redelivery.usps.com/redelivery/</a></li>
                                            <li>pick up the package from post office</li>
                                            <li>or follow directions on the back of the notice, which may allow you to forward it to different address</li>
                                            </ul>
                                        </p>
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

        public RejectRemoveSignConfirmationEmailInfo(IAddressService addressService, 
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
