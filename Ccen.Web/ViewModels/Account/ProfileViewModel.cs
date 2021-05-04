using Amazon.Web.Models;
using Amazon.Web.ViewModels.Account;

namespace Amazon.Web.ViewModels
{
    public class ProfileViewModel
    {
        public string Login { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }

        public string EBayEmail { get; set; }
        public string EBayName { get; set; }

        public string Score { get; set; }
        public string Star { get; set; }


        public PasswordViewModel Password { get; set; }

        public ProfileViewModel()
        {
            Password = new PasswordViewModel();
        }
    }
}