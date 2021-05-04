using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db.Emails
{
    public interface IEmailRepository : IRepository<Email>
    {
        EmailDTO GetByUid(uint uid, EmailFolders folder);
        void Insert(EmailDTO email);
        EmailDTO GetByMessageID(string messageId);
        void UpdateUID(long emailId, uint newUID);

        IQueryable<EmailOrderDTO> GetAllByOrderId(string orderId);
        IQueryable<EmailOrderDTO> GetAllWithOrder(EmailSearchFilter filters);

        int GetUnansweredEmailCount();
    }
}
