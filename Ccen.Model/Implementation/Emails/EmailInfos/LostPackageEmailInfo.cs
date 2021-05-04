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
    public class LostPackageEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.LostPackage; }
        }
        
        private DateTime? DeliveryDate { get; set; }
        private string TrackingStateEvent { get; set; }

        private string Carrier { get; set; }
        private string TrackingNumber { get; set; }
        private string FullAddress { get; set; }

        private string TrackingStateFormatted
        {
            get
            {
                if (String.IsNullOrEmpty(TrackingStateEvent)
                    || !TrackingStateEvent.ToLower().Contains("delivered")
                    || TrackingStateEvent.ToLower() == "delivered")
                    return "<b>delivered</b>";

                return "delivered to <b>" + TrackingStateEvent.Replace("Delivered, ", "") + "</b>";
            }
        }

        /*
        Dear Mr./Mrs. %Name%,
I am sorry to hear you have not received your package. If we cannot track your package down we will attempt to replace it. According to tracking history package was placed into your mailbox on %DATE%:
%USPS tracking URL%
We request that you check some things on your end first:
Please verify this is your current mailing address.
%NAME
ADDRESS
ADDRESS%
 
Please check with other members of your household to see if anyone may have put your package aside. This happens a lot! I’ve done it to myself! ;)
If you live in an apartment complex, please contact your rental office to see if they are holding your package there. Some packages won’t fit in your mailbox so carriers will often leave packages at a manager’s office for safekeeping.
Please call your local post office with the tracking number (%tracking%) and ask them if they can assist you further. Usually they have more information than what we can see online. They may even have your package on hold for you. If they have a postmaster at your local post office, ask to speak with him/her first.
In the meantime, I will file a loss/theft report with the US Postal Inspector. They can be very helpful in finding missing packages within the postal system due to theft/fraud/misdelivery. If they are unable to help us, I will attempt to replace it.
 
Thank you for your patience!
Premium Apparel 
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
                var carrier = Carrier;
                if (!StringHelper.IsEqualNoCase(carrier, ShippingServiceUtils.FedexCarrier)
                    && !StringHelper.IsEqualNoCase(carrier, ShippingServiceUtils.UPSCarrier))
                {
                    carrier = "your local post office";
                }
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear Mr./Mrs. {0},</p>
                            <p>
                                I am sorry to hear you have not received your package. According to tracking history package was {2} on {1}:<br/>
                                <a href='{3}'>{4}</a>
                            </p>
                            <p>
                                We request that you check some things on your end first:<br/>
                                Please verify this is your current mailing address.<br/>
                                {5}
                            </p>
<p>Please check with other members of your household to see if anyone may have put your package aside. This happens a lot! I’ve done it to myself! ;)<br/>
If you live in an apartment complex, please contact your rental office to see if they are holding your package there. Some packages won’t fit in your mailbox so carriers will often leave packages at a manager’s office for safekeeping.<br/>
Please call {7} with the tracking number ({4}) and ask them if they can assist you further. Usually they have more information than what we can see online. They may even have your package on hold for you. If they have a postmaster at your local post office, ask to speak with him/her first.
                            </p>
                            <p>Thank you for your patience!<br/>{6}</p></div>",
                    BuyerLastName,
                    DeliveryDate.HasValue ? DeliveryDate.Value.ToString("MMM dd") : " ",
                    TrackingStateFormatted,
                    MarketUrlHelper.GetTrackingUrl(TrackingNumber, Carrier),
                    TrackingNumber,
                    FullAddress,
                    Signature,
                    Carrier);
            }
        }

        public LostPackageEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            DateTime? deliveryDate,
            string trackingStateEvent,
            string carrier,
            string trackingNumber,
            AddressDTO address,
            string buyerName,
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            DeliveryDate = deliveryDate;
            TrackingStateEvent = trackingStateEvent;

            Carrier = carrier;
            TrackingNumber = trackingNumber;
            FullAddress = AddressHelper.ToStringForLetterWithPersonName(address);

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
