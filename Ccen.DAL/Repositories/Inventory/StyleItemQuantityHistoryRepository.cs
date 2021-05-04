using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class StyleItemQuantityHistoryRepository : Repository<StyleItemQuantityHistory>, IStyleItemQuantityHistoryRepository
    {
        public StyleItemQuantityHistoryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleItemQuantityHistoryDTO> GetAllAsDto()
        {
            var query = from ch in GetAll()
                        join si in unitOfWork.StyleItems.GetAll() on ch.StyleItemId equals si.Id
                        join u in unitOfWork.Users.GetAll() on ch.CreatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        select new StyleItemQuantityHistoryDTO()
                        {
                            Id = ch.Id,
                            StyleId = si.StyleId,

                            Size = si.Size,
                            Color = si.Color,

                            FromQuantity = ch.FromQuantity,
                            Quantity = ch.Quantity,
                            RemainingQuantity = ch.RemainingQuantity,
                            Tag = ch.Tag,
                            Type = ch.Type,

                            CreatedBy = ch.CreatedBy,
                            CreateDate = ch.CreateDate,
                            CreatedByName = u.Name,
                        };

            return query;
        }
    }
}
