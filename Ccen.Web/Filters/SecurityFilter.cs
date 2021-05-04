using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Amazon.Web.Models;

namespace Amazon.Web.Filters
{
    public class SecurityFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {

            //HttpCookie authCookie =
            //  filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];

            //if (authCookie != null)
            //{
            //    FormsAuthenticationTicket authTicket =
            //           FormsAuthentication.Decrypt(authCookie.Value);
            //    var identity = new GenericIdentity(authTicket.Name, "Forms");
            //    var principal = new GenericPrincipal(identity, new string[] { authTicket.UserData });
            //    filterContext.HttpContext.User = principal;
            //}

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            var user = filterContext.HttpContext.User;
            var ip = filterContext.HttpContext.Request.UserHostAddress;

            var userDto = AccessManager.User; //check that user exist in db and active

            var isAccessAllowedByUser = userDto != null;
            var isAccessAllowedByAction = action == "LogOn" 
                || action == "Logoff"
                || controller == "Image"
                || controller == "ImageGet"
                || (controller == "Item" && (action == "GetRank" || action == "CheckRank"));
            if (!isAccessAllowedByAction && !isAccessAllowedByUser)
            {
                FormsAuthentication.RedirectToLoginPage();
            }
        }
    }
}
