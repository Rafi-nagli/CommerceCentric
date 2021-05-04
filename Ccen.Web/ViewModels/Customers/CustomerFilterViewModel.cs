using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Customers
{
    public class CustomerFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string BuyerName { get; set; }
        public string OrderNumber { get; set; }

        public static CustomerFilterViewModel Empty
        {
            get
            {
                return new CustomerFilterViewModel()
                {

                };
            }
        }
    }
}