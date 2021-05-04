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
using Amazon.Core.Entities.Sizes;

namespace Amazon.DAL.Repositories
{
    public class PhotoshootPickListEntryRepository : Repository<PhotoshootPickListEntry>, IPhotoshootPickListEntryRepository
    {
        public PhotoshootPickListEntryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SoldSizeInfo> GetHoldedQuantitiesByStyleItem()
        {
            var items = from ph in unitOfWork.GetSet<ViewPhotoshootHoldedByStyleItem>()
                        join si in unitOfWork.GetSet<StyleItem>() on ph.Id equals si.Id
                        select new SoldSizeInfo
                        {
                            StyleItemId = ph.Id,
                            StyleId = si.StyleId,
                            SoldQuantity = ph.SoldQuantity,
                            TotalSoldQuantity = ph.TotalSoldQuantity,
                        };
            return items;
        }

        public IQueryable<PhotoshootPickListEntryDTO> GetAllAsDto()
        {
            return unitOfWork.GetSet<PhotoshootPickListEntry>().Select(f => new PhotoshootPickListEntryDTO()
            {
                Id = f.Id,

                PhotoshootPickListId = f.PhotoshootPickListId,
                StyleId = f.StyleId,
                StyleString = f.StyleString,
                StyleItemId = f.StyleItemId,
                TakenQuantity = f.TakenQuantity,
                ReturnedQuantity = f.ReturnedQuantity,
                Status = f.Status,
                StatusDate = f.StatusDate,
                StatusBy = f.StatusBy,

                CreateDate = f.CreateDate,
                CreatedBy = f.CreatedBy
            });
        }
    }
}
