using System.Linq;
using Amazon.Core.Entities;

namespace Amazon.Core.Contracts.Db
{
    public interface ISyncHistoryRepository : IRepository<SyncHistory>
    {
    }
}
