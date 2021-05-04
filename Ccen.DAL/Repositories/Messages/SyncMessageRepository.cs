using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class SyncMessageRepository : Repository<SyncMessage>, ISyncMessageRepository
    {
        public SyncMessageRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
