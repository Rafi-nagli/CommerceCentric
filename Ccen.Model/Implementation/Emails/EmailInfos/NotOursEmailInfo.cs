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
    public class NotOursEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.NotOurs; }
        }

        /*
         “              
Please add email template called “Not ours”, put it at then end with following text:
“We received %ItemName% you haven't purchased from us, please provide a prepaid shipping label, with the address where you want us to send it.
Please reply within 7 days, after that time we will have to dispose of it.”
And add note: “Emailed client about wrong item we received, gave him 7 days to respond

”
        */

        public string Subject
        {
            get
            {
                return string.Format("Not ours (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},<br/>
                                We received %ItemName% you haven't purchased from us, please provide a prepaid shipping label, with the address where you want us to send it.<br/>
                                Please reply within 7 days, after that time we will have to dispose of it.
                            </p>
                            <p>Best Regards,<br/>{1}</p>
                        </div>",
                    BuyerFirstName,
                    Signature);
            }
        }

        private IList<ListingOrderDTO> Items { get; set; }

        public NotOursEmailInfo(IAddressService addressService, 
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
