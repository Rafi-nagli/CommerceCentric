using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IListingFBAEstFeeRepository : IRepository<ListingFBAEstFee>
    {
        IEnumerable<string> ProcessRemoved(IEnumerable<string> skuList);
        void CreateOrUpdate(ListingFBAEstFeeDTO listing, DateTime? when);
        IList<ListingFBAEstFeeDTO> GetAllActual();
    }
}
