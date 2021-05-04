using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class RateByCountryRepository : Repository<RateByCountry>, IRateByCountryRepository
    {
        public RateByCountryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<RateByCountryDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }
        
        private IQueryable<RateByCountryDTO> AsDto(IQueryable<RateByCountry> query)
        {
            return query.Select(b => new RateByCountryDTO()
            {
                Id = b.Id,
                
                Country = b.Country,
                PackageType = b.PackageType,
                Weight = b.Weight,

                Cost = b.Cost,
                UpdateDate = b.UpdateDate,
            });
        }
    }
}
