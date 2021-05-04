using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IOpenBoxCountingRepository : IRepository<OpenBoxCounting>
    {
        IList<OpenBoxCounting> GetByStyleId(long styleId);
    }
}
