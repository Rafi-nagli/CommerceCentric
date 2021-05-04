using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IListingFBAInvRepository : IRepository<ListingFBAInv>
    {
        IEnumerable<string> ProcessRemoved(IEnumerable<string> skuList);
        void CreateOrUpdate(ListingFBAInvDTO listing, DateTime? when);
        IList<ListingFBAInvDTO> GetAllActual();
    }
}
