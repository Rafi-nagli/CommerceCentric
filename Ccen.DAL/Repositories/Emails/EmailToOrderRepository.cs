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
    public class EmailToOrderRepository : Repository<EmailToOrder>, IEmailToOrderRepository
    {
        public EmailToOrderRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public void Insert(IList<EmailToOrderDTO> emailToOrders)
        {
            foreach (var item in emailToOrders)
            {
                Add(new EmailToOrder()
                {
                    EmailId = item.EmailId,
                    OrderId = item.OrderId,
                    CreateDate = item.CreateDate,
                });
            }
            unitOfWork.Commit();
        }
    }
}
