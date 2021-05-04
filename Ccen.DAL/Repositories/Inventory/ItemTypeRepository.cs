using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class ItemTypeRepository : Repository<ItemType>, IItemTypeRepository
    {
        public ItemTypeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ItemTypeDTO> GetAllAsDto()
        {
            return unitOfWork.GetSet<ItemType>().Select(t => new ItemTypeDTO()
            {
                Id = t.Id,
                Name = t.Name
            });
        }
    }
}
