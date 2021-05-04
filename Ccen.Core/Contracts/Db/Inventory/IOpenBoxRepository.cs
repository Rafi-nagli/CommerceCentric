using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IOpenBoxRepository : IRepository<OpenBox>
    {
        IList<OpenBox> GetByStyleId(long styleId);
        IQueryable<OpenBoxDto> GetAllAsDto();

        void MarkAsPrintedByStyleId(string styleId);
        string GetDefaultBoxName(long styleId, DateTime when);
    }
}
