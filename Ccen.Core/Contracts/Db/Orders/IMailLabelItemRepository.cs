using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IMailLabelItemRepository : IRepository<MailLabelItem>
    {
        IQueryable<MailLabelItemDTO> GetAllAsDto();
    }
}
