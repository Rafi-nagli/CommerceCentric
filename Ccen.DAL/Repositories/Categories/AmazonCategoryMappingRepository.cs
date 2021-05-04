using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.DAL.Repositories.Categories
{
    public class AmazonCategoryMappingRepository : Repository<AmazonCategoryMapping>, IAmazonCategoryMappingRepository
    {
        public AmazonCategoryMappingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
