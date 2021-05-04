using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Amazon.DAL;
using Amazon.Web.App_Start;
using Amazon.Web.Binder;
using Amazon.Web.General.Models;
using Amazon.Web.ModelBinder;
using Amazon.Web.Models;
using Ccen.Web;
using log4net;
using log4net.Config;

namespace Amazon.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private ILog _logger = log4net.LogManager.GetLogger("RequestLogger");

        protected void Application_Start()
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            AreaRegistration.RegisterAllAreas();
            XmlConfigurator.Configure(new FileInfo(AppSettings.log4net_Config));
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(bool), new OnOffBinder());
            Database.SetInitializer<AmazonContext>(null);
            //DefaultDbInitializer.Init();

            UrlManager.UrlService = new UrlService();

            _logger.Info("Application started");
        }

        protected string[] ExcludeActionList = {"RequestUpdate", "CheckForUpdate"};

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (AppSettings.IsForceHttps)
            {
                if (!Request.IsLocal &&
                    (String.IsNullOrEmpty(Request.Path) 
                    || !Request.Path.ToLower().StartsWith("/image")
                    || !Request.Path.ToLower().StartsWith("/services")))
                {
                    //NOTE: Force https
                    switch (Request.Url.Scheme)
                    {
                        case "https":
                            Response.AddHeader("Strict-Transport-Security", "max-age=300");
                            break;
                        case "http":
                            var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
                            Response.Status = "301 Moved Permanently";
                            Response.AddHeader("Location", path);
                            break;
                    }
                }
            }

            try
            {
                var url = HttpContext.Current.Request.Url.ToString();
                if (!url.Contains(".js") && !url.Contains(".css"))
                {
                    var skipped = ExcludeActionList.Any(a => url.Contains("/" + a));
                    if (!skipped)
                        _logger.Debug("------start: " + url + " ThreadID: " +
                                 System.Threading.Thread.CurrentThread.ManagedThreadId
                                 + ", Timestamp: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.ms"));
                }
            }
            catch
            {
            }
        }


        protected void Application_EndRequest(object sender, EventArgs e)
        {
            try
            {
                var url = HttpContext.Current.Request.Url.ToString();
                if (!url.Contains(".js") && !url.Contains(".css"))
                {
                    var skipped = ExcludeActionList.Any(a => url.Contains("/" + a));
                    if (!skipped)
                        _logger.Debug("--------end: " + url + " ThreadID: " +
                                 System.Threading.Thread.CurrentThread.ManagedThreadId
                                 + ", Timestamp: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.ms"));
                }
            }
            catch
            {
            }
        }
    }
}