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
    public class CustomReportFilterRepository : Repository<CustomReportFilter>, ICustomReportFilterRepository
    {
        public CustomReportFilterRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<EntityUpdateStatus<long>> BulkUpdateForFilter(long reportId,
            IList<CustomReportFilterDTO> filters,
            DateTime when,

            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();
            
            var dbExistFilters = GetFiltered(l => l.CustomReportId == reportId).ToList();
            var newFields = filters.Where(l => l.Id == 0).ToList();

            foreach (var dbField in dbExistFilters)
            {
                var existField = filters.FirstOrDefault(l => l.Id == dbField.Id);
                if (existField != null)
                {
                    dbField.CustomReportPredefinedFieldId = existField.CustomReportPredefinedFieldId;
                    dbField.Operation = existField.Operation;
                    dbField.Value = existField.Value;
                    dbField.CustomReportId = existField.CustomReportId;
                    results.Add(new EntityUpdateStatus<long>(dbField.Id, UpdateType.Update));
                }
                else
                {
                    Remove(dbField);
                    results.Add(new EntityUpdateStatus<long>(dbField.Id, UpdateType.Removed));
                }
            }

            unitOfWork.Commit();

            foreach (var newField in newFields)
            {
                var dbField = new CustomReportFilter()
                {
                    CustomReportPredefinedFieldId = newField.CustomReportPredefinedFieldId, 
                    CustomReportId = newField.CustomReportId,
                    Operation = newField.Operation,
                    Value = newField.Value,
                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbField);
                unitOfWork.Commit();
                newField.Id = dbField.Id;
                results.Add(new EntityUpdateStatus<long>(dbField.Id, UpdateType.Insert));
            }

            return results;
        }        

        public IQueryable<CustomReportFilterDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<CustomReportFilter>()
                select new CustomReportFilterDTO()
                {
                    Id = se.Id,                    
                    CustomReportPredefinedFieldId = se.CustomReportPredefinedFieldId,
                    CustomReportId = se.CustomReportId,
                    Operation = se.Operation,
                    Value = se.Value,
                    CreateDate = se.CreateDate,
                    CreatedBy = se.CreatedBy
                };

            return query;
        }
    }
}
