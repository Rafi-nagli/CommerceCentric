using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;

namespace Amazon.Web.Filters
{

    /// <summary>
    /// http://www.codeproject.com/Articles/731913/Exception-Handling-in-MVC
    /// 
    /// </summary>
    public class LogHandleErrorAttribute : HandleErrorAttribute
    {
        private bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) // || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }
 
            // if the request is AJAX return JSON else view.
            var isAjax = IsAjax(filterContext);
            if (isAjax)
            {
                //Because its a exception raised after ajax invocation
                //Lets return Json
                filterContext.Result = new JsonResult{Data=ExceptionHelper.GetMostDeeperException(filterContext.Exception).Message,
                    JsonRequestBehavior=JsonRequestBehavior.AllowGet};
 
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 400;
            }
            else
            {
                //Normal Exception
                //So let it handle by its default ways.
                base.OnException(filterContext);
                
            }

            if (filterContext.Exception != null)
            {
                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();
                //string loggerName = string.Format("{0}Controller.{1}", controller, action);
                var message = "Unhandled exception (isAjax=" + isAjax + ", controller=" + controller + ", action=" + action + "): " + ExceptionHelper.GetExceptionContextInfo(filterContext.HttpContext, filterContext.RequestContext.RouteData);
                var errorMessage = ExceptionHelper.GetMessage(filterContext.Exception);

                log4net.LogManager.GetLogger("RequestLogger")
                    .Fatal(message + Environment.NewLine + errorMessage, filterContext.Exception);
            }
        }
    }
}