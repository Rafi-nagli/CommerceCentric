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
    public class ReturnWrongDamagedItemEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.ReturnWrongDamagedItem; }
        }

        /*
         “              
Dear %Name%,
We are very sorry for the issue with your order. Please find attached prepaid return label, which we ask you to use to send us back the item you have received.
The label is only valid for 5 business days, please send it shortly. The item you are returning should have all the tags attached.
 
Best Regards,
Premium Apparel
”
        */

        public string Subject
        {
            get
            {
                return string.Format("Return Item(s) (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},<br/>
                                We are very sorry for the issue with your order. Please find attached prepaid return label, which we ask you to use to send us back the item you have received.<br/>
                                The label is only valid for 5 business days, please send it shortly. The item you are returning should have all the tags attached.
                            </p>
                            <p>Best Regards,<br/>{1}</p>
                        </div>",
                    BuyerFirstName,
                    Signature);
            }
        }

        public ReturnWrongDamagedItemEmailInfo(IAddressService addressService, 
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
