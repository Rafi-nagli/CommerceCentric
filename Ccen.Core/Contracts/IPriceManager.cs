using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts
{
    public interface IPriceManager
    {
        void LogListingPrice(IUnitOfWork db,
            PriceChangeSourceType type,
            long listingId,
            string sku,
            decimal newPrice,
            decimal? oldPrice,
            DateTime when,
            long? by);
    }
}
