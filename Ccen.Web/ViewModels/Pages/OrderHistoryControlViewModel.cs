using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Pages
{
    public class OrderHistoryControlViewModel
    {
        public string OrderNumber { get; set; }
        public bool IsCollapsed { get; set; }
        public long? EmailId { get; set; }
    }
}