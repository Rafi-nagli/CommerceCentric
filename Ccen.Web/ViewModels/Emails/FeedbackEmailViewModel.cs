using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Emails
{
    public class FeedbackEmailViewModel : IEmailInfo
    {
        public string BuyerName { get; set; }
        public string OrderId { get; set; }
        public string From { get; set; }
        public string SenderName { get; set; }
        [Display(Name = "To:")]
        public string Email { get; set; }
        public string Subject {get { return "Amazon feedback request"; }}

        public bool IsEmailed { get; set; }

        public IEnumerable<string> ItemNames { get; set; }

        public static FeedbackEmailViewModel GetByOrderId(IItemOrderMappingRepository mappings, string orderId)
        {
            var model = mappings.GetEmailInfo(orderId);
            return new FeedbackEmailViewModel
            {
                OrderId = model.OrderId,
                BuyerName =model.BuyerName,
                Email = model.Email,
                ItemNames = new List<string> (model.ItemNames),
                IsEmailed = model.IsEmailed
            };
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style={4}>
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

                    <p> Thank you,</p>
                    <p> <b>- Premium Apparel</b></p></div>", BuyerFirstName, PurchasedItemsString,
                    "\"https://www.amazon.com/feedback\"", "\"http://www.amazon.com/shops/premiumapparel\"", "\"font-size: 11pt; font-family: Calibri\"");


            }
        }

        public string BuyerFirstName 
        { 
            get
            {
                return !string.IsNullOrEmpty(BuyerName) ? BuyerName.Split(' ').First() : string.Empty;
            } 
        }

        public string PurchasedItemsString
        {
            get
            {
                if (ItemNames == null || !ItemNames.Any())
                {
                    return string.Empty;
                }
                var number = ItemNames.Count();
                switch (number)
                {
                    case 1:
                        return "\"" + ItemNames.First() + "\"";
                    case 2:
                        return "\"" + ItemNames.First() + "\" & \"" + ItemNames.Last() + "\" pajamas";
                    default:
                        return number + " pajamas";
                }
            }
        }
    }
}