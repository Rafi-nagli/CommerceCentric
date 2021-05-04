using Amazon.DTO.CustomReports;
using Amazon.Web.ViewModels.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReports
{   
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CustomFilterAttribute : Attribute
    {
        public long Id { get; set; }
        public string Header { get; set; }
        public string Title { get; set; }
        public FilterOperation Operation { get; set; }
        public string ValueString { get; set; }  
        public bool MultipleValues { get; set; }
    }
}