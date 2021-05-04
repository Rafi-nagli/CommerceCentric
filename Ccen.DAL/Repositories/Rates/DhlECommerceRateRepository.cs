using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories.Orders
{
    public class DhlECommerceRateRepository : Repository<DhlECommerceRate>, IDhlECommerceRateRepository
    {
        public DhlECommerceRateRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<DhlECommerceRateDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<DhlECommerceRateDTO> AsDto(IQueryable<DhlECommerceRate> query)
        {
            return query.Select(b => new DhlECommerceRateDTO()
            {
                Id = b.Id,

                ServiceType = b.ServiceType,
                CountryCode = b.CountryCode,
                Zone = b.Zone,
                Rate = b.Rate,
                Weight = b.Weight,
            });
        }
    }
}
