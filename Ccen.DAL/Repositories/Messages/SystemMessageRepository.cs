using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.Core;
using Amazon.DTO.Orders;
using Amazon.Core.Contracts.Db.Orders;

namespace Amazon.DAL.Repositories
{
    public class SystemMessageRepository : Repository<SystemMessage>, ISystemMessageRepository
    {
        public SystemMessageRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IQueryable<SystemMessageDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        
        private IQueryable<SystemMessageDTO> AsDto(IQueryable<SystemMessage> query)
        {
            return query.Select(n => new SystemMessageDTO()
            {
                Id = n.Id,
                Name = n.Name,
                Tag = n.Tag,
                Data = n.Data,
                Status = n.Status,
                Message = n.Message,
                UpdateDate = n.UpdateDate,
                CreateDate = n.CreateDate
            });
        }
    }
}
