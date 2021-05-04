using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Entities.Users;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.DAL.Repositories
{
    public class SQSAccountRepository : Repository<SQSAccount>, ISQSAccountRepository
    {
        public SQSAccountRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SQSAccountDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<SQSAccount>());
        }

        public IList<SQSAccountDTO> GetByCompanyId(long companyId)
        {
            return GetAllAsDto().Where(c => c.CompanyId == companyId).ToList();
        }

        private IQueryable<SQSAccountDTO> AsDto(IQueryable<SQSAccount> query)
        {
            return query.Select(i => new SQSAccountDTO()
            {
                Id = i.Id,
                CompanyId = i.CompanyId,

                Type = i.Type,

                EndPointUrl = i.EndPointUrl,
                AccessKey = i.AccessKey,
                SecretKey = i.SecretKey,

                CreateDate = i.CreateDate
            });
        }
    }
}
