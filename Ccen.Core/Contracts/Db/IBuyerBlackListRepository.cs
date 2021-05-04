using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IBuyerBlackListRepository : IRepository<BuyerBlackList>
    {
        IList<BuyerBlackListDto> GetAllExtendedAsDto();
        BuyerBlackListDto GetExtendedByIdAsDto(long id);
        IList<BuyerBlackListDto> GetSimular(DTOMarketOrder order);
    }
}
