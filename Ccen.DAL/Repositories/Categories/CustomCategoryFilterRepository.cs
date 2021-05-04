using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Categories;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Categories;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class CustomCategoryFilterRepository : Repository<CustomCategoryFilter>, ICustomCategoryFilterRepository
    {
        public CustomCategoryFilterRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomCategoryFilterDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomCategoryFilterDTO> AsDto(IQueryable<CustomCategoryFilter> query)
        {
            return query.Select(b => new CustomCategoryFilterDTO()
            {
                Id = b.Id,

                CustomCategoryId = b.CustomCategoryId,
                AttributeName = b.AttributeName,
                Operation = b.Operation,
                AttributeValues = b.AttributeValues,

                CreateDate = b.CreateDate,
            });
        }
    }
}
