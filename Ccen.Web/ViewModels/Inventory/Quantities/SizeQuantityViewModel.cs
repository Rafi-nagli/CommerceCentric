using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class SizeQuantityViewModel
    {
        public long StyleItemId { get; set; }
        public string SizeGroupName { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        
        //Manually
        
        //INPUT
        public int? NewManuallyQuantity { get; set; }
        public int? BaseManuallyQuantity { get; set; }
        public bool UseBoxQuantity { get; set; }
        public DateTime? NewRestockDate { get; set; }

        public bool OnHold { get; set; }

        public bool? IsRemoveRestockDate { get; set; }
        
        public int? ManuallyQuantity { get; set; }
        public DateTime? ManuallyQuantitySetDate { get; set; }
        public DateTime? RestockDate { get; set; }
        public int? ManuallySoldQuantity { get; set; }

        public int TotalMarketsSoldQuantity { get; set; }
        public int TotalScannedSoldQuantity { get; set; }
        public int TotalSentToFBAQuantity { get; set; }
        public int TotalSpecialCaseQuantity { get; set; }
        public int TotalSentToPhotoshootQuantity { get; set; }

        public bool HasManullyQuantity
        {
            get { return ManuallyQuantity.HasValue; }
        }


        //Box
        public int? BoxQuantity { get; set; }
        public DateTime? BoxQuantitySetDate { get; set; }
        public int BoxSoldQuantity { get; set; }

        public int? RemainingQuantity { get; set; }


    }
}