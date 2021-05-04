using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts
{
    public interface IWeightService
    {
        double DefaultWeightForNoWeigth { get; }
        double AdjustWeight(double weight, int itemCount);
        double ComposeOrderWeight(IList<ListingOrderDTO> items);

        ItemPackageDTO ComposePackageSize(IList<DTOOrderItem> items);
        ItemPackageDTO ComposePackageSize(IList<ListingOrderDTO> items);
    }
}
