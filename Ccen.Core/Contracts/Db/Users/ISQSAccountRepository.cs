using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Db
{
    public interface ISQSAccountRepository : IRepository<SQSAccount>
    {
        IQueryable<SQSAccountDTO> GetAllAsDto();
        IList<SQSAccountDTO> GetByCompanyId(long companyId);
    }
}
