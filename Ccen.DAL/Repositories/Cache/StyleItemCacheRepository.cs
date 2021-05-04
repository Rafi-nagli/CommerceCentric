using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Contracts.Db.Cache;
using Amazon.Core.Entities.Caches;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO.Caches;

namespace Amazon.DAL.Repositories.Cache
{
    public class StyleItemCacheRepository : Repository<StyleItemCache>, IStyleItemCacheRepository
    {
        public StyleItemCacheRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<StyleItemCacheDTO> GetForStyleId(long styleId)
        {
            var query = unitOfWork.GetSet<StyleItemCache>().Where(si => si.StyleId == styleId);

            return AsDto(query).ToList();
        }

        public IList<StyleItemCacheDTO> GetForStyleItemId(long styleItemId)
        {
            var query = unitOfWork.GetSet<StyleItemCache>().Where(si => si.Id == styleItemId);

            return AsDto(query).ToList();
        }

        public IQueryable<StyleItemCacheDTO> GetAllAsDto()
        {
            var query = unitOfWork.GetSet<StyleItemCache>();

            return AsDto(query);
        }

        private IQueryable<StyleItemCacheDTO> AsDto(IQueryable<StyleItemCache> items)
        {
            return items.Select(i => new StyleItemCacheDTO()
            {
                Id = i.Id,

                StyleId = i.StyleId ?? 0,
                Size = i.Size,

                RemainingQuantity = i.RemainingQuantity,
                RemainingOnHandQuantity = i.RemainingOnHandQuantity,

                InventoryQuantity = i.InventoryQuantity,
                InventoryQuantitySetDate = i.InventoryQuantitySetDate,

                InventoryOnHandQuantity = i.InventoryOnHandQuantity,
                InventoryOnHandQuantitySetDate = i.InventoryOnHandQuantitySetDate,

                ScannedSoldQuantityFromDate = i.ScannedSoldQuantityFromDate,
                SentToFBAQuantityFromDate = i.SentToFBAQuantityFromDate,
                MarketsSoldQuantityFromDate = i.MarketsSoldQuantityFromDate,
                MarketsSoldOnHandQuantityFromDate = i.MarketsSoldOnHandQuantityFromDate,
                SpecialCaseQuantityFromDate = i.SpecialCaseQuantityFromDate,
                SaleEventQuantityFromDate = i.SaleEventQuantityFromDate,
                ReservedQuantityFromDate = i.ReservedQuantityFromDate,
                SentToPhotoshootQuantityFromDate = i.SentToPhotoshootQuantityFromDate,

                TotalScannedSoldQuantity = i.TotalScannedSoldQuantity,
                TotalSentToFBAQuantity = i.TotalSentToFBAQuantity,
                TotalMarketsSoldQuantity = i.TotalMarketsSoldQuantity,
                TotalMarketsSoldOnHandQuantity = i.TotalMarketsSoldOnHandQuantity,
                TotalSpecialCaseQuantity = i.TotalSpecialCaseQuantity,
                TotalSaleEventQuantity = i.TotalSaleEventQuantity,
                TotalReservedQuantity = i.TotalReservedQuantity,
                TotalSentToPhotoshootQuantity = i.TotalSentToPhotoshootQuantity,


                ScannedMaxOrderDate = i.ScannedMaxOrderDate,

                BoxQuantity = i.BoxQuantity,
                BoxQuantitySetDate = i.BoxQuantitySetDate,

                MarketplacesInfo = i.MarketplacesInfo,
                SalesInfo = i.SalesInfo,
                IsInVirtual = i.IsInVirtual,

                Cost = i.Cost,

                IsDirty = i.IsDirty,
                UpdateDate = i.UpdateDate
            });
        }


        public EntityUpdateStatus<long> UpdateCacheItem(StyleItemCacheDTO cache)
        {
            var dbItem = unitOfWork.GetSet<StyleItemCache>()
                .FirstOrDefault(c => c.Id == cache.Id);

            var changeMode = UpdateType.Update;
            if (dbItem == null)
            {
                dbItem = new StyleItemCache();
                dbItem.Id = cache.Id;
                unitOfWork.GetSet<StyleItemCache>().Add(dbItem);

                changeMode = UpdateType.Insert;
            }

            var oldQuantity = dbItem.RemainingQuantity;
            UpdateCacheItem(dbItem, cache);

            unitOfWork.Commit();

            return new EntityUpdateStatus<long>()
            {
                Id = dbItem.Id,
                Status = changeMode,
                Tag = dbItem.RemainingQuantity.ToString(),
                TagSecond = oldQuantity.ToString(),
                CalcDate = cache.CalcDate,
            };
        }
        
        public IList<EntityUpdateStatus<long>> RefreshCacheItems(IList<StyleItemCacheDTO> caches,
            DateTime when)
        {
            var updateInfoes = new List<Tuple<StyleItemCache, int, UpdateType>>();

            var dbItems = unitOfWork.GetSet<StyleItemCache>().ToList();
            
            foreach (var cache in caches)
            {
                var updateMode = UpdateType.Update;
                
                var dbItem = dbItems.FirstOrDefault(i => i.Id == cache.Id);
                if (dbItem == null)
                {
                    dbItem = new StyleItemCache();
                    dbItem.Id = cache.Id;
                    unitOfWork.GetSet<StyleItemCache>().Add(dbItem);
                    dbItems.Add(dbItem);

                    updateMode = UpdateType.Insert;
                }

                var oldQuantity = dbItem.RemainingQuantity;

                UpdateCacheItem(dbItem, cache);

                updateInfoes.Add(new Tuple<StyleItemCache, int, UpdateType>(dbItem, oldQuantity, updateMode));
            }

            var forRemoveItems = dbItems.Where(i => caches.All(c => c.Id != i.Id)).ToList();
            foreach (var removeItem in forRemoveItems)
            {
                unitOfWork.GetSet<StyleItemCache>().Remove(removeItem);
                updateInfoes.Add(new Tuple<StyleItemCache, int, UpdateType>(removeItem, removeItem.RemainingQuantity, UpdateType.Removed));
            }

            unitOfWork.Commit();
            
            unitOfWork.Context.Database.ExecuteSqlCommand("UPDATE StyleItemCaches SET UpdateDate=@p0", when);

            return updateInfoes.Select(i => new EntityUpdateStatus<long>()
            {
                Id = i.Item1.Id,
                Status = i.Item3,
                Tag = i.Item1.RemainingQuantity.ToString(),
                TagSecond = i.Item2.ToString()
            }).ToList();
        }

        private void UpdateCacheItem(StyleItemCache item, StyleItemCacheDTO dto)
        {
            item.Id = dto.Id;
            item.StyleId = dto.StyleId;
            item.Size = dto.Size;

            item.Cost = dto.Cost;

            item.RemainingQuantity = dto.RemainingQuantity;
            item.RemainingOnHandQuantity = dto.RemainingOnHandQuantity;

            item.InventoryQuantity = dto.InventoryQuantity;
            item.InventoryQuantitySetDate = dto.InventoryQuantitySetDate;

            item.InventoryOnHandQuantity = dto.InventoryOnHandQuantity;
            item.InventoryOnHandQuantitySetDate = dto.InventoryOnHandQuantitySetDate;

            item.BoxQuantity = dto.BoxQuantity;
            item.BoxQuantitySetDate = dto.BoxQuantitySetDate;

            item.TotalScannedSoldQuantity = dto.TotalScannedSoldQuantity;
            item.TotalSentToFBAQuantity = dto.TotalSentToFBAQuantity;
            item.TotalMarketsSoldQuantity = dto.TotalMarketsSoldQuantity;
            item.TotalMarketsSoldOnHandQuantity = dto.TotalMarketsSoldOnHandQuantity;
            item.TotalSpecialCaseQuantity = dto.TotalSpecialCaseQuantity;
            item.TotalSaleEventQuantity = dto.TotalSaleEventQuantity;
            item.TotalReservedQuantity = dto.TotalReservedQuantity;
            item.TotalSentToPhotoshootQuantity = dto.TotalSentToPhotoshootQuantity;

            item.ScannedSoldQuantityFromDate = dto.ScannedSoldQuantityFromDate;
            item.SentToFBAQuantityFromDate = dto.SentToFBAQuantityFromDate;
            item.MarketsSoldQuantityFromDate = dto.MarketsSoldQuantityFromDate;
            item.MarketsSoldOnHandQuantityFromDate = dto.MarketsSoldOnHandQuantityFromDate;
            item.SpecialCaseQuantityFromDate = dto.SpecialCaseQuantityFromDate;
            item.SaleEventQuantityFromDate = dto.SaleEventQuantityFromDate;
            item.ReservedQuantityFromDate = dto.ReservedQuantityFromDate;
            item.SentToPhotoshootQuantityFromDate = dto.SentToPhotoshootQuantityFromDate;

            item.MarketplacesInfo = dto.MarketplacesInfo;
            item.SalesInfo = dto.SalesInfo;

            item.ScannedMaxOrderDate = dto.ScannedMaxOrderDate;
            item.PreOrderExpReceiptDate = dto.PreOrderExpReceiptDate;
            item.IsInVirtual = dto.IsInVirtual;

            item.IsDirty = false;
            
            //NOTE: we have performance issue in case of always changing UpdateDate
            //if (item.UpdateDate != dto.UpdateDate)
            if (item.UpdateDate == DateTime.MinValue)
                item.UpdateDate = dto.UpdateDate;
        }

        public IQueryable<StyleItemCacheDTO> GetAllCacheItems()
        {
            return AsDto(unitOfWork.GetSet<StyleItemCache>());
        }
    }
}
