using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Model.Models
{
    public class GiftEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.GiftReceipt; }
        }
        
        /*
         * “Dear %Name%,
Thank you for being our loyal customer. We will ship your order shortly, and will only include gift receipt per your request, which shows your name as a buyer so the recipient would know whom to thank for this beautiful gift.
 
%standard signature%
”


        */


        public string Subject
        {
            get
            {
                return string.Format("Gift (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                        <p>Dear {0},</p>
                        <p>Thank you for being our loyal customer. We will ship your order shortly, and will only include gift receipt per your request, which shows your name as a buyer so the recipient would know whom to thank for this beautiful gift.</p>
                        <p>Best Regards,<br/>{1}</p></div>",
                    BuyerFirstName,
                    Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public GiftEmailInfo(IAddressService addressService, 
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
