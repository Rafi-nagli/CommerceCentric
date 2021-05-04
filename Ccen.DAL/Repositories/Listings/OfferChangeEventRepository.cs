using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Listings;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class OfferChangeEventRepository : Repository<OfferChangeEvent>, IOfferChangeEventRepository
    {
        public OfferChangeEventRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
