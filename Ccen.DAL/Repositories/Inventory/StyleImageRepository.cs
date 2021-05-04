using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class StyleImageRepository : Repository<StyleImage>, IStyleImageRepository
    {
        public StyleImageRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleImageDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }


        public IList<EntityUpdateStatus<long>> UpdateImagesForStyle(long styleId,
            IList<StyleImageDTO> images,
            DateTime when,
            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();

            var dbExistImages = GetFiltered(l => l.StyleId == styleId
                && !l.IsSystem
                && l.Category != (int)StyleImageCategories.Deleted).ToList();
            var newImages = images.Where(l => l.Id == 0).ToList();

            foreach (var dbImage in dbExistImages)
            {
                var existImg = images.FirstOrDefault(l => l.Id == dbImage.Id);
                if (existImg != null)
                {
                    bool hasChanges = dbImage.Image != existImg.Image
                                      || dbImage.IsDefault != existImg.IsDefault
                                      || dbImage.Tags != existImg.Tags
                                      || dbImage.Category != existImg.Category;

                    if (hasChanges)
                    {
                        if (dbImage.Image != existImg.Image)
                            dbImage.Type = (int)StyleImageType.None;

                        dbImage.Image = existImg.Image;
                        dbImage.Category = existImg.Category;
                        dbImage.IsDefault = existImg.IsDefault;
                        dbImage.Tags = existImg.Tags;
                        dbImage.UpdateDate = when;
                        dbImage.UpdatedBy = by;

                        results.Add(new EntityUpdateStatus<long>(dbImage.Id, UpdateType.Update));
                    }
                }
                else
                {
                    Remove(dbImage);
                    results.Add(new EntityUpdateStatus<long>(dbImage.Id, UpdateType.Removed));
                }
            }

            unitOfWork.Commit();

            foreach (var newImage in newImages)
            {
                var dbImage = new StyleImage()
                {
                    StyleId = styleId,
                    Image = newImage.Image,
                    Category = newImage.Category,
                    IsDefault = newImage.IsDefault,
                    Type = (int)StyleImageType.None,
                    Tags = newImage.Tags,
                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbImage);
                unitOfWork.Commit();

                newImage.Id = dbImage.Id;

                results.Add(new EntityUpdateStatus<long>(dbImage.Id, UpdateType.Insert));
            }

            return results;
        }


        private IQueryable<StyleImageDTO> AsDto(IQueryable<StyleImage> query)
        {
            return query.Select(s => new StyleImageDTO()
            {
                Id = s.Id,
                Image = s.Image,
                Type = s.Type,
                Category = s.Category,
                SourceMarketId = s.SourceMarketId,
                IsDefault = s.IsDefault,
                IsSystem = s.IsSystem,
                StyleId = s.StyleId,
                OrderIndex = s.OrderIndex,
                Tags = s.Tags
            });
        }
    }
}
