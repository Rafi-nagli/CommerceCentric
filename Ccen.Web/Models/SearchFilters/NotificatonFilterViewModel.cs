using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Core.Models;

namespace Amazon.Web.Models.SearchFilters
{
    public class NotificationFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string OrderNumber { get; set; }
        public bool InlcudeReaded { get; set; }
        public int? Type { get; set; }

        public bool OnlyPriority { get; set; }

        public SelectList TypeList
        {
            get
            {
                return new SelectList(NotificationTypes, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> NotificationTypes = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(NotificationHelper.ToString(NotificationType.LabelNeverShipped), (int)NotificationType.LabelNeverShipped),
            new KeyValuePair<string, int>(NotificationHelper.ToString(NotificationType.LabelGotStuck), (int)NotificationType.LabelGotStuck),
            new KeyValuePair<string, int>(NotificationHelper.ToString(NotificationType.AmazonProductImageChanged), (int)NotificationType.AmazonProductImageChanged),
        };

        public static NotificationFilterViewModel Empty
        {
            get
            {
                return new NotificationFilterViewModel()
                {

                };
            }
        }
    }
}