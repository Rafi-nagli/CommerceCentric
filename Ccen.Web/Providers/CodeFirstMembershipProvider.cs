using System;
using System.Web.Security;

using Amazon.Core.Entities;
using Amazon.DAL;

namespace Amazon.Web.Providers
{
    public class CodeFirstMembershipProvider : MembershipProvider
    {
        #region Properties

        public override string ApplicationName
        {
            get
            {
                return GetType().Assembly.GetName().Name;
            }
            set
            {
                ApplicationName = GetType().Assembly.GetName().Name;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return 5; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        public override int PasswordAttemptWindow
        {
            get { return 0; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return string.Empty; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        #endregion

        #region Functions

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
            bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (string.IsNullOrEmpty(username))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (string.IsNullOrEmpty(password))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (string.IsNullOrEmpty(email))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            var hashedPassword = Crypto.HashPassword(password);
            if (hashedPassword.Length > 128)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            using (var unitOfWork = new UnitOfWork(null))
            {
                if (unitOfWork.Users.GetByName(username) != null)
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                    return null;
                }
                if (unitOfWork.Users.GetByEmail(email) != null)
                {
                    status = MembershipCreateStatus.DuplicateEmail;
                    return null;
                }
                var newUser = new User
                {
                    Name = username,
                    Password = hashedPassword,
                    IsApproved = isApproved,
                    Email = email,
                    CreateDate = DateTime.UtcNow,
                    LastPasswordChangedDate = DateTime.UtcNow,
                    PasswordFailuresSinceLastSuccess = 0,
                    LastLoginDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow,
                    LastLockoutDate = DateTime.UtcNow,
                    IsLockedOut = false,
                    LastPasswordFailureDate = DateTime.UtcNow,

                    Deleted = false,
                    DeleteDate = null
                };
                unitOfWork.Users.Add(newUser);
                unitOfWork.Commit();

                status = MembershipCreateStatus.Success;
                return new MembershipUser(Membership.Provider.Name, newUser.Name, newUser.Id, email,
                                              null, null, newUser.IsApproved, newUser.IsLockedOut,
                                              newUser.CreateDate.Value, newUser.LastLoginDate.Value,
                                              newUser.LastActivityDate.Value, newUser.LastPasswordChangedDate.Value,
                                              newUser.LastLockoutDate.Value);
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                return false;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(username);
                if (user == null)
                {
                    return false;
                }
                var hashedPassword = user.Password;
                var verificationSucceeded = (hashedPassword != null &&
                                             Crypto.VerifyHashedPassword(hashedPassword, oldPassword));
                if (verificationSucceeded)
                {
                    user.PasswordFailuresSinceLastSuccess = 0;
                }
                else
                {
                    var failures = user.PasswordFailuresSinceLastSuccess;
                    if (failures < MaxInvalidPasswordAttempts)
                    {
                        user.PasswordFailuresSinceLastSuccess += 1;
                        user.LastPasswordFailureDate = DateTime.UtcNow;
                    }
                    else if (failures >= MaxInvalidPasswordAttempts)
                    {
                        user.LastPasswordFailureDate = DateTime.UtcNow;
                        user.LastLockoutDate = DateTime.UtcNow;
                        user.IsLockedOut = true;
                    }
                    unitOfWork.Commit();
                    return false;
                }
                var newHashedPassword = Crypto.HashPassword(newPassword);
                if (newHashedPassword.Length > 128)
                {
                    return false;
                }
                user.Password = newHashedPassword;
                user.LastPasswordChangedDate = DateTime.UtcNow;
                unitOfWork.Commit();
                return true;
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(username);
                if (user == null)
                {
                    return false;
                }
                if (!user.IsApproved)
                {
                    throw new Exception("The user account is not approved.");
                }
                if (user.IsLockedOut)
                {
                    throw new Exception("The user account is locked out.");
                }
                var hashedPassword = user.Password;
                var verificationSucceeded = (hashedPassword != null && Crypto.VerifyHashedPassword(hashedPassword, password));
                if (verificationSucceeded)
                {
                    user.PasswordFailuresSinceLastSuccess = 0;
                    user.LastLoginDate = DateTime.UtcNow;
                    user.LastActivityDate = DateTime.UtcNow;
                }
                else
                {
                    var failures = user.PasswordFailuresSinceLastSuccess;
                    if (failures < MaxInvalidPasswordAttempts)
                    {
                        user.PasswordFailuresSinceLastSuccess += 1;
                        user.LastPasswordFailureDate = DateTime.UtcNow;
                    }
                    else if (failures >= MaxInvalidPasswordAttempts)
                    {
                        user.LastPasswordFailureDate = DateTime.UtcNow;
                        user.LastLockoutDate = DateTime.UtcNow;
                        user.IsLockedOut = true;
                    }
                }
                unitOfWork.Commit();
                return verificationSucceeded;
            }
        }

        public override bool UnlockUser(string userName)
        {
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(userName);
                if (user == null)
                {
                    return false;
                }
                user.IsLockedOut = false;
                user.PasswordFailuresSinceLastSuccess = 0;
                unitOfWork.Commit();
                return true;
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var key = providerUserKey is long ? (long)providerUserKey : 0;
            if (key == 0)
            {
                return null;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetById(key);
                if (user == null)
                {
                    return null;
                }
                if (!user.IsApproved)
                {
                    return null;
                }
                if (user.IsLockedOut)
                {
                    return null;
                }

                if (userIsOnline)
                {
                    user.LastActivityDate = DateTime.UtcNow;
                    unitOfWork.Commit();
                }
                return new MembershipUser(Membership.Provider.Name, user.Name, user.Id, user.Email, null, null,
                    user.IsApproved, user.IsLockedOut, user.CreateDate.Value, user.LastLoginDate.Value,
                    user.LastActivityDate.Value, user.LastPasswordChangedDate.Value, user.LastLockoutDate.Value);
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(username);
                if (user == null)
                {
                    return null;
                }
                if (!user.IsApproved)
                {
                    return null;
                }
                if (user.IsLockedOut)
                {
                    return null;
                }

                if (userIsOnline)
                {
                    user.LastActivityDate = DateTime.UtcNow;
                    unitOfWork.Commit();
                }
                return new MembershipUser(Membership.Provider.Name, user.Name, user.Id, user.Email, null, null,
                    user.IsApproved, user.IsLockedOut, user.CreateDate.Value, user.LastLoginDate.Value,
                    user.LastActivityDate.Value, user.LastPasswordChangedDate.Value, user.LastLockoutDate.Value);
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByEmail(email);
                return user != null
                    ? user.Name
                    : string.Empty;
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(username);
                if (user == null)
                {
                    return false;
                }
                user.Deleted = true;
                user.DeleteDate = DateTime.UtcNow;
                unitOfWork.Commit();
                return true;
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            using (var unitOfWork = new UnitOfWork(null))
            {
                totalRecords = unitOfWork.Users.GetCount();
                var users = unitOfWork.Users.GetAllUsers(pageIndex, pageSize);
                foreach (var user in users)
                {
                    membershipUsers.Add(new MembershipUser(
                        Membership.Provider.Name,
                            user.Name,
                            user.Id,
                            user.Email,
                            null,
                            null,
                            user.IsApproved,
                            user.IsLockedOut,
                            user.CreateDate.Value,
                            user.LastLoginDate.Value,
                            user.LastActivityDate.Value,
                            user.LastPasswordChangedDate.Value,
                            user.LastLockoutDate.Value));
                }
            }
            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            var dateActive = DateTime.UtcNow.Subtract(
                 TimeSpan.FromMinutes(Convert.ToDouble(Membership.UserIsOnlineTimeWindow)));
            using (var unitOfWork = new UnitOfWork(null))
            {
                return unitOfWork.Users.GetCountByLastActivityDate(dateActive);
            }
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            using (var unitOfWork = new UnitOfWork(null))
            {
                totalRecords = unitOfWork.Users.GetCountByName(usernameToMatch);
                var users = unitOfWork.Users.GetListByName(usernameToMatch, pageIndex, pageSize);
                foreach (var user in users)
                {
                    membershipUsers.Add(new MembershipUser(
                        Membership.Provider.Name,
                        user.Name,
                        user.Id,
                        user.Email,
                        null,
                        null,
                        user.IsApproved,
                        user.IsLockedOut,
                        user.CreateDate.Value,
                        user.LastLoginDate.Value,
                        user.LastActivityDate.Value,
                        user.LastPasswordChangedDate.Value,
                        user.LastLockoutDate.Value));
                }
            }
            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            using (var unitOfWork = new UnitOfWork(null))
            {
                totalRecords = unitOfWork.Users.GetCountByEmail(emailToMatch);
                var users = unitOfWork.Users.GetListByEmail(emailToMatch, pageIndex, pageSize);
                foreach (var user in users)
                {
                    membershipUsers.Add(new MembershipUser(
                        Membership.Provider.Name,
                        user.Name,
                        user.Id,
                        user.Email,
                        null,
                        null,
                        user.IsApproved,
                        user.IsLockedOut,
                        user.CreateDate.Value,
                        user.LastLoginDate.Value,
                        user.LastActivityDate.Value,
                        user.LastPasswordChangedDate.Value,
                        user.LastLockoutDate.Value));
                }
            }
            return membershipUsers;
        }

        #endregion

        #region Not Supported

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }
        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException("Consider using methods from WebSecurity module.");
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }
        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException("Consider using methods from WebSecurity module.");
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException("Consider using methods from WebSecurity module.");
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}