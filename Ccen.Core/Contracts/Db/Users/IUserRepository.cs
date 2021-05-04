using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByName(string username);
        UserDTO GetByEmail(string email);
        User GetById(long id);
        int GetCount();
        List<UserDTO> GetAllUsers(int pageIndex, int pageSize);
        int GetCountByLastActivityDate(DateTime dateActive);
        int GetCountByName(string usernameToMatch);
        int GetCountByEmail(string emailToMatch);
        List<UserDTO> GetListByName(string usernameToMatch, int pageIndex, int pageSize);
        List<UserDTO> GetListByEmail(string emailToMatch, int pageIndex, int pageSize);

        IQueryable<UserDTO> GetAllAsDto();

        UserDTO GetByIdAsDto(long id);
        UserDTO GetByNameAsDto(string username);
    }
}
