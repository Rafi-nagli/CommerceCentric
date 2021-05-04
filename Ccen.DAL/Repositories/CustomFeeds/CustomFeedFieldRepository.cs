using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Users;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;
using Amazon.Core.Views;
using Amazon.DTO.CustomFeeds;
using Amazon.Core.Models;

namespace Amazon.DAL.Repositories
{
    public class CustomFeedFieldRepository : Repository<CustomFeedField>, ICustomFeedFieldRepository
    {
        public CustomFeedFieldRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomFeedFieldDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IList<EntityUpdateStatus<long>> BulkUpdateForFeed(long feedId,
            IList<CustomFeedFieldDTO> fields,
            DateTime when,
            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();

            var dbExistFields = GetFiltered(l => l.CustomFeedId == feedId).ToList();
            var newFields = fields.Where(l => l.Id == 0).ToList();

            foreach (var dbField in dbExistFields)
            {
                var existField = fields.FirstOrDefault(l => l.Id == dbField.Id);
                if (existField != null)
                {
                    dbField.SourceFieldName = existField.SourceFieldName;
                    dbField.CustomFieldName = existField.CustomFieldName;
                    dbField.CustomFieldValue = existField.CustomFieldValue;
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
                var dbField = new CustomFeedField()
                {
                    CustomFeedId = feedId,
                    SourceFieldName = newField.SourceFieldName,
                    CustomFieldName = newField.CustomFieldName,
                    CustomFieldValue = newField.CustomFieldValue,
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

        private IQueryable<CustomFeedFieldDTO> AsDto(IQueryable<CustomFeedField> query)
        {
            return query.Select(b => new CustomFeedFieldDTO()
            {
                Id = b.Id,

                CustomFeedId = b.CustomFeedId,
                SourceFieldName = b.SourceFieldName,
                CustomFieldName = b.CustomFieldName,
                CustomFieldValue = b.CustomFieldValue,
                SortOrder = b.SortOrder,

                CreateDate = b.CreateDate,
                CreatedBy = b.CreatedBy
            });
        }
    }
}
