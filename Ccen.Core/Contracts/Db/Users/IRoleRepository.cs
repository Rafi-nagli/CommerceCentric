using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IRoleRepository : IRepository<Role>
    {
        Role GetByName(string roleName);
        string GetNameById(int roleId);
        void AddByName(string roleName);
        bool Delete(string roleName, bool throwOnPopulatedRole);
        List<Role> GetContainingNamesList(string[] names);
        string[] GetAllNames();
        string[] GetUserNames(string roleName, string usernameToMatch);
        RoleDTO GetByIdAsDto(int id);
        IQueryable<RoleDTO> GetAllAsDto();
    }
}
