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
    public class CompanyAddressRepository : Repository<CompanyAddress>, ICompanyAddressRepository
    {
        public CompanyAddressRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CompanyAddressDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<CompanyAddress>());
        }

        public IList<CompanyAddressDTO> GetByCompanyId(long companyId)
        {
            return GetAllAsDto().Where(c => c.CompanyId == companyId).ToList();
        }

        private IQueryable<CompanyAddressDTO> AsDto(IQueryable<CompanyAddress> query)
        {
            return query.Select(i => new CompanyAddressDTO()
            {
                Id = i.Id,
                CompanyId = i.CompanyId,
                Type = i.Type,

                Address1 = i.Address1,
                Address2 = i.Address2,
                City = i.City,
                State = i.State,
                Zip = i.Zip,
                ZipAddon = i.ZipAddon,
                Country = i.Country,
                Phone = i.Phone,
                
                CreateDate = i.CreateDate
            });
        }
    }
}
