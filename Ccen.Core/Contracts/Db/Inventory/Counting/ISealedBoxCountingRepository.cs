using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface ISealedBoxCountingRepository : IRepository<SealedBoxCounting>
    {
        IList<SealedBoxCounting> GetByStyleId(long styleId);
    }
}
