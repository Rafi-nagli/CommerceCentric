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
    public class NoticeLeftEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.NoticeLeft; }
        }
        
        public DateTime? NoticeLeftDate { get; set; }

        public string Currier { get; set; }

        public string TrackingNumber { get; set; }

        public ShippingTypeCode ServiceType { get; set; }

        private string FullAddress { get; set; }

        public string ShippingCountry { get; set; }

        private IList<ListingOrderDTO> Items { get; set; }

        /*
“Dear XXX,
USPS attempted to deliver your order of:
%list_of_Pajamas%
on %date%, but nobody was available to accept the package and they left a notice.
You can try to reschedule delivery at https://redelivery.usps.com/redelivery/  by providing tracking number %tracking_number% or proceed with instructions on the back of the notice card. Orders which aren’t picked up within 7 days usually sent back and may encounter additional fees.
Please don’t hesitate to contact us if you have any questions.
 
Best Regards,
Customer Service”
         */


        public string Subject
        {
            get
            {
                return string.Format("[Important] Delivery Exception (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                if (!ShippingUtils.IsInternational(ShippingCountry))
                {
                    if (ServiceType == ShippingTypeCode.Standard)
                    {
                        return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Dear {0},</p>
                            <p>
                                <div>USPS attempted to deliver your order of:</div>
                                <div>{1}</div>
                                <div>on {2},</div>
                                <div>to: {3}</div>
                                <div>but nobody was available to accept the package and they left a notice.</div>
                            </p>
                            <p>
                                <div>Please contact USPS to pick up your order or proceed with instructions on the back of the notice card. Orders which aren’t picked up within 7 days usually sent back and may encounter additional fees.</div>
                                <div>Your tracking number is {4}. You can contact USPS customer service at 1 (800) 275-8777</div>
                            </p>
                            <p>
                                <div>Please don’t hesitate to contact us if you have any questions.</div>
                            </p>
                            <p>Best Regards,<br/>
                            Customer Service</p></div>",
                            BuyerFirstName,
                            EmailInfoHelper.GetProductString(Items, "<br/>"),
                            EmailInfoHelper.GetDateString(NoticeLeftDate),
                            FullAddress,
                            TrackingNumber);
                    }

                    return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Dear {0},</p>
                            <p>
                                <div>USPS attempted to deliver your order of:</div>
                                <div>{1}</div>
                                <div>on {2},</div>
                                <div>to: {3}</div>
                                <div>but nobody was available to accept the package and they left a notice.</div>
                            </p>
                            <p>
                                <div>You can try to reschedule delivery at <a href='https://redelivery.usps.com/redelivery/'>https://redelivery.usps.com/redelivery/</a> by providing tracking number {4} or proceed with instructions on the back of the notice card. Orders which aren’t picked up within 7 days usually sent back and may encounter additional fees.</div>
                            </p>
                            <p>
                                <div>Please don’t hesitate to contact us if you have any questions.</div>
                            </p>
                            <p>Best Regards,<br/>{5}</p></div>",
                        BuyerFirstName,
                        EmailInfoHelper.GetProductString(Items, "<br/>"),
                        EmailInfoHelper.GetDateString(NoticeLeftDate),
                        FullAddress,
                        TrackingNumber,
                        Signature);
                }
                else
                {
                    if (Currier == ShippingServiceUtils.DHLCarrier
                        || Currier == ShippingServiceUtils.DHLMXCarrier)
                    {
                        if (ShippingUtils.IsMexico(ShippingCountry)
                            || ShippingUtils.IsSpain(ShippingCountry))
                        {
                            return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Estimado {0},</p>
                            <p>
                                <div>DHL reportó la excepción entregando su orden de Amazon de:</div>
                                <div>{1}.</div>
                            </p>
                            <p>
                                <div>Revise el historial de seguimiento en <a href='{2}'>{2}</a> y póngase en contacto con DHL en su país para organizar la devolución o la recogida.</div>
                            </p>
                            <p>Atentamente,<br/>{3}</p></div>",
                                BuyerFirstName,
                                EmailInfoHelper.GetProductString(Items, "<br/>"),
                                String.Format(
                                    "http://www.dhl.com/content/g0/en/express/tracking.shtml?AWB={0}&brand=DHL",
                                    TrackingNumber),
                                Signature);
                        }
                        else
                        {
                            return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Dear {0},</p>
                            <p>
                                <div>DHL reported exception delivering your amazon order of:</div>
                                <div>{1}.</div>
                            </p>
                            <p>
                                <div>Please review tracking history at <a href='{2}'>{2}</a> and contact DHL in your country to arrange redelivery or pick up.</div>
                            </p>
                            <p>Best Regards,<br/>{3}</p></div>",
                                BuyerFirstName,
                                EmailInfoHelper.GetProductString(Items, "<br/>"),
                                String.Format(
                                    "http://www.dhl.com/content/g0/en/express/tracking.shtml?AWB={0}&brand=DHL",
                                    TrackingNumber),
                                Signature);
                        }
                    }
                    else
                    {
                        if (ShippingUtils.IsCanada(ShippingCountry))
                        {
                            return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Dear {0},</p>
                            <p>
                                <div>Canada Post attempted to deliver your order of:</div>
                                <div>{1}</div>
                                <div>on {2},</div>
                                <div>to: {3}</div>
                                <div>but nobody was available to accept the package and they left a notice.</div>
                            </p>
                            <p>
                                Please contact Canada Post to get your order, and provide them your tracking number {4} or proceed with instructions on the back of the notice card. Orders which aren’t picked up within 7 days usually sent back and may encounter additional fees.
                            </p>
                            <p>
                                Please see this link for additional information: <a href='https://www.canadapost.ca/web/en/kb/details.page?article=learn_what_attempte'>https://www.canadapost.ca/web/en/kb/details.page?article=learn_what_attempte</a>
                            </p>
                            <p>
                                <div>Please don’t hesitate to contact us if you have any questions.</div>
                            </p>
                            <p>Best Regards,<br/>{5}</p></div>",
                                BuyerFirstName,
                                EmailInfoHelper.GetProductString(Items, "<br/>"),
                                EmailInfoHelper.GetDateString(NoticeLeftDate),
                                FullAddress,
                                TrackingNumber,
                                Signature);
                        }
                        else
                        {
                            if (ShippingUtils.IsUK(ShippingCountry))
                            {
                                return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Dear {0},</p>
                            <p>
                                <div>Royal Mail attempted to deliver your order of:</div>
                                <div>{1}</div>
                                <div>on {2},</div>
                                <div>to: {3}</div>
                                <div>but nobody was available to accept the package and they left a notice.</div>
                            </p>
                            <p>
                                Please contact Royal Mail to get your order, or try to schedule redelivery at <a href='http://www.royalmail.com/personal/receiving-mail/redelivery'>http://www.royalmail.com/personal/receiving-mail/redelivery</a>. You will need to provide your tracking number {4}. Alternatively you can proceed with instructions on the back of the notice card. Orders which aren’t picked up within 7 days usually sent back and may encounter additional fees.
                            </p>
                            <p>
                                Please don’t hesitate to contact us if you have any questions.
                            </p>
                            <p>Best Regards,<br/>{5}</p></div>",
                                    BuyerFirstName,
                                    EmailInfoHelper.GetProductString(Items, "<br/>"),
                                    EmailInfoHelper.GetDateString(NoticeLeftDate),
                                    FullAddress,
                                    TrackingNumber,
                                    Signature);
                            }
                            else
                            {
                                return string.Format(@"<div style='font-size: 11pt; font-family: Calibri'>
                            <p>Dear {0},</p>
                            <p>
                                <div>Your Local Postal Service attempted to deliver your order of:</div>
                                <div>{1}</div>
                                <div>on {2},</div>
                                <div>to: {3}</div>
                                <div>but nobody was available to accept the package and they left a notice.</div>
                            </p>
                            <p>
                                Please contact your Post Office to get your order. You will be asked to provide your tracking number {4}. Alternatively you can proceed with instructions on the back of the notice card. Orders which aren’t picked up within 7 days usually sent back and may encounter additional fees.
                            </p>
                            <p>
                                Please don’t hesitate to contact us if you have any questions.
                            </p>
                            <p>Best Regards,<br/>{5}</p></div>",
                                    BuyerFirstName,
                                    EmailInfoHelper.GetProductString(Items, "<br/>"),
                                    EmailInfoHelper.GetDateString(NoticeLeftDate),
                                    FullAddress,
                                    TrackingNumber,
                                    Signature);
                            }
                        }
                    }
                }
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public NoticeLeftEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            IList<ListingOrderDTO> items, 
            ShippingTypeCode serviceType,
            string currier,
            DateTime? noticeLeftDate,
            string trackingNumber,
            AddressDTO address,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            Items = items;

            ServiceType = serviceType;
            Currier = currier;
            NoticeLeftDate = noticeLeftDate;

            TrackingNumber = trackingNumber;
            FullAddress = AddressHelper.ToStringForLetterWithPersonName(address);
            ShippingCountry = address.FinalCountry;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
