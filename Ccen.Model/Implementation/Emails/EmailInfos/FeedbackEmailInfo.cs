using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models
{
    public class FeedbackEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get
            {
                return EmailTypes.RequestFeedback;
            }
        }
        
        public string Subject
        {
            get
            {
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                    return String.Format("Feedback request from Amazon seller Premium Apparel (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
                return String.Format("Feedback request (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        private IList<string> ItemNames { get; set; }
        
        public string Body
        {
            get
            {
                if (Market == MarketType.Amazon || Market == MarketType.AmazonEU || Market == MarketType.AmazonAU)
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                    <p>Dear {0},</p>
                    <p>
                        Thank you for purchasing {1} from our store on Amazon Marketplace. We appreciate a lot your business. We are sure you and your child will enjoy it a lot.
                    </p>
                    <p>
                        We would be grateful if you could leave us a 5 star rating on <a href={2}>Amazon</a> (<b><a href={2}>https://www.amazon.com/feedback</a></b>)  as it will help us to grow our store and bring more great products to our valuable clients like yourself. Please don't hesitate to contact us if there is anything we could of done better or if you have questions about your order. Our customer support phone number is 1-888-866-2451.
                    </p>
                    <p>
                        We hope to see you soon again in our store at <a href={3}>http://www.amazon.com/shops/premiumapparel</a>
                    </p>

                    <p>Thank you,<br/>{4}</p></div>",
                        BuyerFirstName,
                        PurchasedItemsString,
                        "\"https://www.amazon.com/feedback\"",
                        "\"http://www.amazon.com/shops/premiumapparel\"",
                        Signature);
                }
                return null;
            }
        }

        private string BuyerFirstName 
        { 
            get
            {
                return EmailInfoHelper.GetFirstName(ToName);
            } 
        }

        public string PurchasedItemsString
        {
            get { return EmailInfoHelper.GetShortProductString(ItemNames); }
        }

        public FeedbackEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber, 
            MarketType market,
            string buyerName,
            string buyerEmail,
            IList<ListingOrderDTO> itemNames) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;

            ItemNames = itemNames.Select(i => i.Title).ToList();
        }
    }
}