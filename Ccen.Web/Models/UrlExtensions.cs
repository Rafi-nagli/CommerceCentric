using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.Models
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Return a view path based on an action name and controller name
        /// </summary>
        /// <param name="url">Context for extension method</param>
        /// <param name="action">Action name</param>
        /// <param name="controller">Controller name</param>
        /// <returns>A string in the form "~/views/{controller}/{action}.cshtml</returns>
        public static string View(this System.Web.Mvc.UrlHelper url, string action, string controller)
        {
            return string.Format("~/Views/{1}/{0}.cshtml", action, controller);
        }
    }
}