using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models
{
    public class UnverifiedAddressEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AddressVerify; }
        }
        
        public string Subject
        {
            get
            {
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                    return String.Format("[Important] Address verification request from Amazon seller Premium Apparel (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
                return String.Format("[Important] Address verification request (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        private string FullAddress { get; set; }

        private string ShipTime { get; set; }
        private const string PmTime = "1:30pm EST";
        private const string AmTime = "9:00am EST";
        /*      %time%: If email sent
         * 12am-12pm (noon), - “1:30pm (EST) today”
         * 12:01pm-11:59pm – “9:00am(EST) tomorrow”         */

        public string Body
        {
            get { return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                 <p>Dear {0},</p>
                                 <p>USPS reported that address you have provided can't be verified:</p>
                                 <p>{1}</p>
                                 <p>Can you please check the address, to make sure your package will be delivered accurately and promptly.</p>
                                 <p>We would appreciate if you reply to this email before {2} with a corrected address or confirm it’s ok to send as-is.</p>
                                 <br/>
                                 <p>Best Regards,<br/>{3}</p></div>", 
                        BuyerFirstName, 
                        FullAddress, 
                        ShipTime,
                        Signature);
            }
        }
        
        public UnverifiedAddressEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            AddressDTO address,
            string buyerName, 
            string buyerEmail,
            DateTime estShipDate) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            var today = DateHelper.GetAppNowTime().Date;
            var now = DateHelper.GetAppNowTime();
            //If (Estimated Ship Date > Today) || (Estimated Ship Date == Today & TimeNow <12pm) then %Time% = estimated ship date 1:30pm (EST). (i.e. Nov 25th, 1:30pn EST).
            //If (Estimated Ship Date == Today & TimeNow >12pm) || (Estimated Ship Date < Today)  %time% = %Tomorrow% 8am EST
            if (estShipDate.Date > today ||
                (estShipDate.Date == today && now.TimeOfDay < new TimeSpan(12, 0, 0)))
                ShipTime = estShipDate.ToString("MMM dd") + ", " + PmTime;
            else
                ShipTime = now.AddDays(1).ToString("MMM dd") + ", " + AmTime;

            FullAddress = AddressHelper.ToStringForLetterWithPersonName(address);

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
