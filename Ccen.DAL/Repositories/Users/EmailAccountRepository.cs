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
    public class EmailAccountRepository : Repository<EmailAccount>, IEmailAccountRepository
    {
        public EmailAccountRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<EmailAccountDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<EmailAccount>());
        }

        public IList<EmailAccountDTO> GetByCompanyId(long companyId)
        {
            return GetAllAsDto().Where(c => c.CompanyId == companyId).ToList();
        }

        private IQueryable<EmailAccountDTO> AsDto(IQueryable<EmailAccount> query)
        {
            return query.Select(i => new EmailAccountDTO()
            {
                Id = i.Id,
                CompanyId = i.CompanyId,

                Type = i.Type,

                ServerHost = i.ServerHost,
                ServerPort = i.ServerPort,

                UserName = i.UserName,
                Password = i.Password,

                DisplayName = i.DisplayName,
                FromEmail = i.FromEmail,

                AcceptingToAddresses = i.AcceptingToAddresses,

                AttachmentDirectory = i.AttachmentDirectory,
                AttachmentFolderRelativeUrl = i.AttachmentFolderRelativeUrl,

                UseSsl = i.UseSsl,

                CreateDate = i.CreateDate
            });
        }
    }
}
