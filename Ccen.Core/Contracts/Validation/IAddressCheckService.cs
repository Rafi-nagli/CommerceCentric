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
    public interface IAddressCheckService
    {
        AddressProviderType Type { get; }
        CheckResult<AddressDTO> CheckAddress(CallSource callSource, AddressDTO address);
    }
}
