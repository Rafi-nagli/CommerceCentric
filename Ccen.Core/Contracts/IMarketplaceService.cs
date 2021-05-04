using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IMarketplaceService
    {
        IList<MarketplaceDTO> GetAll();
        IList<MarketplaceDTO> GetAllWithVirtual();
    }
}
