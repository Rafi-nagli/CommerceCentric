using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels.Emails
{
    public class ComposeEmailParams
    {
        public string OrderNumber { get; set; }
        public int EmailType { get; set; }
    }
}