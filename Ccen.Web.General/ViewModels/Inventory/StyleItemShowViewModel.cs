using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleItemShowViewModel
    {
        public long StyleId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }

        public bool OnHold { get; set; }

        public int? RemainingQuantity { get; set; }

        public int? InventoryQuantity { get; set; }
        public DateTime? InventoryQuantitySetDate { get; set; }

        public int? ScannedSoldQuantityFromDate { get; set; }
        public int? SentToFBAQuantityFromDate { get; set; }
        public int? MarketsSoldQuantityFromDate { get; set; }
        public int? SpecialCaseQuantityFromDate { get; set; }
        public int? SentToPhotoshootQuantityFromDate { get; set; }


        public int? TotalScannedSoldQuantity { get; set; }
        public int? TotalSentToFBAQuantity { get; set; }
        public int? TotalMarketsSoldQuantity { get; set; }
        public int? TotalSpecialCaseQuantity { get; set; }
        public int? TotalSentToPhotoshootQuantity { get; set; }


        public int? BoxQuantity { get; set; }
        public DateTime? BoxQuantitySetDate { get; set; }


        public DateTime? ScannedMaxOrderDate { get; set; }


        public string SalesInfo { get; set; }

        public bool? IsInVirtual { get; set; }

        public bool HasSale 
        {
            get { return !String.IsNullOrEmpty(SalesInfo); }
        }


        public string Name
        {
            get
            {
                var name = this.Size;
                if (!String.IsNullOrEmpty(this.Color))
                    name += "/" + this.Color;
                return name;
            }
        }
    }
}