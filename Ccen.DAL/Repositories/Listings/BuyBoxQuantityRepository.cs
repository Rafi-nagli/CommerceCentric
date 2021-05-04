using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db.Listings;
using Amazon.Core.Entities.Listings;

namespace Amazon.DAL.Repositories.Listings
{
    public class BuyBoxQuantityRepository : Repository<BuyBoxQuantity>, IBuyBoxQuantityRepository
    {
        public BuyBoxQuantityRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
