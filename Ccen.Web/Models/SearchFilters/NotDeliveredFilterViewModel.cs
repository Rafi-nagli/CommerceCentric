using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Models.SearchFilters
{
    public class NotDeliveredFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string BuyerName { get; set; }
        public string OrderNumber { get; set; }

        public string Status { get; set; }

        public static string AllStatus = "";
        public static string AllWithoutDismissedStatus = "All w/o dimsissed";
        public static string DismissStatus = "Dismissed";
        public static string SubmittedClaimStatus = "SubmittedClaim";
        public static string HighlightStatus = "Highlighted";

        public static SelectList StatusList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", AllStatus),
                    new KeyValuePair<string, string>("All w/o dismissed", AllWithoutDismissedStatus),
                    new KeyValuePair<string, string>("Submitted Claim", SubmittedClaimStatus),
                    new KeyValuePair<string, string>("Dismissed", DismissStatus),
                    new KeyValuePair<string, string>("Highlighted", HighlightStatus),
                }, "Value", "Key");
            }
        }


        public static NotDeliveredFilterViewModel Empty
        {
            get
            {
                return new NotDeliveredFilterViewModel()
                {
                    Status = AllWithoutDismissedStatus
                };
            }
        }
    }
}