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
    public class ShipmentProviderRepository : Repository<ShipmentProvider>, IShipmentProviderRepository
    {
        public ShipmentProviderRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ShipmentProviderDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<ShipmentProvider>().Where(p => p.IsActive));
        }

        public IList<ShipmentProviderDTO> GetByCompanyId(long companyId)
        {
            return GetAllAsDto().Where(p => p.CompanyId == companyId && p.IsActive).ToList();
        }

        public void UpdateBalance(long providerId, decimal newBalance, DateTime? when)
        {
            var company = Get(providerId);
            if (company != null)
            {
                company.Balance = newBalance;
                company.BalanceUpdateDate = when;
                unitOfWork.Commit();
            }
        }

        private IQueryable<ShipmentProviderDTO> AsDto(IQueryable<ShipmentProvider> query)
        {
            return query.Select(i => new ShipmentProviderDTO()
            {
                Id = i.Id,
                CompanyId = i.CompanyId,
                Type = i.Type,
                Name = i.Name,
                ShortName = i.ShortName ?? i.Name,
                Key1 = i.Key1,
                Key2 = i.Key2,
                Key3 = i.Key3,
                Key4 = i.Key4,
                UserName = i.UserName,
                Password = i.Password,

                EndPointUrl = i.EndPointUrl,

                Balance = i.Balance,
                BalanceUpdateDate = i.BalanceUpdateDate,

                IsActive = i.IsActive,
                CreateDate = i.CreateDate
            });
        }
    }
}
