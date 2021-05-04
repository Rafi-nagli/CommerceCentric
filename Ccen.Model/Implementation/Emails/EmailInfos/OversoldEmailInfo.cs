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
    public class OversoldEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.Oversold; }
        }
        
        private IList<ListingOrderDTO> Items { get; set; }

        public string FirstItemItemStyle
        {
            get { return Items.FirstOrDefault()?.ItemStyle; }
        }

        public string SubLicense { get; set; }

        /*
         “Dear %FirstName%, 
            We are very sorry but the last piece of the %Pajama Name% you have ordered was damaged and we couldn't send it to you, we will be happy to replace it with another %Sublicnse% %ItemType%  (picture attached)  or refund it.
            Please let me know what can we do to fix this problem. We appreciate your help and understanding.

            Example:
            %Sublicnse% %ItemType%  - Pokemon 2 Piece Pajama or Superman Hooded Towel
        */
        public string Subject
        {
            get
            {
                return string.Format("Oversold (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                        <p>Dear {0},</p>
                        <p>
                            <div>We are very sorry but the last piece of the {1} you have ordered was damaged and we couldn't send it to you, we will be happy to replace it with another {2} {3} (picture attached)  or refund it.</div>
                            <div>Please let me know what can we do to fix this problem. We appreciate your help and understanding.</div>
                        </p>
                        <p>Best Regards,<br/>{4}</p></div>",
                    BuyerFirstName,
                    EmailInfoHelper.GetProductString(Items.Take(1).ToList(), ", "),
                    SubLicense,
                    FirstItemItemStyle,
                    Signature);
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public OversoldEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            IList<ListingOrderDTO> items,
            string subLicense,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            Items = items;
            SubLicense = subLicense;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
