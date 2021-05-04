using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IAddressService
    {
        string DefaultName { get; }
        string GetFullname(MarketType market);

        IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            AddressDTO inputAddress);

        IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            AddressDTO inputAddress,
            AddressProviderType[] applyAddressProviderTypes);

        AddressDTO ReturnAddress { get; }
        //AddressDTO GetGrouponReturnAddress();
        AddressDTO GetReturnAddressByType(CompanyAddressTypes addressType);

        bool IsMine(AddressDTO address);

        string GetEmailSignature(MarketType market);
    }
}
