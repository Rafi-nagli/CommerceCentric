using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
    public class ItemImageRepository : Repository<ItemImage>, IItemImageRepository
    {
        public ItemImageRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ItemImage Update(ImageInfo imageInfo, DateTime when)
        {
            var dbExistItemImages = GetAll().Where(i => i.ItemId == imageInfo.Tag).ToList();

            ItemImage dbExistImage = null;
            if (!dbExistItemImages.Any())
            {
                dbExistImage = new ItemImage();
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
                dbExistImage.Name = imageInfo.Name;
                dbExistImage.ItemId = imageInfo.Tag ?? 0;
                dbExistImage.ImageType = imageInfo.ImageType;
                dbExistImage.UpdateFailAttempts = 0;
                dbExistImage.UpdateDate = when;
            }
            else
            {
                if (dbExistImage.UpdateFailAttempts >= 100)
                {
                    dbExistImage.Image = "#";
                    dbExistImage.Name = imageInfo.Name;
                    dbExistImage.ItemId = imageInfo.Tag ?? 0;
                    dbExistImage.ImageType = imageInfo.ImageType;
                    dbExistImage.UpdateFailAttempts = 0;
                    dbExistImage.UpdateDate = when;
                }
                else
                {
                    dbExistImage.UpdateFailAttempts++;
                }

            }

            return dbExistImage;
        }

        public IQueryable<ItemImageDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<ItemImageDTO> AsDto(IQueryable<ItemImage> query)
        {
            return query.Select(s => new ItemImageDTO()
            {
                Id = s.Id,
                Image = s.Image,
                ImageType = s.ImageType,
                ItemId = s.ItemId,
                UpdateFailAttempts = s.UpdateFailAttempts,
                UpdateDate = s.UpdateDate,
            });
        }
    }
}
