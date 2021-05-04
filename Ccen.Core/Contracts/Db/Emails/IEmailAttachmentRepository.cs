using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Emails;
using Amazon.DTO.Emails;

namespace Amazon.Core.Contracts.Db.Emails
{
    public interface IEmailAttachmentRepository : IRepository<EmailAttachment>
    {
        void Insert(EmailAttachmentDTO attachment);
        IQueryable<EmailAttachmentDTO> GetAllAsDto();
    }
}
