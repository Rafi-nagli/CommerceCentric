using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts
{
    public interface IMarketFactory
    {
        IMarketApi GetApi(long companyId, MarketType market, string marketplaceId);
    }
}
