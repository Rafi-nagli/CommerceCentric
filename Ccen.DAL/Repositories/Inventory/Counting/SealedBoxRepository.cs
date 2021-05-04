using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;

namespace Amazon.DAL.Repositories.Inventory
{
    public class SealedBoxCountingRepository : Repository<SealedBoxCounting>, ISealedBoxCountingRepository
    {
        public SealedBoxCountingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<SealedBoxCounting> GetByStyleId(long styleId)
        {
            return unitOfWork.GetSet<SealedBoxCounting>().Where(s => s.StyleId == styleId).ToList();
        }
    }
}
