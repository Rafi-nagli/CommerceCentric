using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Entities.Users;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.DAL.Repositories
{
    public class AddressProviderRepository : Repository<AddressProvider>, IAddressProviderRepository
    {
        public AddressProviderRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<AddressProviderDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<AddressProvider>());
        }

        public IList<AddressProviderDTO> GetByCompanyId(long companyId)
        {
            return GetAllAsDto().Where(c => c.CompanyId == companyId && c.IsActive).ToList();
        }

        private IQueryable<AddressProviderDTO> AsDto(IQueryable<AddressProvider> query)
        {
            return query.Select(i => new AddressProviderDTO()
            {
                Id = i.Id,
                CompanyId = i.CompanyId,
                Type = i.Type,

                EndPoint = i.EndPoint,
                Key1 = i.Key1,
                Key2 = i.Key2,
                Key3 = i.Key3,
                UserName = i.UserName,
                Password = i.Password,
                
                IsActive = i.IsActive,
                CreateDate = i.CreateDate
            });
        }
    }
}
