using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.Reports
{
    public class ReturnedOrderViewModel 
    {
        public long Id { get; set; }
        public string CustomerOrderId { get; set; }        
        public string OrderUrl => "/Order/Orders?orderId=" + CustomerOrderId; 
        public DateTime? OrderDate { get; set; }
        public string Reason { get; set; }

        public long? StyleItemId { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }
    }
}