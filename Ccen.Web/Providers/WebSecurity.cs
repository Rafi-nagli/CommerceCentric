using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Amazon.DAL;

namespace Amazon.Web.Providers
{
    public class WebSecurity
    {
        public static HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        public static HttpRequestBase Request
        {
            get { return Context.Request; }
        }

        public static HttpResponseBase Response
        {
            get { return Context.Response; }
        }

        public static IPrincipal User
        {
            get { return Context.User; }
        }

        public static bool IsAuthenticated
        {
            get { return User.Identity.IsAuthenticated; }
        }

        public enum MembershipLoginStatus
        {
            Success, Failure
        }

        public static MembershipCreateStatus Register(string username, string password, string email, bool isApproved,
            string firstName, string lastName)
        {
            MembershipCreateStatus createStatus;
            Membership.CreateUser(username, password, email, null, null, isApproved, null, out createStatus);

            if (createStatus == MembershipCreateStatus.Success)
            {
                using (var unitOfWork = new UnitOfWork(null))
                {
                    var user = unitOfWork.Users.GetByName(username);
                    if (user != null)
                    {
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        unitOfWork.Commit();
                    }
                }
                if (isApproved)
                {
                    FormsAuthentication.SetAuthCookie(username, false);
                }
            }
            return createStatus;
        }

        public static MembershipLoginStatus Login(string username, string password, bool rememberMe)
        {
            try
            {
                if (!Membership.ValidateUser(username, password))
                {
                    return MembershipLoginStatus.Failure;
                }
                FormsAuthentication.SetAuthCookie(username, rememberMe);
                return MembershipLoginStatus.Success;
            }
            catch
            {
                return MembershipLoginStatus.Failure;
            }
        }

        public static void Logout()
        {
            FormsAuthentication.SignOut();
        }

        public static MembershipUser GetUser(string username)
        {
            return Membership.GetUser(username);
        }

        public static bool ChangePassword(string oldPassword, string newPassword)
        {
            var currentUser = Membership.GetUser(User.Identity.Name);
            return currentUser != null && currentUser.ChangePassword(oldPassword, newPassword);
        }

        public static bool DeleteUser(string username)
        {
            return Membership.DeleteUser(username);
        }

        public static List<MembershipUser> FindUsersByEmail(string email, int pageIndex, int pageSize)
        {
            int totalRecords;
            return Membership.FindUsersByEmail(email, pageIndex, pageSize, out totalRecords).Cast<MembershipUser>().ToList();
        }

        public static List<MembershipUser> FindUsersByName(string username, int pageIndex, int pageSize)
        {
            int totalRecords;
            return Membership.FindUsersByName(username, pageIndex, pageSize, out totalRecords).Cast<MembershipUser>().ToList();
        }

        public static List<MembershipUser> GetAllUsers(int pageIndex, int pageSize)
        {
            int totalRecords;
            return Membership.GetAllUsers(pageIndex, pageSize, out totalRecords).Cast<MembershipUser>().ToList();
        }
    }
}