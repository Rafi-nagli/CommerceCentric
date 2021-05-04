using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Db
{
    public interface IEmailAccountRepository : IRepository<EmailAccount>
    {
        IQueryable<EmailAccountDTO> GetAllAsDto();
        IList<EmailAccountDTO> GetByCompanyId(long companyId);
    }
}
