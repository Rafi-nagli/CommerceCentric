using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.DropShippers;
using Amazon.DTO.DropShippers;

namespace Amazon.Core.Contracts.Db.DropShippers
{
    public interface IStyleDSHistoryRepository : IRepository<StyleDSHistory>
    {
        IQueryable<StyleDSHistoryDTO> GetRecentAsDto();
        IQueryable<StyleDSHistoryDTO> GetAllAsDto();
    }
}
