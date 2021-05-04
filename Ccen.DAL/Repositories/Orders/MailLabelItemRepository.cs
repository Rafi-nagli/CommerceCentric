using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories
{
    public class MailLabelItemRepository : Repository<MailLabelItem>, IMailLabelItemRepository
    {
        public MailLabelItemRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        

        public IQueryable<MailLabelItemDTO> GetAllAsDto()
        {
            return AsDTO(GetAll());
        }

        public IQueryable<MailLabelItemDTO> AsDTO(IQueryable<MailLabelItem> query)
        {
            return query.Select(mi => new MailLabelItemDTO()
            {
                Id = mi.Id,
                OrderId = mi.OrderId,
                MailLabelInfoId = mi.MailLabelInfoId,
                ItemOrderIdentifier = mi.ItemOrderIdentifier,
                StyleId = mi.StyleId,
                StyleItemId = mi.StyleItemId,
                Quantity = mi.Quantity,

                CreateDate = mi.CreateDate,
                CreatedBy = mi.CreatedBy,
            });
        }
    }
}
