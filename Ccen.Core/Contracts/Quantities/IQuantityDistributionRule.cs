using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Quantities
{
    public interface IQuantityDistributionRule
    {
        int Apply(StyleEntireDto style, 
            MarketType market, 
            string marketplaceId,
            IList<ListingQuantityDTO> listings,
            int sourceRemainingQty, 
            int appliedQty);
    }
}
