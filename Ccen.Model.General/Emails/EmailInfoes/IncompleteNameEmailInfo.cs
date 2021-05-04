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
    public class IncompleteNameEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.IncompleteName; }
        }
        
        private IList<ListingOrderDTO> Items { get; set; }
        
        public string Subject
        {
            get
            {
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                    return String.Format("[Important] Incomplete name request from Amazon seller {1} (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market), SignatureCompany);
                return String.Format("[Important] Incomplete name request (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        private string FullAddress { get; set; }

        public string Body
        {
            get { return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                 <p>Dear {0},</p>
                                 <p>Your order for {1} is missing full name of the recipient, which is required to process and ship your order.</p>
                                 <p>Please reply to this email with recipient’s full name ASAP, and verify that following shipping address is correct:</p>
                                 <p>{2}</p>
                                 <br/>
                                 <p>Best Regards,<br/>{3}</p></div>", 
                        BuyerFirstName, 
                        EmailInfoHelper.GetProductString(Items, ", "),
                        FullAddress,
                        Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public IncompleteNameEmailInfo(IAddressService addressService,
            string byName,
            string orderNumber, 
            MarketType market,
            AddressDTO address,
            IList<ListingOrderDTO> items,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            FullAddress = AddressHelper.ToStringForLetterWithPersonName(address);

            Items = items;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
