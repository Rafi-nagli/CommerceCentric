using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface ISealedBoxRepository : IRepository<SealedBox>
    {
        IList<SealedBox> GetByStyleId(long styleId);
        IQueryable<SealedBoxDto> GetAllAsDto();
        void MarkAsPrintedByStyleId(long styleId);
        string GetDefaultBoxName(long styleId, DateTime when);
        string GetDefaultBoxNameForBothType(long styleId, DateTime when);
    }
}
