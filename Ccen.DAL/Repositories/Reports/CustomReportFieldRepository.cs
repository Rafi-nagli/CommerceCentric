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
using Amazon.Core.Entities.CustomReports;
using Amazon.DTO.CustomReports;

namespace Amazon.DAL.Repositories
{
    public class CustomReportFieldRepository : Repository<CustomReportField>, ICustomReportFieldRepository
    {
        public CustomReportFieldRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<EntityUpdateStatus<long>> BulkUpdateForFeed(long reportId, 
            IList<CustomReportFieldDTO> fields, 
            DateTime when, 
            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();

            var dbExistFields = GetFiltered(l => l.CustomReportId == reportId).ToList();
            var newFields = fields.Where(l => l.Id == 0).ToList();

            foreach (var dbField in dbExistFields)
            {
                var existField = fields.FirstOrDefault(l => l.Id == dbField.Id);
                if (existField != null)
                {
                    dbField.CustomReportPredefinedFieldId = existField.CustomReportPredefinedFieldId;
                    dbField.SortOrder = existField.SortOrder;
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
                var dbField = new CustomReportField()
                {
                    CustomReportId = reportId,
                    CustomReportPredefinedFieldId = newField.CustomReportPredefinedFieldId,
                    SortOrder = newField.SortOrder,
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

        public IQueryable<CustomReportFieldDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<CustomReportField>()
                select new CustomReportFieldDTO()
                {
                    Id = se.Id,
                    CustomReportId = se.CustomReportId,
                    CustomReportPredefinedFieldId = se.CustomReportPredefinedFieldId,
                    SortOrder = se.SortOrder,
                    CreateDate = se.CreateDate,
                    CreatedBy = se.CreatedBy
                };

            return query;
        }
    }
}
