using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Web.Models
{
    public class AccessManager
    {
        private UserDTO _user;
        private RoleDTO _role;

        public const int RoleUserId = 3;

        public const string RoleAdmin = "Admin";
        public const string RoleUser = "User";
        public const string RoleSuperUser = "SuperUser";
        public const string RoleReadonly = "Readonly";
        public const string RoleRestricted = "Restricted";
        public const string RoleClient = "Client";

        public const string AllBaseRole = RoleReadonly + "," + RoleUser + "," + RoleAdmin + "," + RoleSuperUser + "," + RoleClient;
        public const string AllWriteRole = RoleUser + "," + RoleAdmin + "," + RoleSuperUser + "," +RoleClient;
        public const string AllRoles = RoleReadonly + "," + RoleUser + "," + RoleAdmin + "," + RoleSuperUser + "," + RoleRestricted + "," + RoleClient;
        public const string AllAdmins = RoleAdmin + "," + RoleSuperUser;
        public const string AllClients = RoleClient;

        public static string GetRoleName(int id)
        {
            switch (id)
            {
                case 1:
                    return RoleSuperUser;
                case 2:
                    return RoleAdmin;
                case 3:
                    return RoleUser;
                case 4:
                    return RoleReadonly;
                case 5:
                    return RoleRestricted;
                case 6:
                    return RoleClient;
            }
            return "-";
        }


        public static UserDTO User
        {
            get
            {
                if (HttpContext.Current.User != null
                    && HttpContext.Current.User.Identity.IsAuthenticated 
                    && SessionHelper.User == null)
                {
                    var userIdentity = System.Web.Security.Membership.GetUser(HttpContext.Current.User.Identity.Name);
                    if (userIdentity != null)
                    {
                        using (var uow = new UnitOfWork(null))
                        {
                            var user = uow.Users.GetByIdAsDto((long)userIdentity.ProviderUserKey);

                            //Role
                            var role = uow.Roles.GetByIdAsDto(user.RoleId);
                            user.Role = role;
                            
                            //Company
                            var company = uow.Companies.GetByIdWithSettingsAsDto(user.CompanyId);
                            user.Company = company;
                            
                            SessionHelper.User = user;
                        }
                    }
                }
                return SessionHelper.User;
            }
        }

        private static CompanyDTO _defaultCompany;
        public static CompanyDTO DefaultCompany
        {
            get
            {
                if (_defaultCompany == null)
                {
                    //var defaultCompanyName = AppSettings.DefaultCompanyName;
                    using (var uow = new UnitOfWork(null))
                    {
                        //Company
                        _defaultCompany = uow.Companies.GetFirstWithSettingsAsDto();
                    }
                }
                return _defaultCompany;
            }    
        } 

        public static CompanyDTO Company
        {
            get
            {
                var user = User;
                if (user != null)
                {
                    return user.Company;
                }
                return DefaultCompany;
            }
        }

        public static IList<ShipmentProviderDTO> ShipmentProviderInfoList
        {
            get
            {
                var user = User;
                if (user != null)
                {
                    return user.Company.ShipmentProviderInfoList;
                }
                return new List<ShipmentProviderDTO>();
            }
        }

        public static IList<EmailAccountDTO> EmailAccountList
        {
            get
            {
                var user = User;
                if (user != null)
                {
                    return user.Company.EmailAccounts;
                }
                return new List<EmailAccountDTO>();
            }
        }



        public static bool IsAuthenticated
        {
            get
            {
                return User != null;
            }
        }

        public static long? UserId
        {
            get
            {
                var user = User;
                if (user != null)
                    return user.Id;
                return null;
            }
        }

        public static string UserName
        {
            get
            {
                var user = User;
                if (user != null)
                    return user.Name;
                return null;
            }
        }

        public static long? CompanyId
        {
            get
            {
                var user = User;
                if (user != null)
                    return user.CompanyId;
                return null;
            }
        }

        public static string RoleName
        {
            get
            {
                return (User != null && User.Role != null) ? User.Role.Name : null;
            }
        }

        public static bool IsReadonly
        {
            get
            {
                return RoleName == RoleReadonly;
            }
        }

        public static bool IsSuperAdmin
        {
            get { return UserName == "Admin"; }
        }

        public static bool IsAdmin
        {
            get { return RoleName == RoleAdmin || RoleName == RoleSuperUser; }
        }

        public static bool IsRestricted
        {
            get { return RoleName == RoleRestricted; }
        }
        
        public static bool IsClient
        {
            get { return RoleName == RoleClient; }
        }

        public static bool IsFulfilment
        {
            get { return RoleName != RoleClient; }
        }


        public static bool CanViewSystemInfo()
        {
            return UserName == "Admin";
        }

        public static bool CanEditSystemInfo()
        {
            return UserName == "Admin";
        }

        public static bool CanViewDebugInfo()
        {
            return UserName == "Admin";
        }

        public static bool CanViewSyncStatusPanel()
        {
            return RoleName != RoleReadonly && RoleName != RoleRestricted
                && ((HttpContext.Current.Request?.Url?.AbsoluteUri?.Contains("Orders") ?? false)
                    || (HttpContext.Current.Request?.Url?.AbsoluteUri?.Contains("Batch") ?? false));
        }

        public static bool CanViewBalance()
        {
            return RoleName != RoleReadonly && RoleName != RoleRestricted;
        }

        public static bool CanViewNotifyInfo()
        {
            return RoleName != RoleRestricted;
        }

        public static bool CanViewOrders()
        {
            return RoleName != RoleRestricted;
        }

        public static bool CanPrintLabel()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanEditBatch()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanUpgradeOrder()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanEditOrder()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanEditProduct()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanEditPrice()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanEditStyle()
        {
            return RoleName != RoleReadonly && RoleName != RoleRestricted;
        }

        public static bool CanDoStyleOperations()
        {
            return RoleName != RoleRestricted;
        }

        public static bool CanEditVendorOrder()
        {
            return RoleName != RoleReadonly;
        }

        public static bool CanEditScanOrder()
        {
            return RoleName != RoleReadonly;
        }
    }
}
