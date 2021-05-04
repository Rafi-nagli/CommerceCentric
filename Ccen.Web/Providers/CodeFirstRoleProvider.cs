using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Amazon.Core.Contracts.Factories;
using Amazon.DAL;
using Amazon.DTO;

namespace Amazon.Web.Providers
{
    public class CodeFirstRoleProvider : RoleProvider
    {
        private IList<RoleDTO> _roleList = new List<RoleDTO>();
        private IDbFactory _dbFactory = new DbFactory();

        public CodeFirstRoleProvider()
        {
            Console.WriteLine("CodeFirstRoleProvider");

            using (var db = _dbFactory.GetRWDb())
            {
                _roleList = db.Roles.GetAllAsDto().ToList();
            }
        }

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


        public override bool IsUserInRole(string username, string roleName)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(roleName))
            {
                return false;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(username);
                if (user == null)
                    return false;

                var role = _roleList.FirstOrDefault(r => r.Id == user.RoleId);
                if (role != null && role.Name == roleName)
                    return true;

                return false;
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var user = unitOfWork.Users.GetByName(username);
                if (user != null)
                {
                    var role = _roleList.FirstOrDefault(r => r.Id == user.RoleId);
                    if (role != null)
                        return new string[] { role.Name };
                }
                return new string[] {};
            }
        }

        public override void CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                using (var unitOfWork = new UnitOfWork(null))
                {
                    unitOfWork.Roles.AddByName(roleName);
                }
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException();
        }

        public override bool RoleExists(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return false;
            }
            var role = _roleList.FirstOrDefault(r => r.Name == roleName);
            return role != null;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return null;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                var role = _roleList.FirstOrDefault(r => r.Name == roleName);
                if (role != null)
                {
                    var users = unitOfWork.Users.GetAllAsDto().Where(u => u.RoleId == role.Id).ToList();
                    return users.Select(u => u.Name).ToArray();
                }
                return new string[] {};
            }
        }

        public override string[] GetAllRoles()
        {
            return _roleList.Select(r => r.Name).ToArray();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (string.IsNullOrEmpty(roleName) || string.IsNullOrEmpty(usernameToMatch))
            {
                return null;
            }
            using (var unitOfWork = new UnitOfWork(null))
            {
                return unitOfWork.Roles.GetUserNames(roleName, usernameToMatch);
            }
        }
    }
}