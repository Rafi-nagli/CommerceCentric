using Amazon.Core.Models;
using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ICompanyAddressService
    {
        AddressDTO GetPickupAddress(MarketIdentifier market);
        AddressDTO GetReturnAddress(MarketIdentifier makret);
    }
}
