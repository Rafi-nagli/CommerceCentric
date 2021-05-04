using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleBoxItemViewModel
    {
        public long StyleId { get; set; }
        public long StyleItemId { get; set; }
        public int BoxQuantity { get; set; }
        public int Quantity { get; set; }

        //public int? ApproveStatus { get; set; }
    }
}