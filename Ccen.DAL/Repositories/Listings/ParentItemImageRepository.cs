using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class ParentItemImageRepository : Repository<ParentItemImage>, IParentItemImageRepository
    {
        public ParentItemImageRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ParentItemImage Update(ImageInfo imageInfo, DateTime when)
        {
            var dbExistItemImages = GetAll().Where(i => i.ParentItemId == imageInfo.Tag).ToList();

            ParentItemImage dbExistImage = null;
            if (!dbExistItemImages.Any())
            {
                dbExistImage = new ParentItemImage();
                dbExistImage.CreateDate = when;

                Add(dbExistImage);
            }
            else
            {
                dbExistImage = dbExistItemImages.First();
            }

            if (!String.IsNullOrEmpty(imageInfo.Image))
            {
                dbExistImage.Image = imageInfo.Image;
                dbExistImage.ParentItemId = imageInfo.Tag ?? 0;
                dbExistImage.ImageType = imageInfo.ImageType;
                dbExistImage.UpdateDate = when;
            }
            else
            {
                if (imageInfo.UpdateFailAttempts >= 100)
                {
                    dbExistImage.Image = "#";
                    dbExistImage.ParentItemId = imageInfo.Tag ?? 0;
                    dbExistImage.ImageType = imageInfo.ImageType;
                    dbExistImage.UpdateDate = when;
                }
            }
            return dbExistImage;
        }

        public IQueryable<ItemImageDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<ItemImageDTO> AsDto(IQueryable<ParentItemImage> query)
        {
            return query.Select(s => new ItemImageDTO()
            {
                Id = s.Id,
                Image = s.Image,
                ImageType = s.ImageType,
                ItemId = s.ParentItemId,
                UpdateFailAttempts = s.UpdateFailAttempts,
                UpdateDate = s.UpdateDate,
            });
        }
    }
}
