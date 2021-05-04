using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.Web.Models;
using Amazon.Web.Providers;
using Amazon.Web.ViewModels.Results;

namespace Amazon.Web.ViewModels.Companies
{
    public class UserViewModel
    {
        public long? Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int RoleId { get; set; }

        public DateTime? CreateDate { get; set; }

        public bool IsLockedOut { get; set; }

        public string RoleName
        {
            get { return AccessManager.GetRoleName(RoleId); }
        }


        public UserViewModel()
        {
            
        }

        public UserViewModel(UserDTO user)
        {
            Id = user.Id;

            UserName = user.Name;
            FirstName = user.FirstName;
            LastName = user.LastName;

            Email = user.Email;
            CreateDate = user.CreateDate;

            IsLockedOut = user.IsLockedOut;

            RoleId = user.RoleId;
        }

        public static IList<UserViewModel> GetAll(IUnitOfWork db)
        {
            return db.Users.GetAllAsDto()
                .Where(u => !u.Deleted)
                .ToList()
                .Select(u => new UserViewModel(u))
                .OrderBy(u => u.UserName)
                .ToList();
        }

        public IList<MessageString> Validate(IUnitOfWork db)
        {
            var results = new List<MessageString>();
            if (!Id.HasValue)
            {
                var userByName = db.Users.GetAllAsDto().FirstOrDefault(u => u.Name == UserName);
                if (userByName != null)
                    results.Add(MessageString.Error("", "User with that name already exists"));
            }
            return results;
        }

        public IList<MessageString> Apply(IUnitOfWork db,
            long companyId,
            DateTime? when,
            long? by)
        {
            var results = new List<MessageString>();

            User user = null;
            if (Id.HasValue)
                user = db.Users.GetById(Id.Value);
            if (user == null)
            {
                user = new User()
                {
                    Name = UserName,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    Password = Crypto.HashPassword(Password),
                    IsLockedOut = IsLockedOut,

                    CompanyId = companyId,
                    RoleId = RoleId,

                    IsApproved = true,
                    CreateDate = DateTime.UtcNow,
                    LastPasswordChangedDate = DateTime.UtcNow,
                    PasswordFailuresSinceLastSuccess = 0,
                    LastLoginDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow,
                    LastLockoutDate = DateTime.UtcNow,
                    LastPasswordFailureDate = DateTime.UtcNow,

                    Deleted = false,
                    DeleteDate = null
                };
                db.Users.Add(user);
            }
            else
            {
                user.Email = Email;
                user.FirstName = FirstName;
                user.LastName = LastName;

                user.IsLockedOut = IsLockedOut;

                user.RoleId = RoleId;

                if (!String.IsNullOrEmpty(Password))
                    user.Password = Crypto.HashPassword(Password);
            }

            db.Commit();

            return results;
        }

        //public void UnlockUser(IUnitOfWork db,
        //    long userId)
        //{
        //    var user = db.Users.GetById(userId);
        //    if (user != null)
        //    {
        //        user.IsLockedOut = false;
        //        user.PasswordFailuresSinceLastSuccess = 0;

        //        db.Commit();
        //    }
        //}

        public IList<MessageString> Delete(IUnitOfWork db, 
            DateTime? when,
            long? by)
        {
            var results = new List<MessageString>();

            if (Id == by)
            {
                results.Add(MessageString.From("", "You can't delete yourself"));
                return results;
            }

            if (Id.HasValue)
            {
                var user = db.Users.GetById(Id.Value);
                if (user != null)
                {
                    user.DeleteDate = when;
                    user.Deleted = true;
                    db.Commit();
                }
            }

            return results;
        }
    }
}