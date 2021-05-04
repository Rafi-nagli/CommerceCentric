using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Listings;

namespace Amazon.Web.ViewModels.Products
{
    public class ListingDefectViewModel
    {
        public string Explanation { get; set; }
        public string FieldName { get; set; }
        public string AlertType { get; set; }
        public string AlertName { get; set; }
        public string Status { get; set; }

        public ListingDefectViewModel()
        {
            
        }

        public ListingDefectViewModel(ListingDefectDTO listingDefect)
        {
            Explanation = listingDefect.Explanation;
            FieldName = listingDefect.FieldName;
            AlertType = listingDefect.AlertType;
            AlertName = listingDefect.AlertName;
            Status = listingDefect.Status;
        }
    }
}