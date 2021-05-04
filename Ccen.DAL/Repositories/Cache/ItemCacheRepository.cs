using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db.Cache;
using Amazon.Core.Entities.Caches;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO.Caches;

namespace Amazon.DAL.Repositories.Cache
{
    public class ItemCacheRepository : Repository<ItemCache>, IItemCacheRepository
    {
        public ItemCacheRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public EntityUpdateStatus<long> UpdateCacheItem(ItemCacheDTO cache)
        {
            var dbItem = unitOfWork.GetSet<ItemCache>()
                .FirstOrDefault(c => c.Id == cache.Id);

            if (dbItem == null)
            {
                dbItem = new ItemCache();
                dbItem.Id = cache.Id;
                unitOfWork.GetSet<ItemCache>().Add(dbItem);
            }

            UpdateCacheItem(dbItem, cache);

            unitOfWork.Commit();

            return null;
        }

        public IList<EntityUpdateStatus<long>> RefreshCacheItems(IList<ItemCacheDTO> caches, DateTime when)
        {
            var dbItems = unitOfWork.GetSet<ItemCache>().ToList();

            foreach (var cache in caches)
            {
                var dbItem = dbItems.FirstOrDefault(i => i.Id == cache.Id);
                if (dbItem == null)
                {
                    dbItem = new ItemCache();
                    dbItem.Id = cache.Id;
                    unitOfWork.GetSet<ItemCache>().Add(dbItem);
                }
                UpdateCacheItem(dbItem, cache);
            }

            var forRemoveItems = dbItems.Where(i => caches.All(c => c.Id != i.Id)).ToList();
            foreach (var removeItem in forRemoveItems)
            {
                unitOfWork.GetSet<ItemCache>().Remove(removeItem);
            }
            
            unitOfWork.Commit();

            unitOfWork.Context.Database.ExecuteSqlCommand("UPDATE ItemCaches SET UpdateDate=@p0", when);

            return null;
        }

        private void UpdateCacheItem(ItemCache item, ItemCacheDTO dto)
        {
            item.Id = dto.Id;

            item.LastSoldDate = dto.LastSoldDate;

            //item.MarketplaceId = dto.MarketplaceId;

            item.IsDirty = false;

            if (item.CreateDate == null)
                item.CreateDate = dto.CreateDate;

            //Note: disable for speed up updating
            //item.UpdateDate = dto.UpdateDate;
        }

        public IQueryable<ItemCacheDTO> GetAllCacheItems()
        {
            return unitOfWork.GetSet<ItemCache>()
                .Select(c => new ItemCacheDTO
                {
                    Id = c.Id,

                    LastSoldDate = c.LastSoldDate,

                    IsDirty = c.IsDirty,

                    UpdateDate = c.UpdateDate,
                    CreateDate = c.CreateDate
                });
        }
    }
}
