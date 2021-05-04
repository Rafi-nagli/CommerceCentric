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
    public class UndeliverableAsAddressedRequestEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.UndeliverableAsAddressed; }
        }
        
        private string Carrier { get; set; }
        private string TrackingNumber { get; set; }

        private IList<ListingOrderDTO> Items { get; set; }

        /*
         * “Dear %Name%,
            Your order of %List Of pajamas% being returned to us by USPS because the address you have provided for this order is undeliverable.
            Please review full tracking history of this order at %link to USPS with tracking number%.
            Please let us know how would you like us to proceed with your order once we get it back.

            Best Regards,
            Customer Service”
        */


        public string Subject
        {
            get
            {
                return string.Format("Delivery Exception (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                        <p>Dear {0},</p>
                        <p>
                            <div>Your order of {1} being returned to us by {4} because the address you have provided for this order is undeliverable.</div>
                            <div>Please review full tracking history of this order at <a href='{3}'>{2}</a>.</div>
                            <div>Please let us know how would you like us to proceed with your order once we get it back.</div>
                        </p>
                        <p>Best Regards,<br/>{5}</p></div>",
                    BuyerFirstName,
                    EmailInfoHelper.GetProductString(Items, ", "),
                    TrackingNumber,
                    MarketUrlHelper.GetTrackingUrl(TrackingNumber, Carrier),
                    Carrier,
                    Signature);
            }
        }

        public UndeliverableAsAddressedRequestEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            IList<ListingOrderDTO> items,
            string carrier,
            string trackingNumber,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            Items = items;
            Carrier = carrier;
            TrackingNumber = trackingNumber;
            
            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
