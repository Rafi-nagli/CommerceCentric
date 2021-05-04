using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleSizeHistoryViewModel
    {
        public long StyleItemId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
    }
}