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
    public class ListingCacheRepository : Repository<ListingCache>, IListingCacheRepository
    {
        public ListingCacheRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public EntityUpdateStatus<long> UpdateCacheItem(ListingCacheDTO cache)
        {
            var dbItem = unitOfWork.GetSet<ListingCache>()
                .FirstOrDefault(c => c.Id == cache.Id);

            if (dbItem == null)
            {
                dbItem = new ListingCache();
                dbItem.Id = cache.Id;
                unitOfWork.GetSet<ListingCache>().Add(dbItem);
            }

            UpdateCacheItem(dbItem, cache);

            unitOfWork.Commit();

            return null;
        }

        public IList<EntityUpdateStatus<long>> RefreshCacheItems(IList<ListingCacheDTO> caches,
            DateTime when)
        {
            var dbItems = unitOfWork.GetSet<ListingCache>().ToList();

            foreach (var cache in caches)
            {
                var dbItem = dbItems.FirstOrDefault(i => i.Id == cache.Id);
                if (dbItem == null)
                {
                    dbItem = new ListingCache();
                    dbItem.Id = cache.Id;
                    unitOfWork.GetSet<ListingCache>().Add(dbItem);
                }
                UpdateCacheItem(dbItem, cache);
            }

            var forRemoveItems = dbItems.Where(i => caches.All(c => c.Id != i.Id)).ToList();
            foreach (var removeItem in forRemoveItems)
            {
                unitOfWork.GetSet<ListingCache>().Remove(removeItem);
            }

            unitOfWork.Commit();

            unitOfWork.Context.Database.ExecuteSqlCommand("UPDATE ListingCaches SET UpdateDate=@p0", when);

            return null;
        }

        private void UpdateCacheItem(ListingCache item, ListingCacheDTO dto)
        {
            item.Id = dto.Id;
            item.ItemId = dto.ItemId;

            item.SoldQuantity = dto.SoldQuantity;
            item.MaxOrderDate = dto.MaxOrderDate;

            //item.MarketplaceId = dto.MarketplaceId;

            item.IsDirty = false;

            if (item.CreateDate == null)
                item.CreateDate = dto.CreateDate;

            //Note: disable for speed up updating
            //item.UpdateDate = dto.UpdateDate;
        }

        public IQueryable<ListingCacheDTO> GetAllCacheItems()
        {
            return unitOfWork.GetSet<ListingCache>()
                .Select(c => new ListingCacheDTO
                {
                    Id = c.Id,
                    ItemId = c.ItemId,

                    SoldQuantity = c.SoldQuantity,
                    MaxOrderDate = c.MaxOrderDate,

                    IsDirty = c.IsDirty,

                    UpdateDate = c.UpdateDate,
                    CreateDate = c.CreateDate
                });
        }
    }
}
