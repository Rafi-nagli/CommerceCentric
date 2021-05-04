using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.InventoryGroup
{
    public class InventoryGroupItemViewModel
    {
        public long Id { get; set; }
        public string StyleString { get; set; }
        public long StyleId { get; set; }

        public string Name { get; set; }
        public int Quantity { get; set; }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }
    }
}