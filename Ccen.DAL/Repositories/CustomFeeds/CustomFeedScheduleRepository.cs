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

namespace Amazon.DAL.Repositories
{
    public class CustomFeedScheduleRepository : Repository<CustomFeedSchedule>, ICustomFeedScheduleRepository
    {
        public CustomFeedScheduleRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomFeedScheduleDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomFeedScheduleDTO> AsDto(IQueryable<CustomFeedSchedule> query)
        {
            return query.Select(b => new CustomFeedScheduleDTO()
            {
                Id = b.Id,
                CustomFeedId = b.CustomFeedId,

                StartTime = b.StartTime,
                RecurrencyPeriod = b.RecurrencyPeriod,
                RepeatInterval = b.RepeatInterval,
                DaysOfWeek = b.DaysOfWeek,

                UpdateDate = b.UpdateDate,
                CreateDate = b.CreateDate,
                CreatedBy = b.CreatedBy,
            });
        }
    }
}
