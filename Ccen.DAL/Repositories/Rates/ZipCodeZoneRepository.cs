using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class ZipCodeZoneRepository : Repository<ZipCodeZone>, IZipCodeZoneRepository
    {
        public ZipCodeZoneRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ZipCodeZoneRangeDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<ZipCodeZoneRangeDTO> AsDto(IQueryable<ZipCodeZone> query)
        {
            return query.Select(b => new ZipCodeZoneRangeDTO()
            {
                Id = b.Id,
                RangeStart = b.RangeStart,
                RangeEnd = b.RangeEnd,
                Zone = b.Zone,
            });
        }
    }
}
