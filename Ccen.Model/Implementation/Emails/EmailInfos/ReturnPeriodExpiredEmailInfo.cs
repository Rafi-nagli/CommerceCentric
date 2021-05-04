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
    public class ReturnPeriodExpiredEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.ReturnPeriodExpired; }
        }

        /*
         “              
“Dear %FirstName%,
Unfortunately 30 days return period already expired and your order can’t be returned for refund or exchange.

Best Regards,
%SupportName%
%CompanyName%



        */

        public string Subject
        {
            get
            {
                return string.Format("Return Period Expired (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},<br/>
                                Unfortunately 30 days return period already expired and your order can’t be returned for refund or exchange.
                            </p>
                            <p>Best Regards,<br/>{1}</p>
                        </div>",
                        BuyerFirstName,
                        Signature);                
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public ReturnPeriodExpiredEmailInfo(IAddressService addressService, 
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
