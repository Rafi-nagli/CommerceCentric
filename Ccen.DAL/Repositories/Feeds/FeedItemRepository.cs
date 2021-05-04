using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.Feeds;
using Amazon.Core.Models;
using Amazon.DTO.Feeds;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class FeedItemRepository : Repository<FeedItem>, IFeedItemRepository
    {
        public FeedItemRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<FeedItemDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public void Insert(IList<FeedItemDTO> feedItems)
        {
            if (feedItems == null)
                return;

            foreach (var feedItem in feedItems)
            {
                Add(new FeedItem()
                {
                    FeedId = feedItem.FeedId,
                    ItemId = feedItem.ItemId,
                    MessageId = feedItem.MessageId
                });
            }
            unitOfWork.Commit();
        }

        public IQueryable<FeedItemDTO> AsDto(IQueryable<FeedItem> query)
        {
            return query.Select(f => new FeedItemDTO()
            {
                Id = f.Id,
                FeedId = f.FeedId,
                MessageId = f.MessageId,
                ItemId = f.ItemId,
            });
        }
    }
}
