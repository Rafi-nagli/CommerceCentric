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
using Amazon.Core.Entities.DropShippers;
using Amazon.DTO.DropShippers;
using Amazon.DTO.CustomReports;
using Amazon.Core.Entities.CustomReports;

namespace Amazon.DAL.Repositories
{
    public class CustomReportRepository : Repository<CustomReport>, ICustomReportRepository
    {
        public CustomReportRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomReportDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<CustomReport>()
                select new CustomReportDTO()
                {
                    Id = se.Id,
                    InMenu = se.InMenu,
                    Name = se.Name,

                    CreateDate = se.CreateDate,
                    CreatedBy = se.CreatedBy
                };

            return query;
        }
    }
}
