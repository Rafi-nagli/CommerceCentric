using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class CountryRepository : Repository<Country>, ICountryRepository
    {
        public CountryRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
