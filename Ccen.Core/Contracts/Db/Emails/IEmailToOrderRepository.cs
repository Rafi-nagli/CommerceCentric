using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Emails;
using Amazon.DTO.Emails;

namespace Amazon.Core.Contracts.Db.Emails
{
    public interface IEmailToOrderRepository : IRepository<EmailToOrder>
    {
        void Insert(IList<EmailToOrderDTO> emailToOrders);
    }
}
