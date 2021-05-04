using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IGiftService
    {
        bool ApplyGifts(DTOOrder order, IList<ListingOrderDTO> items);
    }
}
