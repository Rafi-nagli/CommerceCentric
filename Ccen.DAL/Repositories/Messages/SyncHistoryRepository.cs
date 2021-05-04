using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class SyncHistoryRepository : Repository<SyncHistory>, ISyncHistoryRepository
    {
        public SyncHistoryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
