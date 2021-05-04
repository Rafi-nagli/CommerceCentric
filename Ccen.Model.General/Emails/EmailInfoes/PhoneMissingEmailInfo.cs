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
    public class PhoneMissingEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.PhoneMissing; }
        }
        
        private IList<ListingOrderDTO> Items { get; set; }
        
        public string Subject
        {
            get
            {
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                    return String.Format("[Important] Phone number request from Amazon seller {1} (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market), SignatureCompany);
                return String.Format("[Important] Phone number request (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        /*
         “Dear %Fname%,
            We have received your order for %pajamas%, but it’s missing phone number. Because we ship from US, phone number is required by international carriers.
            Please email it to us, as soon as possible.

            Best Regards,
            Customer Service
            “

         * */

        public string Body
        {
            get { return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                 <p>Dear {0},</p>
                                 <p>We have received your order for {1}, but it’s missing phone number. Because we ship from US, phone number is required by international carriers.<br/>
                                 Please email it to us, as soon as possible.</p>
                                 <p>Best Regards,<br/>{2}</p></div>", 
                        BuyerFirstName, 
                        EmailInfoHelper.GetProductString(Items, ", "),
                        Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public PhoneMissingEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            IList<ListingOrderDTO> items,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            Items = items;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
