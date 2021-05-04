using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IStyleItemQuantityHistoryRepository : IRepository<StyleItemQuantityHistory>
    {
        IQueryable<StyleItemQuantityHistoryDTO> GetAllAsDto();
    }
}
