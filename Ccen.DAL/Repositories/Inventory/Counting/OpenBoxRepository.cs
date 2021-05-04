using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;

namespace Amazon.DAL.Repositories.Inventory
{
    public class OpenBoxCountingRepository : Repository<OpenBoxCounting>, IOpenBoxCountingRepository
    {
        public OpenBoxCountingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<OpenBoxCounting> GetByStyleId(long styleId)
        {
            return unitOfWork.GetSet<OpenBoxCounting>().Where(s => s.StyleId == styleId).ToList();
        }
    }
}
