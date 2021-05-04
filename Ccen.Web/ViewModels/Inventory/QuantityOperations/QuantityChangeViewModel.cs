using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class QuantityChangeViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public long StyleItemId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public bool InActive { get; set; }

        public string StyleUrl
        {
            get { return Amazon.Web.Models.UrlHelper.GetStyleUrl(StyleString); }
        }
    }
}