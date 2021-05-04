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
using Amazon.DTO;
using Amazon.DTO.Events;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class SaleEventRepository : Repository<SaleEvent>, ISaleEventRepository
    {
        public SaleEventRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SaleEventDTO> GetAllAsDto()
        {
            return unitOfWork.GetSet<SaleEvent>().Select(f => new SaleEventDTO()
            {
                Id = f.Id,
                Name = f.Name,
                Type = f.Type,
                Status = f.Status,
                Site = f.Site,
                StartDate = f.StartDate,
                EndDate = f.EndDate,
                CutOffDate = f.CutOffDate,

                CreateDate = f.CreateDate,
                CreatedBy = f.CreatedBy,
            });
        }
    }
}
