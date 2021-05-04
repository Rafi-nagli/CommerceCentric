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
    public class IBCRateRepository : Repository<IBCRate>, IIBCRateRepository
    {
        public IBCRateRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<IBCRateDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<IBCRateDTO> AsDto(IQueryable<IBCRate> query)
        {
            return query.Select(b => new IBCRateDTO()
            {
                Id = b.Id,

                ServiceType = b.ServiceType,
                CountryCode = b.CountryCode,

                RatePerPiece = b.RatePerPiece,
                RatePerPound = b.RatePerPound
            });
        }
    }
}
