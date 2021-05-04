using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleAdditionRepository : IRepository<StyleAddition>
    {
        IList<StyleAdditionDTO> GetByStyleId(long styleId);
        IQueryable<StyleAdditionDTO> GetAllAsDto();
    }
}
