using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        public User GetByName(string username)
        {
            return unitOfWork.GetSet<User>().Include("Role").FirstOrDefault(u => u.Name == username && !u.Deleted);
        }

        public UserDTO GetByEmail(string email)
        {
            var query = unitOfWork.GetSet<User>().Where(u => u.Email == email && !u.Deleted);
            return AsDto(query).FirstOrDefault();
        }

        public User GetById(long id)
        {
            return unitOfWork.GetSet<User>().FirstOrDefault(u => u.Id == id);
        }

        public int GetCount()
        {
            return unitOfWork.GetSet<User>().Count(u => !u.Deleted);
        }

        public int GetCountByLastActivityDate(DateTime dateActive)
        {
            return unitOfWork.GetSet<User>().Count(u => u.LastActivityDate > dateActive && !u.Deleted);
        }

        public int GetCountByName(string username)
        {
            return unitOfWork.GetSet<User>().Count(u => u.Name == username && !u.Deleted);
        }

        public int GetCountByEmail(string email)
        {
            return unitOfWork.GetSet<User>().Count(u => u.Email == email && !u.Deleted);
        }

        public List<UserDTO> GetAllUsers(int pageIndex, int pageSize)
        {
            var query = unitOfWork.GetSet<User>()
                .Where(u => !u.Deleted)
                .OrderBy(u => u.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            return AsDto(query).ToList();
        }

        public List<UserDTO> GetListByName(string username, int pageIndex, int pageSize)
        {
            var query = unitOfWork.GetSet<User>()
                .Where(u => u.Name == username && !u.Deleted)
                .OrderBy(u => u.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
            return AsDto(query).ToList();
        }

        public List<UserDTO> GetListByEmail(string email, int pageIndex, int pageSize)
        {
            var query = unitOfWork.GetSet<User>()
                .Where(u => u.Email == email && !u.Deleted)
                .OrderBy(u => u.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
            return AsDto(query).ToList();
        }


        public UserDTO GetByIdAsDto(long id)
        {
            var query = unitOfWork.GetSet<User>().Where(u => u.Id == id);
            return AsDto(query).FirstOrDefault();
        }

        public UserDTO GetByNameAsDto(string username)
        {
            return AsDto(unitOfWork.GetSet<User>().Where(u => u.Name == username && !u.Deleted)).FirstOrDefault();
        }

        public IQueryable<UserDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }


        private IQueryable<UserDTO> AsDto(IQueryable<User> query)
        {
            return query.Select(u => new UserDTO()
            {
                Id = u.Id,
                Name = u.Name,

                RoleId = u.RoleId,

                CompanyId = u.CompanyId,

                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,

                IsApproved = u.IsApproved,
                IsLockedOut = u.IsLockedOut,
                LastActivityDate = u.LastActivityDate,
                LastLockoutDate = u.LastLockoutDate,
                LastLoginDate = u.LastLoginDate,

                PasswordFailuresSinceLastSuccess = u.PasswordFailuresSinceLastSuccess,
                LastPasswordFailureDate = u.LastPasswordFailureDate,
                LastPasswordChangedDate = u.LastPasswordChangedDate,

                IsAcceptedTerms = u.IsAcceptedTerms,

                CreateDate = u.CreateDate,

               
                Deleted = u.Deleted,
                DeleteDate = u.DeleteDate,
            });
        }
    }
}
