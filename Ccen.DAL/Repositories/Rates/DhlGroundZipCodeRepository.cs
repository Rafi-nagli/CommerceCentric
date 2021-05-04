using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Rates;
using Amazon.DTO.Orders;
using Amazon.DTO.Rates;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class DhlGroundZipCodeRepository : Repository<DhlGroundZipCode>, IDhlGroundZipCodeRepository
    {
        public DhlGroundZipCodeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ZipCodeZoneDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<ZipCodeZoneDTO> AsDto(IQueryable<DhlGroundZipCode> query)
        {
            return query.Select(b => new ZipCodeZoneDTO()
            {
                Id = b.Id,
                Zip = b.Zip,
            });
        }
    }
}
