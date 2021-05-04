using System.Linq;
using Amazon.Core.Entities;

namespace Amazon.Core.Contracts.Db
{
    public interface ISyncMessageRepository : IRepository<SyncMessage>
    {
    }
}
