using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Users;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;
using Amazon.Core.Views;

namespace Amazon.DAL.Repositories
{
    public class CustomFeedRepository : Repository<CustomFeed>, ICustomFeedRepository
    {
        public CustomFeedRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomFeedDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomFeedDTO> AsDto(IQueryable<CustomFeed> query)
        {
            return query.Select(b => new CustomFeedDTO()
            {
                Id = b.Id,

                FeedName = b.FeedName,
                Protocol = b.Protocol,
                FtpSite = b.FtpSite,
                FtpFolder = b.FtpFolder,
                UserName = b.UserName,

                DropShipperId = b.DropShipperId,
                OverrideDSFeedType = b.OverrideDSFeedType,
                OverrideDSProductType = b.OverrideDSProductType,

                Password = b.Password,
                IsPassiveMode = b.IsPassiveMode,
                IsSFTP = b.IsSFTP,

                UpdateDate = b.UpdateDate,
                CreateDate = b.CreateDate,
                CreatedBy = b.CreatedBy
            });
        }
    }
}
