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
    public class AlreadyShippedEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AlreadyShipped; }
        }
        
        private string Carrier { get; set; }
        private string TrackingNumber { get; set; }

        private string FullAddress { get; set; }

        private IList<ListingOrderDTO> Items { get; set; }

        /*
         * Dear %FName%,
            Unfortunately your order already shipped and can’t be changed at this time.
            It was shipped to:
            %Address%

            Tracking number:%trucking%

            Best Regards,
            Customer Service

        */


        public string Subject
        {
            get
            {
                return string.Format("Order already shipped (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                        <p>Dear {0},</p>
                        <p>
                            <div>Unfortunately your order already shipped and can’t be changed at this time.</div>
                            <div>It was shipped to:</div>
                            <div>{1}</div>
                        </p>
                        <p>Tracking number: <a href='{3}'>{2}</a></p>
                        <p>Best Regards,<br/>{4}</p></div>",
                    BuyerFirstName,
                    FullAddress,
                    TrackingNumber,
                    MarketUrlHelper.GetTrackingUrl(TrackingNumber, Carrier),
                    Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public AlreadyShippedEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            IList<ListingOrderDTO> items,
            string carrier,
            string trackingNumber,
            AddressDTO address,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            Items = items;
            Carrier = carrier;
            TrackingNumber = trackingNumber;

            FullAddress = AddressHelper.ToStringForLetterWithPersonName(address);

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
