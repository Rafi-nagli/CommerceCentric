using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ViewModels.Pages
{
    public class QuantityOperationPageViewModel
    {
        public string Type { get; set; }
        public string StyleId { get; set; }
        public List<SelectListItem> Types { get; set; }
        public List<SelectListItem> Users { get; set; }
    }
}