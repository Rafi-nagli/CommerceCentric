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
    public class SignConfirmationRequestEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.SignConfirmationRequest; }
        }
        
        public ShippingTypeCode ServiceType { get; set; }

        private IList<ListingOrderDTO> Items { get; set; }

        /*
         “Dear %Customer%, 
Thank you for ordering %Product name%, and selecting rush delivery. Your order will ship shortly, with fastest USPS service available in your area (usually overnight).
To make sure you order delivered promptly and securely to you, we will send it with Signature Confirmation requirement.
If you would like us to remove Signature Confirmation requirement, so mailman could leave the order in your mailbox or by the front door, please reply to this email with “Remove signature confirmation” in the body of the email (we need to receive that email before your order ships).
PLEASE NOTE: if you request us to remove signature confirmation requirement, you release us from responsibility for the package once it’s marked delivered to you by USPS.

Best Regards,
Premium Apparel

         * */


        public string Subject
        {
            get
            {
                return string.Format("Signature requirement (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                if (ServiceType == ShippingTypeCode.PriorityExpress)
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},</p>
                            <p>
                                <div>Thank you for ordering {1}, and selecting rush delivery. Your order will ship shortly, with fastest USPS service available in your area (usually overnight).</div>
                                <div>To make sure you order delivered promptly and securely to you, we will send it with <b>Signature Confirmation</b> requirement.</div>
                                <div>If you would like us to remove Signature Confirmation requirement, so mailman could leave the order in your mailbox or by the front door, please reply to this email with “Remove signature confirmation” in the body of the email (we need to receive that email before your order ships).</div>
                            </p>
                            <p>PLEASE NOTE: if you request us to remove signature confirmation requirement, you release us from responsibility for the package once it’s marked delivered to you by USPS.
                            </p>
                            <p>Best Regards,<br/>Premium Apparel</p></div>",
                        BuyerFirstName,
                        EmailInfoHelper.GetProductString(Items, ", "));
                }
                else
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},</p>
                            <p>
                                <div>Thank you for ordering {1}.</div>
                                <div>To make sure you order delivered promptly and securely to you, we will send it with <b>Signature Confirmation</b> requirement.</div>
                                <div>If you would like us to remove Signature Confirmation requirement, so mailman could leave the order in your mailbox or by the front door, please reply to this email with “Remove signature confirmation” in the body of the email (we need to receive that email before your order ships).</div>
                            </p>
                            <p>PLEASE NOTE: if you request us to remove signature confirmation requirement, you release us from responsibility for the package once it’s marked delivered to you by USPS.
                            </p>
                            <p>Best Regards,<br/>{2}</p></div>",
                        BuyerFirstName,
                        EmailInfoHelper.GetProductString(Items, ", "),
                        Signature);
                }
            }
        }

        public SignConfirmationRequestEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            IList<ListingOrderDTO> items,
            ShippingTypeCode serviceType,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            ServiceType = serviceType;
            Items = items;
            
            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
