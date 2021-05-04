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
    public class FeedMessageRepository : Repository<FeedMessage>, IFeedMessageRepository
    {
        public FeedMessageRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<FeedMessageDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public void Insert(IList<FeedMessageDTO> feedMessages)
        {
            foreach (var feedMessage in feedMessages)
            {
                Add(new FeedMessage()
                {
                    FeedId = feedMessage.FeedId,
                    MessageId = feedMessage.MessageId,
                    MessageCode = feedMessage.MessageCode,
                    ResultCode = feedMessage.ResultCode,
                    Message = feedMessage.Message,
                    CreateDate = feedMessage.CreateDate
                });
            }
            unitOfWork.Commit();
        }

        public IQueryable<FeedMessageDTO> AsDto(IQueryable<FeedMessage> query)
        {
            return query.Select(f => new FeedMessageDTO()
            {
                Id = f.Id,
                FeedId = f.FeedId,
                MessageId = f.MessageId,
                Message = f.Message,
                MessageCode = f.MessageCode,
                ResultCode = f.ResultCode,
                CreateDate = f.CreateDate,
            });
        }
    }
}
