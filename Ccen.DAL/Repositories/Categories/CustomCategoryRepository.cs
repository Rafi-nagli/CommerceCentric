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
    public class CustomCategoryRepository : Repository<CustomCategory>, ICustomCategoryRepository
    {
        public CustomCategoryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomCategoryDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomCategoryDTO> AsDto(IQueryable<CustomCategory> query)
        {
            return query.Select(c => new CustomCategoryDTO()
            {
                Id = c.Id,

                Name = c.Name,
                CategoryId = c.CategoryId,
                ParentCategoryName = c.ParentCategoryName,
                CategoryPath = c.CategoryPath,

                Mode = c.Mode,

                ShopifyUrl = c.ShopifyUrl,
                ShopifyName = c.ShopifyName,
                ShopifyDescription = c.ShopifyDescription,

                Market = c.Market,
                MarketplaceId = c.MarketplaceId,

                CreateDate = c.CreateDate
            });
        }
    }
}
