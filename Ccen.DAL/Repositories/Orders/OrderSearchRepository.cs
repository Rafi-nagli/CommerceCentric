using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories
{
    public class OrderSearchRepository : Repository<OrderSearch>, IOrderSearchRepository
    {
        public OrderSearchRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<OrderSearchDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<OrderSearchDTO> AsDto(IQueryable<OrderSearch> query)
        {
            return query.Select(b => new OrderSearchDTO()
            {
                Id = b.Id,
                OrderNumber = b.OrderNumber,
                CreateDate = b.CreateDate,
                CreatedBy = b.CreatedBy,
            });
        }
    }
}
