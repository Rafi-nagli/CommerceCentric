using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.InventoryGroup
{
    public class InventoryGroupFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }


        public static InventoryGroupFilterViewModel Default
        {
            get { return new InventoryGroupFilterViewModel(); }
        }
    }
}