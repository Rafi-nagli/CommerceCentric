using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Events;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Events;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class SaleEventFeedRepository : Repository<SaleEventFeed>, ISaleEventFeedRepository
    {
        public SaleEventFeedRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IQueryable<SaleEventFeedDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<SaleEventFeed>()
                select new SaleEventFeedDTO()
                {
                    Id = se.Id,
                    Filename = se.Filename,
                    SaleEventId = se.SaleEventId,
                    CreateDate = se.CreateDate,
                    CreatedBy = se.CreatedBy,
                };

            return query;
        }
    }
}
