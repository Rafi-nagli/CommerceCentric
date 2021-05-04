using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IFeedRepository : IRepository<Feed>
    {
        IQueryable<FeedDTO> GetAllAsDto();
        FeedDTO GetUnprocessedFeed(int type, MarketType market, string marketplaceId);
        string GetUnprocessedFeedId(int type, MarketType market, string marketplaceId);
        long InsertFeed(string feedRequestId, int messages, int type, int status, MarketType market, string marketplaceId);
    }
}
