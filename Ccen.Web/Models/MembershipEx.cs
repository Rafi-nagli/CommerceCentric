using System.Web.Security;
using Amazon.Web.ViewModels;

namespace Amazon.Web.Models
{
    public class MembershipEx
    {
        public static bool Login(LogOnModel model, string returnUrl)
        {
            var result = Membership.ValidateUser(model.UserName, model.Password);
            if (result)
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
            }
            return result;
        }

        public static void Logout()
        {
            FormsAuthentication.SignOut();
        }
    }
}