using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Listings;

namespace Amazon.Core.Contracts.Db.Listings
{
    public interface IBuyBoxQuantityRepository : IRepository<BuyBoxQuantity> 
    {
    }
}
