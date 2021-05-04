using System;
using System.Web.Mvc;
using System.Web.Security;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Account;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.Controllers
{
    [Authorize]
    public partial class AccountController : BaseController
    {
        public override string TAG
        {
            get { return "AccountController."; }
        }


        [Authorize]
        public virtual ActionResult Profile()
        {
            LogI("Profile begin");

            var profile = new ProfileViewModel();
            try
            {
                var user = AccessManager.User;
                if (user != null)
                {
                    profile.Email = user.Email;
                    if (user.Role != null)
                        profile.RoleName = user.Role.Name;
                }
            }
            catch (Exception ex)
            {
                LogE("Profile error", ex);
            }
            return View(profile);
        }
        
        [Authorize]
        public virtual ActionResult ChangePassword(PasswordViewModel model)
        {
            LogI("ChangePassword begin");
            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    model.Result = MessageResult.Success("Password has been successfully changed");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }
            return PartialView("_ChangePasswordPartial", model);
        }

        [Authorize]
        public virtual ActionResult AcceptTerms()
        {
            LogI("AcceptTerms, userId=" + AccessManager.UserId);

            var model = new AcceptTermsViewModel(DbFactory, LogService, Time);
            if (AccessManager.UserId.HasValue)
                model.Accept(AccessManager.UserId.Value, Request);

            return JsonGet(true);
        }

        [AllowAnonymous]
        public virtual ActionResult LogOn(string returnUrl)
        {
            LogI("LogOn begin");

            return View(new LogOnModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public virtual ActionResult LogOn(LogOnModel model)
        {
            LogI("LogOn begin, model=" + model);

            var result = false;
            try
            {
                result = Membership.ValidateUser(model.UserName, model.Password);
                if (result)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    SessionHelper.Clear();
                    var membmershipUser = Membership.GetUser(model.UserName);
                    long userId = (long)membmershipUser.ProviderUserKey;
                    return RedirectToLocal(model.ReturnUrl, userId);
                }
                else
                {
                    // If we got this far, something failed, redisplay form
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            
            return View(model);
        }

        public virtual ActionResult LogOff()
        {
            LogI("LogOff begin");

            FormsAuthentication.SignOut();
            SessionHelper.Clear();

            return RedirectToAction("Index", "Home");
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl, long? userId)
        {
            int? roleId = null;
            if (userId.HasValue)
                roleId = Db.Users.GetByIdAsDto(userId.Value)?.RoleId;
            var roleName = "";
            if (roleId.HasValue)
                roleName = AccessManager.GetRoleName(roleId.Value);

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (roleName == AccessManager.RoleRestricted)
                return RedirectToAction("Index", "Dashboard");
            else 
                return RedirectToAction("Index", "Home");
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }


        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
