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
    public class AcceptReturnRequestEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AcceptReturnRequest;  }
        }
        
        public bool IsWasBiggerSize { get; set; }
        public string Size { get;set; }

        public string Subject
        {
            get { return String.Format("Exchange request for order (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market)); }
        }

        public string Body
        {
            get
            {
                return String.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                        <p>Dear Mr./Mrs. {0},</p>
                                        <p>
                                            <div>Thank you for your order. We are sorry the item you have received didn't fit.</div>
                                            <div>We will be happy to exchange it free of charge for {1} size - {2}.</div>
                                            <div>Please let us know if you like us to send you a {1} size.</div>
                                        </p>
                                        <p>Best Regards,<br/>{3}</p></div>",
                               BuyerLastName,
                               IsWasBiggerSize ? "smaller" : "bigger",
                               Size,
                               Signature);
            }
        }

        public AcceptReturnRequestEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            bool isWasBigger,
            string size,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            IsWasBiggerSize = isWasBigger;
            Size = size;
            
            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
