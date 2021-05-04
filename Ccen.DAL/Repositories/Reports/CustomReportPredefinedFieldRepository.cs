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
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.CustomReports;
using Amazon.Core.Entities.CustomReports;

namespace Amazon.DAL.Repositories
{
    public class CustomReportPredefinedFieldRepository : Repository<CustomReportPredefinedField>, ICustomReportPredefinedFieldRepository
    {
        public CustomReportPredefinedFieldRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomReportPredefinedFieldDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<CustomReportPredefinedField>()
                select new CustomReportPredefinedFieldDTO()
                {
                    Id = se.Id,
                    DataType = se.DataType,
                    EntityName = se.EntityName,
                    Name = se.Name,
                    Width = se.Width.HasValue ? se.Width.Value : 100,
                    ColumnName = se.ColumnName,
                    CreateDate = se.CreateDate,
                    Title = se.Title
                };

            return query;
        }
    }
}
