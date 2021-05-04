using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class DhlRateCodeRepository : Repository<DhlRateCode>, IDhlRateCodeRepository
    {
        public DhlRateCodeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<DhlRateCodeDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<DhlRateCodeDTO> AsDto(IQueryable<DhlRateCode> query)
        {
            return query.Select(b => new DhlRateCodeDTO()
            {
                Id = b.Id,

                Country = b.Country,
                CountryCode = b.CountryCode,
                RateCode = b.RateCode,
            });
        }
    }
}
