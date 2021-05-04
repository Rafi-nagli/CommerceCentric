using System;
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
    public class UndeliverableInquiryEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.UndeliverableInquiry; }
        }
        
        private string Carrier { get; set; }
        public string TrackingNumber { get; set; }

        public string Subject
        {
            get
            {
                return string.Format("Delivery Exception (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        /*
“
Dear %FName%,
Your order couldn't be delivered, because address you have provided was incorrect, please review tracking history at:
%tracking link%

the order is on the way back to us, please let us know what would you like us to do when we get it back. We can either resend it to a different/corrected address or issue a refund.

Best Regards,
Customer Service
”

 */

        public string Body
        {
            get
            {
                return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                        <p>Dear {0},</p>
                        <p>
                            <div>Your order couldn't be delivered, because address you have provided was incorrect, please review tracking history at:</div>
                            <div><a href='{2}'>{1}</a></div>                            
                        </p>
                        <p>
                            <div>the order is on the way back to us, please let us know what would you like us to do when we get it back. We can either resend it to a different/corrected address or issue a refund.</div>
                        </p>
                        <p>Best Regards,<br/>{3}</p></div>",
                    BuyerFirstName,
                    TrackingNumber,
                    MarketUrlHelper.GetTrackingUrl(TrackingNumber, Carrier),
                    Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public UndeliverableInquiryEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            string carrier,
            string trackingNumber,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            TrackingNumber = trackingNumber;
            Carrier = carrier;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
