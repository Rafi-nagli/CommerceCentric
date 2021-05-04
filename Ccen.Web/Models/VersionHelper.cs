using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Amazon.Web.App_Start;

namespace Amazon.Web.Models
{
    public class VersionHelper
    {
        public static string GetAssemblyVersion()
        {
            return Assembly.GetAssembly(typeof (BundleConfig)).GetName().Version.ToString();
        }
    }
}