using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Models.SearchFilters
{
    public class FeedbackFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string BuyerName { get; set; }
        public string OrderNumber { get; set; }

        public string FeedbackStatus { get; set; }

        public static string AllStatus = "";
        public static string DeliveredNotSentStatus = "DeliveredNotSent";
        public static string NotDeliveredStatus = "NotDelivered";
        public static string AlreadySentStatus = "AlreadySent";

        public static SelectList FeedbackStatusList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", AllStatus),
                    new KeyValuePair<string, string>("Delivered, Not Sent", DeliveredNotSentStatus),
                    new KeyValuePair<string, string>("Not Delivered", NotDeliveredStatus),
                    new KeyValuePair<string, string>("Already Sent", AlreadySentStatus),
                }, "Value", "Key");
            }
        }


        public static FeedbackFilterViewModel Empty
        {
            get
            {
                return new FeedbackFilterViewModel()
                {
                    FeedbackStatus = DeliveredNotSentStatus
                };
            }
        }
    }
}