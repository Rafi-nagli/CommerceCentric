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
    public class DuplicateEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.Duplicate; }
        }
        
        private DuplicateOrdersDTO DuplicateInfo { get; set; }

        /*
         Updated:
         * Subject: Duplicate Order Alert (Order...)
         * Dear %NAME%,
We noticed that you have placed 2 identical orders for %Product%. If it was intentional, you don't need to do anything and both will ship today. If you need to cancel this order please do so before 1:30pm (EST) today, by following instructions at http://www.amazon.com/gp/help/customer/display.html?nodeId=595034

Best Regards,
Premium Apparel

         * 
         Old:
         “Dear Client, 
    We have noticed you put %number of duplicate order% identical orders for %pajama name% %pajama size%, 
    order numbers are: %list of all duplicate order numbers%. 
    If this was intentional, you don’t need to do anything and all mentioned above orders will be shipped to your shortly. 
    If duplicate orders were accidental, please cancel them as described in http://www.amazon.com/gp/help/customer/display.html?nodeId=595034.
         */


        public string Subject
        {
            get
            {
                return string.Format("[Important] Duplicate Order Alert (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},</p>
                            <p>
                                <div>We noticed that you have placed 2 identical orders for {1}.</div>
                                <div>If it was intentional, you don’t need to do anything and both will ship today.</div>
                                <div>If you need to cancel this order please do so before 1:30pm (EST) today, by following instructions at</div>
                                <div><a href={2}>{2}</a></div>
                            </p>
                            <p>Best Regards,<br/>{3}</p></div>",
                        BuyerFirstName,
                        EmailInfoHelper.GetProductString(DuplicateInfo.Items, ", "),
                        "\"http://www.amazon.com/gp/help/customer/display.html?nodeId=595034\"",
                        Signature);
                }
                else
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                            <p>Dear {0},</p>
                            <p>
                                <div>We noticed that you have placed 2 identical orders for {1}.</div>
                                <div>If it was intentional, you don’t need to do anything and both will ship today.</div>
                                <div>If you need to cancel this order please do so before 1:30pm (EST) today</div>
                            </p>
                            <p>Best Regards,<br/>{2}</p></div>",
                                    BuyerFirstName,
                                    EmailInfoHelper.GetProductString(DuplicateInfo.Items, ", "),
                                    Signature);
                }
            }
        }

        public DuplicateEmailInfo(IAddressService addressService, 
            string orderNumber, 
            MarketType market,
            DuplicateOrdersDTO duplicateInfo,
            string buyerName, 
            string buyerEmail) : base(addressService)
        {
            Tag = orderNumber;
            Market = market;

            DuplicateInfo = duplicateInfo;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
