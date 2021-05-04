using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class StyleItemInfoViewModel
    {
        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }

        public string StyleColor { get; set; }
        public string StyleSize { get; set; }

        public int LinkedListingCount { get; set; }
        public int? RemainingQuantity { get; set; }
        
        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public StyleItemInfoViewModel()
        {
            
        }

        public StyleItemInfoViewModel(ItemShortInfoDTO itemInfo)
        {
            StyleId = itemInfo.StyleId;
            StyleString = itemInfo.StyleString;
            StyleItemId = itemInfo.StyleItemId;

            StyleSize = itemInfo.StyleSize;
            StyleColor = itemInfo.StyleColor;

            LinkedListingCount = itemInfo.LinkedListingCount;
            RemainingQuantity = itemInfo.RemainingQuantity < 0 ? 0 : itemInfo.RemainingQuantity;
        }
    }
}