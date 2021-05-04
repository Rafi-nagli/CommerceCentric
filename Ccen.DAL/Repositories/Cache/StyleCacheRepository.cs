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
    public class StyleCacheRepository : Repository<StyleCache>, IStyleCacheRepository
    {
        public StyleCacheRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public EntityUpdateStatus<long> UpdateCacheItem(StyleCacheDTO cache)
        {
            var dbItem = unitOfWork.GetSet<StyleCache>()
                .FirstOrDefault(c => c.Id == cache.Id);
            
            if (dbItem == null)
            {
                dbItem = new StyleCache();
                dbItem.Id = cache.Id;
                unitOfWork.GetSet<StyleCache>().Add(dbItem);
            }

            UpdateCacheItem(dbItem, cache);

            unitOfWork.Commit();

            return null;
        }
        
        public IList<EntityUpdateStatus<long>> RefreshCacheItems(IList<StyleCacheDTO> caches,
            DateTime when)
        {
            var dbItems = unitOfWork.GetSet<StyleCache>().ToList();

            foreach (var cache in caches)
            {
                var dbItem = dbItems.FirstOrDefault(i => i.Id == cache.Id);
                
                if (dbItem == null)
                {
                    dbItem = new StyleCache();
                    dbItem.Id = cache.Id;
                    unitOfWork.GetSet<StyleCache>().Add(dbItem);
                }

                UpdateCacheItem(dbItem, cache);
            }

            var forRemoveItems = dbItems.Where(i => caches.All(c => c.Id != i.Id)).ToList();
            foreach (var removeItem in forRemoveItems)
            {
                unitOfWork.GetSet<StyleCache>().Remove(removeItem);
            }

            unitOfWork.Commit();

            unitOfWork.Context.Database.ExecuteSqlCommand("UPDATE StyleCaches SET UpdateDate=@p0", when);

            return null;
        }

        private void UpdateCacheItem(StyleCache item, StyleCacheDTO dto)
        {
            if (item.MainLicense != dto.MainLicense)
                item.MainLicense = dto.MainLicense;
            if (item.SubLicense != dto.SubLicense)
                item.SubLicense = dto.SubLicense;
            if (item.Gender != dto.Gender)
                item.Gender = dto.Gender;
            if (item.AgeGroup != dto.AgeGroup)
                item.AgeGroup = dto.AgeGroup;
            if (item.ItemStyle != dto.ItemStyle)
                item.ItemStyle = dto.ItemStyle;
            if (item.ShippingSizeValue != dto.ShippingSizeValue)
                item.ShippingSizeValue = dto.ShippingSizeValue;
            if (item.PackageLength != dto.PackageLength)
                item.PackageLength = dto.PackageLength;
            if (item.PackageWidth != dto.PackageWidth)
                item.PackageWidth = dto.PackageWidth;
            if (item.PackageHeight != dto.PackageHeight)
                item.PackageHeight = dto.PackageHeight;
            if (item.InternationalPackageValue != dto.InternationalPackageValue)
                item.InternationalPackageValue = dto.InternationalPackageValue;
            if (item.ExcessiveShipmentValue != dto.ExcessiveShipmentValue)
                item.ExcessiveShipmentValue = dto.ExcessiveShipmentValue;
            if (item.HolidayValue != dto.HolidayValue)
                item.HolidayValue = dto.HolidayValue;

            if (item.LastSoldDateOnMarket != dto.LastSoldDateOnMarket)
                item.LastSoldDateOnMarket = dto.LastSoldDateOnMarket;

            if (item.MarketplacesInfo != dto.MarketplacesInfo)
                item.MarketplacesInfo = dto.MarketplacesInfo;

            if (item.AssociatedASIN != dto.AssociatedASIN)
                item.AssociatedASIN = dto.AssociatedASIN;

            if (item.AssociatedSourceMarketId != dto.AssociatedSourceMarketId)
                item.AssociatedSourceMarketId = dto.AssociatedSourceMarketId;

            if (item.AssociatedMarket != dto.AssociatedMarket)
                item.AssociatedMarket = dto.AssociatedMarket;

            if (item.AssociatedMarketplaceId != dto.AssociatedMarketplaceId)
                item.AssociatedMarketplaceId = dto.AssociatedMarketplaceId;


            if (item.IsDirty != false)
                item.IsDirty = false;
            //if (item.UpdateDate != dto.UpdateDate)
            if (item.UpdateDate == DateTime.MinValue)
                item.UpdateDate = dto.UpdateDate;
        }

        public IQueryable<StyleCacheDTO> GetAllCacheItems()
        {
            return unitOfWork.GetSet<StyleCache>()
                .Select(c => new StyleCacheDTO()
                {
                    Id = c.Id,
                    MainLicense = c.MainLicense,
                    SubLicense = c.SubLicense,
                    Gender = c.Gender,
                    AgeGroup = c.AgeGroup,
                    ItemStyle = c.ItemStyle,
                    ShippingSizeValue = c.ShippingSizeValue,
                    PackageLength = c.PackageLength,
                    PackageWidth = c.PackageWidth,
                    PackageHeight = c.PackageHeight,
                    InternationalPackageValue = c.InternationalPackageValue,
                    ExcessiveShipmentValue = c.ExcessiveShipmentValue,
                    MarketplacesInfo = c.MarketplacesInfo,

                    AssociatedASIN = c.AssociatedASIN,
                    AssociatedSourceMarketId = c.AssociatedSourceMarketId,
                    AssociatedMarket = c.AssociatedMarket,
                    AssociatedMarketplaceId = c.AssociatedMarketplaceId,

                    LastSoldDateOnMarket = c.LastSoldDateOnMarket,

                    IsDirty = c.IsDirty,
                    UpdateDate = c.UpdateDate
                });
        }
    }
}
