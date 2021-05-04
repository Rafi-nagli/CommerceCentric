using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Pages
{
    public class MailPageViewModel
    {
        public string OrderId { get; set; }

        public AddressViewModel ReturnAddress { get; set; }
        public AddressViewModel PickupAddress { get; set; }
    }
}