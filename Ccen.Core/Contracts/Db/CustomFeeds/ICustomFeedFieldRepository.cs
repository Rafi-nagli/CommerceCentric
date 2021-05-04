using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Users;
using Amazon.Core.Models;
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface ICustomFeedFieldRepository : IRepository<CustomFeedField>
    {
        IQueryable<CustomFeedFieldDTO> GetAllAsDto();

        IList<EntityUpdateStatus<long>> BulkUpdateForFeed(long feedId,
            IList<CustomFeedFieldDTO> fields,
            DateTime when,
            long? by);
    }
}
