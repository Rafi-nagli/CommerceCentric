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
using Amazon.Core.Entities.Inventory;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class PhotoshootPickListRepository : Repository<PhotoshootPickList>, IPhotoshootPickListRepository
    {
        public PhotoshootPickListRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<PhotoshootPickListDTO> GetAllAsDto()
        {
            return unitOfWork.GetSet<PhotoshootPickList>().Select(f => new PhotoshootPickListDTO()
            {
                Id = f.Id,
                Archived = f.Archived,
                PhotoshootDate = f.PhotoshootDate,
                IsLocked = f.IsLocked,
                CreateDate = f.CreateDate,
                CreatedBy = f.CreatedBy,
            });
        }
    }
}
