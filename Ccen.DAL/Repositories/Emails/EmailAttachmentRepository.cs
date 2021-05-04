using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.Db.Emails;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Emails;
using Amazon.Core;
using Amazon.DTO.Emails;

namespace Amazon.DAL.Repositories.Emails
{
    public class EmailAttachmentRepository : Repository<EmailAttachment>, IEmailAttachmentRepository
    {
        public EmailAttachmentRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<EmailAttachmentDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<EmailAttachmentDTO> AsDto(IQueryable<EmailAttachment> query)
        {
            return query.Select(e => new EmailAttachmentDTO()
            {
                Id = e.Id,
                EmailId = e.EmailId,
                FileName = e.FileName,
                RelativePath = e.Path,
                Title = e.Title,
                CreateDate = e.CreateDate,
            });
        }

        public void Insert(EmailAttachmentDTO attachment)
        {
            Add(new EmailAttachment()
            {
                EmailId = attachment.EmailId,
                FileName = attachment.FileName,
                Path = attachment.RelativePath,
                Title = attachment.Title,

                CreateDate = attachment.CreateDate,
            });
            unitOfWork.Commit();
        }
    }
}
