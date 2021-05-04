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
    public class CustomCategoryToStyleRepository : Repository<CustomCategoryToStyle>, ICustomCategoryToStyleRepository
    {
        public CustomCategoryToStyleRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomCategoryToStyleDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomCategoryToStyleDTO> AsDto(IQueryable<CustomCategoryToStyle> query)
        {
            return query.Select(b => new CustomCategoryToStyleDTO()
            {
                Id = b.Id,

                CustomCategoryId = b.CustomCategoryId,
                StyleId = b.StyleId,

                StartDate = b.StartDate,
                EndDate = b.EndDate,
                CreateDate = b.CreateDate,
            });
        }
    }
}
