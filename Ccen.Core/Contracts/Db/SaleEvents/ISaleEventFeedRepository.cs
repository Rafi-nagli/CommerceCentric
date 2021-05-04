using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Events;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Events;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.Core.Contracts.Db
{
    public interface ISaleEventFeedRepository : IRepository<SaleEventFeed>
    {
        IQueryable<SaleEventFeedDTO> GetAllAsDto();
    }
}
