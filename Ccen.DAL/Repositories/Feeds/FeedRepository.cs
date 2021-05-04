using System;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class FeedRepository : Repository<Feed>, IFeedRepository
    {
        public FeedRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<FeedDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public string GetUnprocessedFeedId(int type, MarketType market, string marketplaceId)
        {
            var feed = GetUnprocessedFeed(type, market, marketplaceId);
            return feed != null ? feed.AmazonIdentifier : null;
        }

        public long InsertFeed(string feedRequestId, 
            int messages, 
            int type, 
            int status,
            MarketType market,
            string marketplaceId)
        {
            var feed = new Feed
            {
                AmazonIdentifier = feedRequestId,
                Market = (int)market,
                MarketplaceId = marketplaceId,
                MessageCount = messages,
                Type = type,
                Status = status,
                SubmitDate = DateTime.UtcNow
            };
            Add(feed);
            unitOfWork.Commit();
            return feed.Id;
        }

        public FeedDTO GetUnprocessedFeed(int type, MarketType market, string marketplaceId)
        {
            var query = unitOfWork.GetSet<Feed>().OrderByDescending(f => f.SubmitDate).Where(f => f.Type == type 
                && (f.Status == (int)FeedStatus.Submitted
                    || f.Status == (int)FeedStatus.InProgress));

            if (market != MarketType.None)
                query = query.Where(i => i.Market == (int) market);

            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(i => i.MarketplaceId == marketplaceId);

            return AsDto(query).FirstOrDefault();
        }

        public IQueryable<FeedDTO> AsDto(IQueryable<Feed> query)
        {
            return query.Select(f => new FeedDTO()
            {
                Id = f.Id,
                Market = f.Market,
                MarketplaceId = f.MarketplaceId,

                Name = f.Name,
                Description = f.Description,

                AmazonIdentifier = f.AmazonIdentifier,
                RequestFilename = f.RequestFilename,
                ResponseFilename = f.ResponseFilename,

                Status = f.Status,
                MessageCount = f.MessageCount,

                SubmitDate = f.SubmitDate,

                Type = f.Type,
            });
        }
    }
}
