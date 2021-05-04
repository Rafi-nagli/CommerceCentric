using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Cache;
using Amazon.Core.Entities.Caches;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO.Caches;

namespace Amazon.DAL.Repositories.Cache
{
    public class ParentItemCacheRepository : Repository<ParentItemCache>, IParentItemCacheRepository
    {
        public ParentItemCacheRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public EntityUpdateStatus<long> UpdateCacheItem(ParentItemCacheDTO cache)
        {
            var dbItem = unitOfWork.GetSet<ParentItemCache>()
                .FirstOrDefault(c => c.Id == cache.Id);

            if (dbItem == null)
            {
                dbItem = new ParentItemCache();
                dbItem.Id = cache.Id;
                unitOfWork.GetSet<ParentItemCache>().Add(dbItem);
            }

            UpdateCacheItem(dbItem, cache);

            unitOfWork.Commit();

            return null;
        }

        public IList<EntityUpdateStatus<long>> RefreshCacheItems(IList<ParentItemCacheDTO> caches,
            DateTime when)
        {
            var dbItems = unitOfWork.GetSet<ParentItemCache>().ToList();

            foreach (var cache in caches)
            {
                var dbItem = dbItems.FirstOrDefault(i => i.Id == cache.Id);
                if (dbItem == null)
                {
                    dbItem = new ParentItemCache();
                    dbItem.Id = cache.Id;
                    unitOfWork.GetSet<ParentItemCache>().Add(dbItem);
                }
                UpdateCacheItem(dbItem, cache);
            }

            var forRemoveItems = dbItems.Where(i => caches.All(c => c.Id != i.Id)).ToList();
            foreach (var removeItem in forRemoveItems)
            {
                unitOfWork.GetSet<ParentItemCache>().Remove(removeItem);
            }

            unitOfWork.Commit();

            unitOfWork.Context.Database.ExecuteSqlCommand("UPDATE ParentItemCaches SET UpdateDate=@p0", when);

            return null;
        }

        private void UpdateCacheItem(ParentItemCache item, ParentItemCacheDTO dto)
        {
            item.Id = dto.Id;
            item.Market = dto.Market;
            item.MarketplaceId = dto.MarketplaceId;

            item.DisplayQuantity = dto.DisplayQuantity;
            item.RealQuantity = dto.RealQuantity;
            item.LastSoldDate = dto.LastSoldDate;
            item.LastOpenDate = dto.LastOpenDate;

            item.HasListings = dto.HasListings;
            item.HasChildWithFakeParentASIN = dto.HasChildWithFakeParentASIN;
            item.HasQtyDifferences = dto.HasQtyDifferences;
            item.HasPriceDifferences = dto.HasPriceDifferences;

            item.PositionsInfo = dto.PositionsInfo;

            item.MinPrice = dto.MinPrice;
            item.MaxPrice = dto.MaxPrice;
            item.IsDirty = false;

            if (item.CreateDate == null)
                item.CreateDate = dto.CreateDate;
        }

        public IQueryable<ParentItemCacheDTO> GetAllCacheItems()
        {
            return unitOfWork.GetSet<ParentItemCache>()
                .Select(c => new ParentItemCacheDTO
                {
                    Id = c.Id,
                    Market = c.Market,
                    MarketplaceId = c.MarketplaceId,

                    RealQuantity = c.RealQuantity,
                    DisplayQuantity = c.DisplayQuantity,

                    LastSoldDate = c.LastSoldDate,
                    LastOpenDate = c.LastOpenDate,

                    HasListings = c.HasListings,
                    HasChildWithFakeParentASIN = c.HasChildWithFakeParentASIN,
                    HasQtyDifferences = c.HasQtyDifferences,
                    HasPriceDifferences = c.HasPriceDifferences,

                    MaxPrice = c.MaxPrice,
                    MinPrice = c.MinPrice,

                    PositionsInfo = c.PositionsInfo,

                    IsDirty = c.IsDirty,

                    UpdateDate = c.UpdateDate,
                    CreateDate = c.CreateDate
                });
        }
    }
}
