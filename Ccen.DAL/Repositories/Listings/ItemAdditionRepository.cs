using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Listings;
using Amazon.DTO;
using Amazon.DTO.Graphs;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class ItemAdditionRepository : Repository<ItemAddition>, IItemAdditionRepository
    {
        public ItemAdditionRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<ItemAdditionDTO> GetAllAsDTO()
        {
            return AsDto(GetAll());
        }

        public IQueryable<ItemAdditionDTO> AsDto(IQueryable<ItemAddition> query)
        {
            return query.Select(i => new ItemAdditionDTO()
            {
                Id = i.Id,
                ItemId = i.ItemId,
                Field = i.Field,
                Value = i.Value,
                Source = i.Source,
                CreateDate = i.CreateDate,
            });
        }
    }
}
