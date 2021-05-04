using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts
{
    public interface ICompanyService
    {
        IList<ShipmentProviderDTO> GetShipmentProviders(long companyId);
        EmailAccountDTO GetImapAccount(long companyId);
        EmailAccountDTO GetSmtpAccount(long companyId);
    }
}
