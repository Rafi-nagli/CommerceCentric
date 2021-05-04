using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Feeds;
using Amazon.Core.Models;
using Amazon.DTO.Feeds;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IFeedItemRepository : IRepository<FeedItem>
    {
        IQueryable<FeedItemDTO> GetAllAsDto();
        void Insert(IList<FeedItemDTO> feedItems);
    }
}
