using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories
{
    public class StyleChangeHistoryRepository : Repository<StyleChangeHistory>, IStyleChangeHistoryRepository
    {
        public StyleChangeHistoryRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IList<StyleChangeHistoryDTO> GetByStyleIdDto(long styleId)
        {
            var query = from ch in GetAll()
                join u in unitOfWork.Users.GetAll() on ch.ChangedBy equals u.Id into withUser
                from u in withUser.DefaultIfEmpty()
                where ch.StyleId == styleId
                select new StyleChangeHistoryDTO()
                {
                    Id = ch.Id,
                    StyleId = ch.StyleId,
                    FieldName = ch.FieldName,
                    FromValue = ch.FromValue,
                    ExtendFromValue = ch.ExtendFromValue,
                    ToValue = ch.ToValue,
                    ExtendToValue = ch.ExtendToValue,

                    ChangedBy = ch.ChangedBy,
                    ChangedByName = u.Name,
                    ChangeDate = ch.ChangeDate
                };

            return query.ToList();
        }
    }
}
