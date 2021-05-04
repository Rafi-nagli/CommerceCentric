using System.Collections.Generic;
using System.Linq;

using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public Role GetByName(string roleName)
        {
            return unitOfWork.GetSet<Role>().FirstOrDefault(r => r.Name == roleName);
        }

        public string GetNameById(int roleId)
        {
            var role = unitOfWork.GetSet<Role>().FirstOrDefault(r => r.Id == roleId);
            return role != null ? role.Name : string.Empty;
        }

        public void AddByName(string roleName)
        {

            var role = unitOfWork.GetSet<Role>().FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                var newRole = new Role
                {
                    Name = roleName
                };
                unitOfWork.GetSet<Role>().Add(newRole);
                unitOfWork.Commit();
            }
        }

        public bool Delete(string roleName, bool throwOnPopulatedRole)
        {
            var role = unitOfWork.GetSet<Role>().FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                return false;
            }
            if (throwOnPopulatedRole)
            {
                if (role.Users.Any())
                {
                    return false;
                }
            }
            else
            {
                role.Users.Clear();
            }
            unitOfWork.GetSet<Role>().Remove(role);
            unitOfWork.Commit();
            return true;
        }

        public List<Role> GetContainingNamesList(string[] names)
        {
            return unitOfWork.GetSet<Role>().Where(r => names.Contains(r.Name)).ToList();
        }

        public string[] GetAllNames()
        {
            return unitOfWork.GetSet<Role>().Select(r => r.Name).ToArray();
        }

        public string[] GetUserNames(string roleName, string usernameToMatch)
        {
            var users = unitOfWork.GetSet<User>().Include("Role").Where(u => u.Role.Name == roleName).ToList();
            return users.Where(u => u.Name.Contains(usernameToMatch) && !u.Deleted).Select(u => u.Name).ToArray();
        }

        public IQueryable<RoleDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public RoleDTO GetByIdAsDto(int id)
        {
            var query = unitOfWork.GetSet<Role>().Where(r => r.Id == id);
            return AsDto(query).FirstOrDefault();
        }

        public IQueryable<RoleDTO> AsDto(IQueryable<Role> query)
        {
            return query.Select(r => new RoleDTO()
            {
                Id = r.Id,
                Name = r.Name,
                Title = r.Title,
                SortOrder = r.SortOrder,
                IsActive = r.IsActive,
            });
        }
    }
}
