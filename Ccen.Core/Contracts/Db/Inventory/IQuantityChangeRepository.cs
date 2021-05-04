using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Inventory;


namespace Amazon.Core.Contracts.Db.Listings
{
    public interface IQuantityChangeRepository : IRepository<QuantityChange>
    {
    }
}
