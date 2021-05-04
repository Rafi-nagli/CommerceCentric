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
    public class SkyPostalCityCodeRepository : Repository<SkyPostalCityCode>, ISkyPostalCityCodeRepository
    {
        public SkyPostalCityCodeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
