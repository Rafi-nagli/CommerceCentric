using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReports
{
    public class LinkButtonInfo : LinkInfo
    {       
        public string Icon { get; set; }
    }

    public class LinkInfo
    {
        public string HRef { get; set; }        
        public string Text { get; set; }
    }
}