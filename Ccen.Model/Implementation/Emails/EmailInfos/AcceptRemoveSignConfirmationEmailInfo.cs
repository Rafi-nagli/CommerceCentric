using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class AcceptRemoveSignConfirmationEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AcceptRemoveSignConfirmation;  }
        }
        
        private IList<ListingOrderDTO> Items { get; set; }

        public string Subject
        {
            get { return String.Format("Signature requirement removed (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market)); }
        }

        public string Body
        {
            get
            {
                return String.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                        <p>Dear {0},</p>
                                        <p>Per your request we have removed Signature Requirement from the order {1}.</p>
                                        <br/>
                                        <p>Best Regards,<br/>{2}</p></div>",
                               BuyerFirstName,
                               EmailInfoHelper.GetProductString(Items, ", "),
                               Signature);
            }
        }

        public AcceptRemoveSignConfirmationEmailInfo(IAddressService addressService, 
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
