using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Categories;
using Amazon.Core.Entities.Users;
using Amazon.Core.Models;
using Amazon.DTO.Categories;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface ICustomCategoryToStyleRepository : IRepository<CustomCategoryToStyle>
    {
        IQueryable<CustomCategoryToStyleDTO> GetAllAsDto();
    }
}
