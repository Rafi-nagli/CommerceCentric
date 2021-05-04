using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemSaleToListingRepository : IRepository<StyleItemSaleToListing>
    {
        IQueryable<StyleItemSaleToListingDTO> GetAllAsDto();

        IQueryable<ViewListingSaleDTO> GetAllListingSaleAsDTO();

        IList<long> UpdateForSale(long saleId,
            IList<StyleItemSaleToListingDTO> saleToListings,
            DateTime when,
            long? by);
    }
}
